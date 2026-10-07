"""AST-only installer config transactions on retained, unique owned D fixtures."""
import base64
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest

INSTALLER = Path(__file__).resolve().parents[1] / 'install.ps1'
BASE = Path(r'D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\installer-config-repair-fixtures')


@unittest.skipUnless(os.name == 'nt', 'Windows Framework file replacement contract')
class InstallerConfigWriterTests(unittest.TestCase):
    def run_case(self, case, attributes=32):
        BASE.mkdir(parents=True, exist_ok=True)
        root = Path(tempfile.mkdtemp(prefix='owned-', dir=BASE))
        script = r'''
$ErrorActionPreference='Stop'
Import-Module (Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/Modules/Microsoft.PowerShell.Utility/Microsoft.PowerShell.Utility.psd1') -Force
$tokens=$null;$errors=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile('INSTALLER',[ref]$tokens,[ref]$errors)
if($errors){throw 'Installer parse failed'}
$names=@('Assert-ExistingConfigWritable','Get-ConfigSnapshot','New-ConfigWritePlan','Complete-ConfigWritePlan','Restore-ConfigBackup','ConvertTo-ConfigBytes')
foreach($name in $names){
 $fn=$ast.Find({param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq $name},$true)
 if(!$fn){throw ('Missing config helper: '+$name)}
 Invoke-Expression $fn.Extent.Text
}
$root='ROOT';$path=Join-Path $root 'owned-config.json';$case='CASE';$attrs=ATTRS
if($case -eq 'missing-parent'){$path=Join-Path $root 'new-owned-directory/config.json'}
$utf8=New-Object Text.UTF8Encoding($false)
function Snapshot($p){
 if(![IO.File]::Exists($p)){return @{exists=$false}}
 $sections=[Security.AccessControl.AccessControlSections]::Access -bor [Security.AccessControl.AccessControlSections]::Owner -bor [Security.AccessControl.AccessControlSections]::Group
 return @{exists=$true;bytes=[Convert]::ToBase64String([IO.File]::ReadAllBytes($p));attributes=[int][IO.File]::GetAttributes($p);acl=[IO.File]::GetAccessControl($p,$sections).GetSecurityDescriptorSddlForm($sections)}
}
function Equal($a,$b){($a|ConvertTo-Json -Compress) -eq ($b|ConvertTo-Json -Compress)}
function Require($condition,$message){if(!$condition){throw $message}}
if($case -ne 'new-repeat' -and $case -ne 'missing-parent'){
 [IO.File]::WriteAllText($path,'{"deviceId":"owned","unknown":{"keep":[1,2,"한글"]}}',$utf8)
 [IO.File]::SetAttributes($path,[IO.FileAttributes]$attrs)
 if($case -eq 'acl-denied'){
  # Only this newly-created owned fixture receives an explicit denial.
  $acl=[IO.File]::GetAccessControl($path);$sid=[Security.Principal.WindowsIdentity]::GetCurrent().User
  $rule=New-Object Security.AccessControl.FileSystemAccessRule($sid,([Security.AccessControl.FileSystemRights]::WriteData -bor [Security.AccessControl.FileSystemRights]::AppendData),[Security.AccessControl.AccessControlType]::Deny)
  $acl.AddAccessRule($rule);[IO.File]::SetAccessControl($path,$acl)
 }
}
$before=Snapshot $path;$problem='';$hold=$null
try{
 if($case -eq 'readonly' -or $case -eq 'locked' -or $case -eq 'directory' -or $case -eq 'acl-denied'){
  if($case -eq 'locked'){$hold=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)}
  if($case -eq 'directory'){$path=Join-Path $root 'directory.json';[IO.Directory]::CreateDirectory($path)|Out-Null}
  $blocked=$false;try{$plan=New-ConfigWritePlan $path $utf8.GetBytes('{"new":true}')}catch{$blocked=$true;$problem=$_.Exception.Message}
  Require $blocked 'Invalid/denied config was accepted'
  if($case -ne 'directory'){Require (Equal $before (Snapshot $path)) 'Denial changed original metadata or bytes'}
 }elseif($case -eq 'initial-read-changed'){
  $initial=Get-ConfigSnapshot $path
  $s=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Write,[IO.FileShare]::Read)
  try{$b=$utf8.GetBytes('{"outside":true}');$s.Write($b,0,$b.Length);$s.SetLength($b.Length)}finally{$s.Dispose()}
  $expected=Snapshot $path;$blocked=$false
  try{$plan=New-ConfigWritePlan $path $utf8.GetBytes('{"stale":true}') $initial}catch{$blocked=$true;$problem=$_.Exception.Message}
  Require $blocked 'Initial-read change was accepted'
  Require (Equal $expected (Snapshot $path)) 'Stale plan changed original'
 }elseif($case -eq 'changed' -or $case -eq 'commit-locked'){
  $plan=New-ConfigWritePlan $path $utf8.GetBytes('{"new":true}')
  Require (Equal $before (Snapshot $path)) 'Preparation changed original'
  if($case -eq 'changed'){$s=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Write,[IO.FileShare]::Read);try{$b=$utf8.GetBytes('{"outside":true}');$s.Write($b,0,$b.Length);$s.SetLength($b.Length)}finally{$s.Dispose()}}
  else{$hold=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)}
  $expected=Snapshot $path;$blocked=$false
  try{Complete-ConfigWritePlan $plan}catch{$blocked=$true;$problem=$_.Exception.Message}
  Require $blocked 'Changed/locked commit was accepted'
  Require (Equal $expected (Snapshot $path)) 'Failed commit changed original'
 }elseif($case -eq 'nested' -or $case -eq 'too-deep'){
  if($case -eq 'too-deep'){
   $obj=@{leaf='preserve'};for($i=0;$i -lt 95;$i++){$obj=@{nested=$obj}}
   $blocked=$false;try{$bytes=ConvertTo-ConfigBytes @{unknown=$obj}}catch{$blocked=$true;$problem=$_.Exception.Message}
   Require $blocked 'Unsupported nesting was silently truncated'
  }else{
  $obj=@{leaf='preserve'};for($i=0;$i -lt 20;$i++){$obj=@{nested=$obj}}
  $data=@{deviceId='owned';unknown=$obj};$bytes=ConvertTo-ConfigBytes $data
  $round=$utf8.GetString($bytes)|ConvertFrom-Json;$n=$round.unknown;for($i=0;$i -lt 20;$i++){$n=$n.nested}
  Require ($n.leaf -eq 'preserve') 'Unknown nested value was lost'
  }
 }else{
  foreach($value in @('{"deviceId":"owned","schemaVersion":3,"unknown":{"keep":[1,2,"한글"]}}','{"deviceId":"owned","schemaVersion":3,"uiHelper":"new","unknown":{"keep":[1,2,"한글"]}}')){
   $plan=New-ConfigWritePlan $path $utf8.GetBytes($value)
   Require (Equal $before (Snapshot $path)) 'Preparation changed original'
   if($case -eq 'missing-parent' -and !$before.exists){
    $parent=Split-Path -Parent $path;Require (![IO.Directory]::Exists($parent)) 'Preparation created installation directory'
    [IO.Directory]::CreateDirectory($parent)|Out-Null
   }
   Complete-ConfigWritePlan $plan
   $after=Snapshot $path
   Require ([IO.File]::ReadAllText($path,$utf8) -eq $value) 'Config content not updated'
   if($before.exists){Require ($before.attributes -eq $after.attributes -and $before.acl -eq $after.acl) 'Metadata changed'}
   if($case -eq 'rollback'){
    Restore-ConfigBackup $path $plan.Backup
    Require (Equal $before (Snapshot $path)) 'Rollback did not restore exact config'
   }else{$before=$after}
  }
 }
 [IO.File]::WriteAllText((Join-Path $root 'result.json'),(@{case=$case;success=$true;error=$problem;scope='AST config helpers only; no top-level installer'}|ConvertTo-Json),$utf8)
}finally{if($hold){$hold.Dispose()}}
'''
        for old, new in [('INSTALLER', str(INSTALLER)), ('ROOT', str(root)), ('CASE', case), ('ATTRS', str(attributes))]:
            script = script.replace(old, new.replace("'", "''"))
        command = base64.b64encode(script.encode('utf-16le')).decode('ascii')
        result = subprocess.run(['powershell.exe', '-NoProfile', '-NonInteractive', '-EncodedCommand', command], capture_output=True, text=True, encoding='utf-8', errors='replace', timeout=30, creationflags=subprocess.CREATE_NO_WINDOW)
        (root / 'process-output.txt').write_text(result.stdout + result.stderr, encoding='utf-8')
        self.assertEqual(result.returncode, 0, str(root) + '\n' + result.stdout + result.stderr)
        self.assertTrue(json.loads((root / 'result.json').read_text(encoding='utf-8-sig'))['success'])

    def test_hidden_system_updates_preserve_attributes_acl_and_unknown_fields(self):
        for attrs in (32, 2 | 32, 4 | 32, 2 | 4 | 32):
            with self.subTest(attributes=attrs):
                self.run_case('update-repeat', attrs)

    def test_new_config_then_repeated_updates(self):
        self.run_case('new-repeat')

    def test_new_config_plan_does_not_create_missing_install_directory(self):
        self.run_case('missing-parent')

    def test_rollback_restores_hidden_original_exactly(self):
        self.run_case('rollback', 2 | 32)

    def test_readonly_denial_does_not_modify_original(self):
        self.run_case('readonly', 1 | 2 | 32)

    def test_sharing_denial_does_not_modify_original(self):
        self.run_case('locked', 2 | 32)

    def test_owned_acl_write_denial_does_not_modify_original(self):
        self.run_case('acl-denied', 2 | 32)

    def test_directory_is_rejected(self):
        self.run_case('directory')

    def test_changed_original_is_not_overwritten(self):
        self.run_case('changed', 2 | 32)

    def test_change_after_initial_read_is_rejected_before_staging(self):
        self.run_case('initial-read-changed', 2 | 32)

    def test_commit_sharing_denial_does_not_modify_original(self):
        self.run_case('commit-locked', 2 | 32)

    def test_unknown_nested_values_survive_serialization(self):
        self.run_case('nested')

    def test_excessive_nesting_is_rejected_instead_of_truncated(self):
        self.run_case('too-deep')


if __name__ == '__main__':
    unittest.main()
