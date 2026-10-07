"""Run only the AST-extracted config guard on fresh, retained D fixtures; never install."""
import base64
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest

INSTALLER = Path(__file__).resolve().parents[1] / 'install.ps1'
FIXTURE_BASE = Path(r'D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\installer-preflight-tests')
CONFLICT = 'config_writealltext_attribute_conflict_before_install'


class InstallerPreflightTests(unittest.TestCase):
    def run_guard(self, attributes=None, missing=False):
        if os.name != 'nt':
            self.skipTest('WinPS/.NET Framework file attribute contract')
        FIXTURE_BASE.mkdir(parents=True, exist_ok=True)
        root = Path(tempfile.mkdtemp(prefix='owned-', dir=FIXTURE_BASE))
        script = r'''
$ErrorActionPreference='Stop'
Import-Module (Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/Modules/Microsoft.PowerShell.Utility/Microsoft.PowerShell.Utility.psd1') -Force
$tokens=$null;$errors=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile('INSTALLER',[ref]$tokens,[ref]$errors)
if($errors){throw 'Installer parse failed'}
$function=$ast.Find({param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Assert-ExistingConfigWritable'},$true)
if(!$function){throw 'Config preflight function missing'}
# Only this function is evaluated. Top-level installer statements are never invoked.
Invoke-Expression $function.Extent.Text
$root='FIXTURE';$path=Join-Path $root 'owned-config.json'
function Snapshot([string]$p){
 if(![IO.File]::Exists($p)){return @{exists=$false}}
 $bytes=[IO.File]::ReadAllBytes($p);$sha=[Security.Cryptography.SHA256]::Create()
 try{$hash=[BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-','').ToLowerInvariant()}finally{$sha.Dispose()}
 $sections=[Security.AccessControl.AccessControlSections]::Access -bor [Security.AccessControl.AccessControlSections]::Owner -bor [Security.AccessControl.AccessControlSections]::Group
 $acl=[IO.File]::GetAccessControl($p,$sections)
 return @{exists=$true;bytesBase64=[Convert]::ToBase64String($bytes);sha256=$hash;attributes=[int][IO.File]::GetAttributes($p);aclSddl=$acl.GetSecurityDescriptorSddlForm($sections)}
}
if(!MISSING){
 [IO.File]::WriteAllText($path,'{"ownedFixture":true,"content":"must remain unchanged"}',(New-Object Text.UTF8Encoding($false)))
 [IO.File]::SetAttributes($path,[IO.FileAttributes]ATTRIBUTES)
}
$before=Snapshot $path;$success=$false;$errorMessage='';$exceptionType='';$hresult=''
try{Assert-ExistingConfigWritable $path;$success=$true}catch{$errorMessage=$_.Exception.Message;$exceptionType=$_.Exception.GetType().FullName;$hresult=('0x{0:X8}' -f [int]$_.Exception.HResult)}
$after=Snapshot $path
$report=@{success=$success;error=$errorMessage;exceptionType=$exceptionType;hResult=$hresult;before=$before;after=$after;runtime=$PSVersionTable.PSVersion.ToString();clr=[Environment]::Version.ToString();scope='AST helper only; retained new D fixture; no full installer or operational file'}
[IO.File]::WriteAllText((Join-Path $root 'result.json'),($report|ConvertTo-Json -Depth 8),(New-Object Text.UTF8Encoding($false)))
'''
        for old, new in [('INSTALLER', str(INSTALLER).replace("'", "''")), ('FIXTURE', str(root).replace("'", "''")), ('MISSING', '$true' if missing else '$false'), ('ATTRIBUTES', str(attributes or 128))]:
            script = script.replace(old, new)
        command = base64.b64encode(script.encode('utf-16le')).decode('ascii')
        result = subprocess.run(['powershell.exe', '-NoProfile', '-NonInteractive', '-EncodedCommand', command], capture_output=True, text=True, encoding='utf-8', errors='replace', timeout=25, creationflags=subprocess.CREATE_NO_WINDOW)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        report = json.loads((root / 'result.json').read_text(encoding='utf-8-sig'))
        self.assertEqual(report['before'], report['after'], 'Guard changed bytes, attributes or ACL')
        return report

    def assert_conflict(self, attributes, *reasons):
        report = self.run_guard(attributes)
        self.assertFalse(report['success'], 'Hidden/System config must fail closed before installation')
        self.assertIn(CONFLICT, report['error'])
        for reason in reasons:
            self.assertIn(reason, report['error'])

    def test_hidden_existing_config_is_rejected_without_modification(self):
        self.assert_conflict(2 | 32, 'Hidden')

    def test_system_existing_config_is_rejected_without_modification(self):
        self.assert_conflict(4 | 32, 'System')

    def test_hidden_system_existing_config_is_rejected_without_modification(self):
        self.assert_conflict(2 | 4 | 32, 'Hidden', 'System')

    def test_normal_config_preflight_succeeds_without_writing(self):
        self.assertTrue(self.run_guard(128)['success'])

    def test_new_config_is_not_created_by_preflight(self):
        report = self.run_guard(missing=True)
        self.assertTrue(report['success'])
        self.assertFalse(report['after']['exists'])

    def test_existing_readwrite_access_denial_still_fails(self):
        report = self.run_guard(1 | 32)
        self.assertFalse(report['success'])
        self.assertIn('config_not_writable_before_install', report['error'])

    def test_both_preflight_calls_precede_installation_mutation_and_guard_never_bypasses(self):
        text = INSTALLER.read_text(encoding='utf-8-sig')
        first = text.index(' Assert-ExistingConfigWritable $configPath')
        last = text.rindex(' Assert-ExistingConfigWritable $configPath')
        self.assertLess(first, text.index('Copy-Item -LiteralPath $manifestFile'))
        self.assertLess(last, text.index('New-Item -ItemType Directory -Force -Path $target,$statePath,$runtimePath'))
        self.assertLess(last, text.index('$installTouched=$true'))
        self.assertLess(last, text.index("[IO.File]::WriteAllText((Join-Path $statePath 'stop.flag')"))
        self.assertLess(last, text.index('Stop-Process -Id'))
        body = text.split('function Assert-ExistingConfigWritable(', 1)[1].split('function Invoke-HiddenTool(', 1)[0]
        self.assertIn(CONFLICT, body)
        self.assertLess(body.index('[IO.FileAttributes]::Hidden'), body.index('[IO.File]::Open('))
        self.assertIn('[IO.FileAttributes]::System', body)
        for forbidden in ['WriteAllText(', '.Write(', '.SetLength(', 'SetAttributes(', 'Set-Acl', 'Start-Process', 'Stop-Process']:
            self.assertNotIn(forbidden, body)


if __name__ == '__main__':
    unittest.main()
