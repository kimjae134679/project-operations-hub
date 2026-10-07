"""Isolated PowerShell orchestration tests: no live process or application launch."""
import json
import hashlib
from pathlib import Path
import subprocess
import shutil
import unittest
import uuid


SCRIPT = Path(__file__).with_name('switch_control_version.ps1')
CHECKS = Path(r'D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks')

HARNESS = r'''
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$tokens=$null;$errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile($args[0],[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'script_parse_failed'}
foreach($node in $ast.EndBlock.Statements){
    if($node -is [Management.Automation.Language.FunctionDefinitionAst]){
        . ([scriptblock]::Create($node.Extent.Text))
    }
}
$case=$args[1]
$script:trace=[Collections.Generic.List[string]]::new()
$script:step=0;$script:clock=[DateTimeOffset]::Parse('2026-10-07T00:00:00Z')
$script:statuses=[Collections.Generic.List[object]]::new()
$versions='D:\A_KJ\AI\Applications\AIControlTower\versions'
$old=Join-Path $versions '0.9.6-20261007\AIControlTower.exe'
$new=Join-Path $versions '0.9.7-20261007-parallel'
$r=[pscustomobject]@{
    CurrentExecutable=$old;ExpectedCurrentSha256='2fbc5b312f114cbc60a064cf5caa167251e3ac63bb226522c5a33fa18a82443d';VersionRoot=$new
    ExpectedVersion='0.9.7';ExpectedSourceCommit=('b'*40);ExpectedSha256=('c'*64)
    ExpectedGuardSha256='a608ac3553310edf211f2ee311a14dca84d4e5a416db02f3f503a04464652730';CurrentMode='manual-control'
    StatusPath=Join-Path $args[2] 'status.json';WaitSeconds=3;PollSeconds=1;CheckOnly=$false
}
function Assert-SwitchNoReparse([string]$Path) {}
function Get-SwitchFileIdentity([string]$Path){
    if($case -eq 'old_hash'){return [pscustomobject]@{Version='0.9.6';Hash=('d'*64);ProductVersion='0.9.6+84840a130d8a3fb6911b50fe528ab3709e1c76e1'}}
    if($case -eq 'old_version'){return [pscustomobject]@{Version='0.9.5';Hash=$r.ExpectedCurrentSha256;ProductVersion='0.9.6+84840a130d8a3fb6911b50fe528ab3709e1c76e1'}}
    return [pscustomobject]@{Version='0.9.6';Hash=$r.ExpectedCurrentSha256;ProductVersion='0.9.6+84840a130d8a3fb6911b50fe528ab3709e1c76e1'}
}
function Open-SwitchLease {
    $script:trace.Add('lease')
    if($case -eq 'duplicate'){return $null}
    return [pscustomobject]@{Owned=$true}
}
function Close-SwitchLease($Lease){$script:trace.Add('release')}
function Get-SwitchClock {return $script:clock}
function Wait-SwitchPoll([int]$Seconds){$script:clock=$script:clock.AddSeconds($Seconds)}
function Write-SwitchStatus($Request,$Outcome){
    if($case -eq 'status_write_failure' -and $Outcome.status -eq 'started'){throw 'fixture_status_write_failed'}
    $script:statuses.Add($Outcome)
}
function Invoke-SwitchGuard($Request,[bool]$CheckOnly){
    $script:trace.Add($(if($CheckOnly){'check'}else{'launch'}))
    if($case -eq 'preflight_fail' -and $CheckOnly){return [pscustomobject]@{ExitCode=2;status='held';reason='descriptor_expired';started=$false}}
    if($case -eq 'recheck_fail' -and $CheckOnly -and @($script:trace | Where-Object {$_ -eq 'check'}).Count -eq 2){return [pscustomobject]@{ExitCode=2;status='held';reason='executable_mismatch';started=$false}}
    if($case -eq 'launch_fail' -and !$CheckOnly){return [pscustomobject]@{ExitCode=3;status='held';reason='manual_instance_running';started=$false}}
    if($case -eq 'launch_uncertain' -and !$CheckOnly){throw 'fixture_guard_completion_unknown'}
    return [pscustomobject]@{ExitCode=0;status=$(if($CheckOnly){'verified'}else{'started'});reason='ok';started=(!$CheckOnly)}
}
function Get-SwitchProcesses {
    $script:trace.Add('processes');$script:step++
    $supervisor=[pscustomobject]@{ProcessId=5996;ExecutablePath='D:\A_KJ\AI\Applications\AIControlTower\AIControlTower.exe';CommandLine='"D:\A_KJ\AI\Applications\AIControlTower\AIControlTower.exe" --remote-supervisor';CreationDate=[datetime]'2026-10-06T01:00:00Z';SessionId=1}
    $manual=[pscustomobject]@{ProcessId=101;ExecutablePath=$old;CommandLine=('"'+$old+'" --manual-control');CreationDate=[datetime]'2026-10-07T01:00:00Z';SessionId=1}
    if($case -eq 'unknown'){$supervisor.CommandLine='"x" --mystery'}
    if($case -eq 'missing_metadata'){$supervisor.ExecutablePath=$null}
    if($case -eq 'other_manual'){$manual.ExecutablePath=Join-Path $versions '0.9.6-other\AIControlTower.exe';$manual.CommandLine='"x" --manual-control'}
    if($case -eq 'pid_reused' -and $script:step -gt 1){$manual.CreationDate=[datetime]'2026-10-07T02:00:00Z'}
    if($case -eq 'different_session'){$manual.SessionId=2}
    if($case -eq 'application_exit'){$manual.CommandLine=('"'+$old+'"')}
    if($case -eq 'changed_mode' -and $script:step -gt 1){$manual.CommandLine='"x" --background'}
    if($case -eq 'duplicate_old'){return @($supervisor,$manual,$manual)}
    if($case -in @('timeout','pid_reused','changed_mode','unknown','missing_metadata','other_manual','different_session') -or ($case -ne 'already_gone' -and $script:step -le 2)){return @($supervisor,$manual)}
    return @($supervisor)
}
function Get-SwitchSessionId {return 1}
switch($case){
    'bad_old_root' {$r.CurrentExecutable='C:\unapproved\AIControlTower.exe'}
    'same_path' {$r.VersionRoot=[IO.Path]::GetDirectoryName($r.CurrentExecutable)}
    'bad_target_version' {$r.ExpectedVersion='0.9.8'}
    'bad_guard_hash' {$r.ExpectedGuardSha256=('d'*64)}
    'bad_status' {$r.StatusPath='C:\unapproved\status.json'}
    'too_long' {$r.WaitSeconds=1801}
    'check_only' {$r.CheckOnly=$true}
    'application_exit' {$r.CurrentMode='application'}
}
if($case -eq 'real_file_hash'){
    $real=$ast.EndBlock.Statements | Where-Object {$_ -is [Management.Automation.Language.FunctionDefinitionAst] -and $_.Name -eq 'Get-SwitchFileIdentity'}
    . ([scriptblock]::Create($real.Extent.Text))
    function Get-FileHash {throw 'fixture_cmdlet_unavailable'}
    $file=Join-Path $args[2] 'identity.bin'
    [IO.File]::WriteAllBytes($file,[Text.Encoding]::UTF8.GetBytes('owned test fixture'))
    Get-SwitchFileIdentity $file | ConvertTo-Json -Compress
    exit 0
}
$result=Invoke-ControlVersionSwitch $r
[pscustomobject]@{result=$result;trace=@($script:trace.ToArray());statuses=@($script:statuses.ToArray())}|ConvertTo-Json -Depth 8 -Compress
'''


class SwitchControlVersionTests(unittest.TestCase):
    def setUp(self):
        self.assertTrue(SCRIPT.exists(), 'safe wait-and-launch replacement utility is missing')
        self.root = CHECKS / ('version-switch-unit-' + uuid.uuid4().hex)
        self.root.mkdir(parents=True)
        self.harness = self.root / 'harness.ps1'
        self.harness.write_text(HARNESS, encoding='utf-8')

    def tearDown(self):
        if hasattr(self, 'root'):
            resolved = self.root.resolve()
            self.assertEqual(CHECKS.resolve(), resolved.parent)
            shutil.rmtree(resolved)

    def run_case(self, case):
        child = subprocess.run(['powershell.exe', '-NoLogo', '-NoProfile', '-NonInteractive',
                                '-File', str(self.harness),
                                str(SCRIPT), case, str(self.root)],
                               capture_output=True, text=True, timeout=20)
        self.assertEqual(0, child.returncode, child.stderr)
        self.assertEqual('', child.stderr)
        return json.loads(child.stdout)

    def test_delayed_exact_old_exit_launches_once_after_fresh_verification(self):
        out = self.run_case('delayed_exit')
        self.assertEqual('started', out['result']['status'])
        self.assertFalse(out['result']['operatingConfirmed'])
        self.assertEqual(2, out['trace'].count('check'))
        self.assertEqual(1, out['trace'].count('launch'))
        self.assertEqual('release', out['trace'][-1])
        self.assertTrue(any(s['reason'] == 'manual_exit_required' for s in out['statuses']))
        self.assertLess(out['trace'].index('processes'), out['trace'].index('launch'))

    def test_timeout_preserves_old_instance_and_requests_normal_tray_exit(self):
        out = self.run_case('timeout')
        self.assertEqual('wait_timeout_manual_exit_required', out['result']['reason'])
        self.assertTrue(out['result']['requiresUserExit'])
        self.assertNotIn('launch', out['trace'])
        self.assertEqual(1, out['trace'].count('check'))

    def test_already_gone_can_launch_without_signalling(self):
        out = self.run_case('already_gone')
        self.assertEqual('started', out['result']['status'])
        self.assertEqual(1, out['trace'].count('launch'))

    def test_normal_application_waits_for_normal_close_not_tray_exit(self):
        out = self.run_case('application_exit')
        self.assertEqual('started', out['result']['status'])
        self.assertTrue(any(s['reason'] == 'application_exit_required' for s in out['statuses']))

    def test_failed_guard_or_launch_is_never_retried(self):
        for case, checks, launches in [('preflight_fail', 1, 0), ('recheck_fail', 2, 0), ('launch_fail', 2, 1)]:
            with self.subTest(case=case):
                out = self.run_case(case)
                self.assertEqual('held', out['result']['status'])
                self.assertEqual(checks, out['trace'].count('check'))
                self.assertEqual(launches, out['trace'].count('launch'))

    def test_unknown_or_changed_live_identity_blocks_launch(self):
        for case in ['unknown', 'missing_metadata', 'other_manual', 'pid_reused',
                     'different_session', 'changed_mode', 'duplicate_old']:
            with self.subTest(case=case):
                out = self.run_case(case)
                self.assertEqual('held', out['result']['status'])
                self.assertNotIn('launch', out['trace'])

    def test_invalid_request_never_probes_processes_or_launches(self):
        for case in ['old_hash', 'old_version', 'bad_old_root', 'same_path', 'bad_target_version',
                     'bad_guard_hash', 'bad_status', 'too_long']:
            with self.subTest(case=case):
                out = self.run_case(case)
                self.assertEqual('held', out['result']['status'])
                self.assertNotIn('processes', out['trace'])
                self.assertNotIn('launch', out['trace'])

    def test_duplicate_waiter_never_checks_or_probes(self):
        out = self.run_case('duplicate')
        self.assertEqual('replacement_waiter_running', out['result']['reason'])
        self.assertEqual(['lease'], out['trace'])

    def test_check_only_verifies_package_without_process_probe_or_launch(self):
        out = self.run_case('check_only')
        self.assertEqual('verified', out['result']['status'])
        self.assertFalse(out['result']['started'])
        self.assertNotIn('processes', out['trace'])
        self.assertNotIn('launch', out['trace'])

    def test_status_write_failure_cannot_erase_observed_launch(self):
        out = self.run_case('status_write_failure')
        self.assertEqual('started', out['result']['status'])
        self.assertTrue(out['result']['started'])
        self.assertFalse(out['result']['operatingConfirmed'])
        self.assertEqual(1, out['trace'].count('launch'))

    def test_uncertain_guard_completion_does_not_claim_no_launch_or_retry(self):
        out = self.run_case('launch_uncertain')
        self.assertEqual('outcome_unknown', out['result']['status'])
        self.assertIsNone(out['result']['started'])
        self.assertFalse(out['result']['operatingConfirmed'])
        self.assertEqual(1, out['trace'].count('launch'))

    def test_real_file_identity_does_not_depend_on_unavailable_hash_cmdlet(self):
        out = self.run_case('real_file_hash')
        self.assertEqual(hashlib.sha256(b'owned test fixture').hexdigest(), out['Hash'].lower())

    def test_production_surface_has_no_shutdown_ui_or_arbitrary_execution_seams(self):
        source = SCRIPT.read_text(encoding='utf-8').lower()
        for forbidden in ['stop-process', '.kill(', 'closemainwindow', 'mainwindowhandle',
                          'getwindow', 'sendmessage', 'postmessage', 'setforegroundwindow',
                          '--remote-control', 'register-scheduledtask', 'fixturepath',
                          'scriptblock]$', 'invoke-expression']:
            self.assertNotIn(forbidden, source)
        self.assertIn("name='aicontroltower.exe'", source)
        self.assertIn("'start_manual_control.ps1'", source)
        self.assertIn('--manual-control', source)


if __name__ == '__main__':
    unittest.main()
