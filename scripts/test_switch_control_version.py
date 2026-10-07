"""Isolated PowerShell orchestration tests: no live process or application launch."""
import json
import hashlib
import os
from pathlib import Path
import subprocess
import shutil
import sys
import time
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
if($case -eq 'capability_actual_table'){
    # The real AST-loaded function runs before any orchestration mock is defined.
    # This is a pure identity-table query: no file, process or IPC boundary runs.
    $commit='5b0f3296d259ce03882e16eba1d8f93604af570f'
    $hash='2615d4a88f53410d963403cbb4dafabd7f6abf003c3a95737d4138b362cb244f'
    $cases=@(
        @{name='exact';version='0.9.7';commit=$commit;hash=$hash},
        @{name='wrong_version';version='0.9.6';commit=$commit;hash=$hash},
        @{name='wrong_source';version='0.9.7';commit=('0'*40);hash=$hash},
        @{name='wrong_sha';version='0.9.7';commit=$commit;hash=('0'*64)},
        @{name='legacy_475';version='0.9.7';commit='475a0fcd6f9ec3f27d35f79cb1b90b7b5831b0a7';hash='128475e91b2ff8a7f6cb1f4e34fa26353d625eedff61c462151d071fa01458cc'}
    )
    $results=[ordered]@{}
    foreach($candidate in $cases){
        $request=[pscustomobject]@{ExpectedCurrentVersion=$candidate.version;ExpectedCurrentSourceCommit=$candidate.commit;ExpectedCurrentSha256=$candidate.hash}
        $results[$candidate.name]=Test-SwitchExitCapability $request
    }
    $results | ConvertTo-Json -Compress
    return
}
$script:trace=[Collections.Generic.List[string]]::new()
$script:step=0;$script:clock=[DateTimeOffset]::Parse('2026-10-07T00:00:00Z')
$script:statuses=[Collections.Generic.List[object]]::new()
$versions='D:\A_KJ\AI\Applications\AIControlTower\versions'
$old=Join-Path $versions '0.9.6-20261007\AIControlTower.exe'
$new=Join-Path $versions '0.9.7-20261007-parallel'
$r=[pscustomobject]@{
    CurrentExecutable=$old;ExpectedCurrentSha256='2fbc5b312f114cbc60a064cf5caa167251e3ac63bb226522c5a33fa18a82443d';VersionRoot=$new
    ExpectedCurrentVersion='0.9.6';ExpectedCurrentSourceCommit='84840a130d8a3fb6911b50fe528ab3709e1c76e1'
    ExpectedVersion='0.9.7';ExpectedSourceCommit=('b'*40);ExpectedSha256=('c'*64)
    ExpectedGuardSha256='a608ac3553310edf211f2ee311a14dca84d4e5a416db02f3f503a04464652730';CurrentMode='manual-control'
    StatusPath=Join-Path $args[2] 'status.json';WaitSeconds=3;PollSeconds=1;CheckOnly=$false
}
function Assert-SwitchNoReparse([string]$Path) {}
function Get-SwitchFileIdentity([string]$Path){
    if($case -eq 'old_hash'){return [pscustomobject]@{Version='0.9.6';Hash=('d'*64);ProductVersion='0.9.6+84840a130d8a3fb6911b50fe528ab3709e1c76e1'}}
    if($case -eq 'old_version'){return [pscustomobject]@{Version='0.9.5';Hash=$r.ExpectedCurrentSha256;ProductVersion='0.9.6+84840a130d8a3fb6911b50fe528ab3709e1c76e1'}}
    return [pscustomobject]@{Version=$r.ExpectedCurrentVersion;FileVersion=($r.ExpectedCurrentVersion+'.0');Hash=$r.ExpectedCurrentSha256;ProductVersion=($r.ExpectedCurrentVersion+'+'+$r.ExpectedCurrentSourceCommit)}
}
function Get-SwitchCurrentDeployment($Request){
    $hash=$Request.ExpectedCurrentSha256
    if($case -eq 'ipc_bad_old_descriptor'){$hash='0'*64}
    return [pscustomobject]@{version=$Request.ExpectedCurrentVersion;sourceCommit=$Request.ExpectedCurrentSourceCommit;fileSha256=$hash;fileVersion=($Request.ExpectedCurrentVersion+'.0');productVersion=($Request.ExpectedCurrentVersion+'+'+$Request.ExpectedCurrentSourceCommit)}
}
function Test-SwitchExitCapability($Request){return $case.StartsWith('ipc_') -and $case -notin @('ipc_legacy','ipc_bad_old_descriptor')}
$script:ipcCalls=0
function Invoke-SwitchExitClient($Request,$Deadline){
    $script:ipcCalls++;$script:trace.Add('ipc:'+ $Request.CurrentExecutable)
    $status='graceful_exit_accepted';$code=0
    switch($case){
        'ipc_busy_accept' {if($script:ipcCalls -eq 1){$status='held_jobs';$code=3}}
        'ipc_pending_accept' {if($script:ipcCalls -eq 1){$status='pending_writes';$code=3}}
        'ipc_unknown' {$status='outcome_unknown';$code=6}
        'ipc_transport' {throw 'fixture_transport_unknown'}
        'ipc_inconsistent' {$code=3}
        'ipc_bad_schema' {return [pscustomobject]@{schemaVersion=9;status=$status;reason=$status;exitCode=0;NativeExitCode=0}}
        'ipc_identity_rejected' {$status='identity_rejected';$code=5}
        'ipc_held_unknown' {$status='held_unknown';$code=3}
        'ipc_unsupported' {$status='unsupported';$code=4}
        'ipc_pending_request' {$status='held_request_pending';$code=3}
        'ipc_deadline' {$status='held_jobs';$code=3}
        'ipc_deadline_consumed' {$status='held_jobs';$code=3;$script:clock=$script:clock.AddSeconds(3)}
    }
    return [pscustomobject]@{schemaVersion=1;status=$status;reason=$status;exitCode=$code;NativeExitCode=$code}
}
function Open-SwitchLease {
    $script:trace.Add('lease')
    if($case -eq 'duplicate'){return $null}
    return [pscustomobject]@{Owned=$true}
}
function Close-SwitchLease($Lease){$script:trace.Add('release')}
function Get-SwitchClock {return $script:clock}
function Get-SwitchMonotonicSeconds {return ($script:clock-[DateTimeOffset]::Parse('2026-10-07T00:00:00Z')).TotalSeconds}
function Wait-SwitchPoll([int]$Seconds){$script:trace.Add('poll_wait');$script:clock=$script:clock.AddSeconds($Seconds)}
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
    if($case -eq 'ipc_pid_reused' -and $script:ipcCalls -gt 0){$manual.CreationDate=[datetime]'2026-10-07T02:00:00Z';return @($supervisor,$manual)}
    if($case -in @('ipc_still_alive','ipc_deadline') -or ($case -in @('ipc_busy_accept','ipc_pending_accept') -and $script:ipcCalls -lt 2) -or ($case.StartsWith('ipc_') -and $script:ipcCalls -lt 1 -and $case -ne 'ipc_legacy')){return @($supervisor,$manual)}
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
if($case.StartsWith('ipc_')){
    $old=Join-Path $versions '0.9.7-20261007\AIControlTower.exe'
    $r.CurrentExecutable=$old;$r.ExpectedCurrentVersion='0.9.7';$r.ExpectedCurrentSourceCommit='f'*40;$r.ExpectedCurrentSha256='e'*64
}
if($case -in @('ipc_client_boundary','ipc_client_env_missing','ipc_client_env_reparse')){
    $real=$ast.EndBlock.Statements | Where-Object {$_ -is [Management.Automation.Language.FunctionDefinitionAst] -and $_.Name -eq 'Invoke-SwitchExitClient'}
    . ([scriptblock]::Create($real.Extent.Text))
    function Assert-SwitchRequest($Request){}
    $oldRoot=Join-Path $args[2] 'old-runtime'
    [IO.Directory]::CreateDirectory($oldRoot)|Out-Null
    [IO.Directory]::CreateDirectory((Join-Path $oldRoot 'bundle-extract'))|Out-Null
    if($case -ne 'ipc_client_env_missing'){[IO.Directory]::CreateDirectory((Join-Path $oldRoot 'runtime-temp'))|Out-Null}
    $r.CurrentExecutable=Join-Path $oldRoot 'only-owned-client.exe'
    $r.VersionRoot=Join-Path $args[2] 'target-runtime'
    $originalBundle=$env:DOTNET_BUNDLE_EXTRACT_BASE_DIR;$originalTemp=$env:TEMP;$originalTmp=$env:TMP
    $env:DOTNET_BUNDLE_EXTRACT_BASE_DIR='C:\unapproved-inherited-bundle'
    $env:TEMP='C:\unapproved-inherited-temp';$env:TMP='C:\unapproved-inherited-tmp'
    function Assert-SwitchNoReparse([string]$Path){
        if($case -eq 'ipc_client_env_reparse' -and $Path -eq (Join-Path $oldRoot 'runtime-temp')){throw 'fixture_reparse_rejected'}
    }
    $script:boundary=$null;$script:boundaryCalls=0
    function Read-SwitchChildProtocol($Start){
        $script:boundaryCalls++
        $script:boundary=[pscustomobject]@{file=$Start.FileName;arguments=$Start.Arguments;workingDirectory=$Start.WorkingDirectory;redirectOut=$Start.RedirectStandardOutput;redirectError=$Start.RedirectStandardError;shell=$Start.UseShellExecute;noWindow=$Start.CreateNoWindow;windowStyle=$Start.WindowStyle.ToString();bundle=$Start.EnvironmentVariables['DOTNET_BUNDLE_EXTRACT_BASE_DIR'];temp=$Start.EnvironmentVariables['TEMP'];tmp=$Start.EnvironmentVariables['TMP']}
        return [pscustomobject]@{schemaVersion=1;status='graceful_exit_accepted';reason='graceful_exit_accepted';exitCode=0;NativeExitCode=0}
    }
    $reply=$null;$short=$null;$failed=$false
    try {
        try{$reply=Invoke-SwitchExitClient $r ((Get-SwitchMonotonicSeconds)+25)}catch{$failed=$true}
        if($case -eq 'ipc_client_boundary'){$short=Invoke-SwitchExitClient $r ((Get-SwitchMonotonicSeconds)+19)}
    } finally {
        $env:DOTNET_BUNDLE_EXTRACT_BASE_DIR=$originalBundle;$env:TEMP=$originalTemp;$env:TMP=$originalTmp
    }
    [pscustomobject]@{reply=$reply;short=$short;boundary=$script:boundary;calls=$script:boundaryCalls;failed=$failed;oldRoot=$oldRoot;targetRoot=$r.VersionRoot}|ConvertTo-Json -Depth 5 -Compress
    exit 0
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
if($case -like 'guard_*'){
    $real=$ast.EndBlock.Statements | Where-Object {$_ -is [Management.Automation.Language.FunctionDefinitionAst] -and $_.Name -eq 'Invoke-SwitchGuard'}
    . ([scriptblock]::Create($real.Extent.Text))
    $r.VersionRoot=$args[2]
    $r.ExpectedGuardSha256=Get-SwitchSha256 (Join-Path $r.VersionRoot 'start_manual_control.ps1')
    $watch=[Diagnostics.Stopwatch]::StartNew()
    $response=$null;$failed=$false
    try{$response=Invoke-SwitchGuard $r $false}catch{$failed=$true}
    $done=Join-Path $args[2] ($case+'-done.txt')
    $parentExit=Join-Path $args[2] ($case+'-parent-exit.txt')
    [pscustomobject]@{response=$response;failed=$failed;elapsedMilliseconds=$watch.ElapsedMilliseconds;descendantFinished=[IO.File]::Exists($done);afterParentExitMilliseconds=([DateTime]::UtcNow-[IO.File]::GetLastWriteTimeUtc($parentExit)).TotalMilliseconds}|ConvertTo-Json -Depth 5 -Compress
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
        self.fixture_markers = []

    def tearDown(self):
        if hasattr(self, 'root'):
            # These are only our short-lived fixture children. Let them finish naturally;
            # never terminate an application, enumerate live processes, or remove their root early.
            for ready, done in self.fixture_markers:
                deadline = time.monotonic() + 8
                while ready.exists() and not done.exists() and time.monotonic() < deadline:
                    time.sleep(0.05)
            resolved = self.root.resolve()
            self.assertEqual(CHECKS.resolve(), resolved.parent)
            shutil.rmtree(resolved)

    def run_case(self, case):
        if case.startswith('guard_'):
            self.create_inherited_pipe_fixture(case)
        temp = self.root / 'temp'
        temp.mkdir(exist_ok=True)
        environment = dict(os.environ, TEMP=str(temp), TMP=str(temp))
        stdout_path = self.root / (case + '-harness-stdout.log')
        stderr_path = self.root / (case + '-harness-stderr.log')
        # Files have no EOF dependency on inherited descendant pipe handles.
        # subprocess.run waits for this exact harness parent; keep its original
        # external timeout and the existing natural descendant cleanup policy.
        with stdout_path.open('wb') as stdout, stderr_path.open('wb') as stderr:
            child = subprocess.run(['powershell.exe', '-NoLogo', '-NoProfile', '-NonInteractive',
                                    '-File', str(self.harness),
                                    str(SCRIPT), case, str(self.root)],
                                   stdout=stdout, stderr=stderr,
                                   timeout=35 if case == 'guard_silent_inherited_pipe' else 20,
                                   env=environment)
        output = stdout_path.read_text(encoding='utf-8')
        error = stderr_path.read_text(encoding='utf-8')
        self.assertEqual(0, child.returncode, error)
        self.assertEqual('', error)
        return json.loads(output)

    def create_inherited_pipe_fixture(self, case):
        ready = self.root / (case + '-ready.txt')
        done = self.root / (case + '-done.txt')
        parent_exit = self.root / (case + '-parent-exit.txt')
        descendant = self.root / (case + '-descendant.py')
        hold_seconds = 25 if case == 'guard_silent_inherited_pipe' else 4
        descendant.write_text('from pathlib import Path\nimport time\n'
                              f'Path({str(ready)!r}).write_text("ready")\n'
                              f'time.sleep({hold_seconds})\n'
                              f'Path({str(done)!r}).write_text("done")\n', encoding='utf-8')
        inherited_pipe = case in ('guard_pipe_stdout', 'guard_pipe_stderr', 'guard_silent_inherited_pipe')
        if inherited_pipe:
            self.fixture_markers.append((ready, done))
        # Directly exercise the production guard reader, not the top-level CLI. The
        # arbitrary fixture guard exists only in this AST-loaded test harness.
        quote = lambda value: "'" + str(value).replace("'", "''") + "'"
        guard_lines = [
            'param($VersionRoot,$ExpectedVersion,$ExpectedSourceCommit,$ExpectedSha256,[switch]$CheckOnly)',
            "$ErrorActionPreference='Stop'",
        ]
        if inherited_pipe:
            guard_lines += [
            '$start=New-Object Diagnostics.ProcessStartInfo',
            '$start.FileName=' + quote(sys.executable),
            '$start.Arguments=' + quote('"' + str(descendant) + '"'),
            '$start.UseShellExecute=$false;$start.CreateNoWindow=$true',
            '$start.WindowStyle=[Diagnostics.ProcessWindowStyle]::Hidden',
            # Isolate which inherited pipe is keeping EOF open.
            '$start.RedirectStandardError=$true' if case != 'guard_pipe_stderr' else '$start.RedirectStandardOutput=$true',
            '$owned=[Diagnostics.Process]::Start($start);$owned.Dispose()',
            '$limit=[DateTime]::UtcNow.AddSeconds(3)',
            'while(![IO.File]::Exists(' + quote(ready) + ')){if([DateTime]::UtcNow -ge $limit){throw "fixture_start_timeout"};Start-Sleep -Milliseconds 10}',
            ]
        guard_lines += [
            '[IO.File]::WriteAllText(' + quote(parent_exit) + ',"exiting")',
        ]
        valid_result = '[Console]::Out.WriteLine(\'{"status":"started","reason":"fixture_only","started":true,"instanceProbed":false}\')'
        if case != 'guard_silent_inherited_pipe':
            guard_lines.append(valid_result)
        if case == 'guard_two_json':
            guard_lines.append(valid_result)
        elif case == 'guard_invalid_json':
            guard_lines[-1] = '[Console]::Out.WriteLine("not-json")'
        elif case == 'guard_multiline_json':
            guard_lines[-1] = '[Console]::Out.WriteLine("{`n`"status`":`"started`",`n`"started`":true`n}")'
        elif case == 'guard_excess_stdout':
            guard_lines[-1] = '[Console]::Out.WriteLine(("x"*17000))'
        elif case == 'guard_excess_stderr':
            guard_lines.append('[Console]::Error.WriteLine(("x"*17000))')
        elif case == 'guard_stderr_error':
            guard_lines.append('[Console]::Error.WriteLine("fixture-error")')
        guard_lines.append('exit 7' if case == 'guard_parent_failure' else 'exit 0')
        guard = '\n'.join(guard_lines)
        (self.root / 'start_manual_control.ps1').write_text(guard, encoding='utf-8')

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

    def test_guard_json_result_does_not_wait_for_descendant_stdout_eof(self):
        out = self.run_case('guard_pipe_stdout')
        self.assertEqual('started', out['response']['status'])
        self.assertFalse(out['descendantFinished'], out)

    def test_python_harness_observes_parent_exit_before_inherited_descendant_eof(self):
        out = self.run_case('guard_pipe_stdout')
        parent_exit = self.root / 'guard_pipe_stdout-parent-exit.txt'
        elapsed_after_parent_exit = time.time() - parent_exit.stat().st_mtime
        self.assertEqual('started', out['response']['status'])
        self.assertLess(elapsed_after_parent_exit, 3.0,
                        'The Python harness waited for the unrelated descendant EOF after its exact parent exited.')
        self.assertFalse((self.root / 'guard_pipe_stdout-done.txt').exists())

    def test_guard_json_result_does_not_wait_for_descendant_stderr_eof(self):
        out = self.run_case('guard_pipe_stderr')
        self.assertEqual('started', out['response']['status'])
        self.assertFalse(out['descendantFinished'], out)

    def test_guard_response_rejects_invalid_or_multiple_frames_and_error_output(self):
        for case in ('guard_two_json', 'guard_invalid_json', 'guard_multiline_json',
                     'guard_excess_stdout', 'guard_excess_stderr', 'guard_stderr_error'):
            with self.subTest(case=case):
                out = self.run_case(case)
                self.assertTrue(out['failed'], out)

    def test_guard_parent_failure_cannot_be_a_successful_launch(self):
        out = self.run_case('guard_parent_failure')
        self.assertTrue(out['failed'] or out['response']['ExitCode'] != 0, out)

    def test_entire_guard_response_read_is_bounded_without_waiting_inherited_eof(self):
        out = self.run_case('guard_silent_inherited_pipe')
        self.assertTrue(out['failed'], out)
        self.assertLess(out['elapsedMilliseconds'], 22000, out)

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

    def ipc_calls(self, out):
        return [item for item in out['trace'] if item.startswith('ipc:')]

    def test_legacy_097_is_wait_only_without_invoking_unknown_cli_flags(self):
        out = self.run_case('ipc_legacy')
        self.assertEqual('started', out['result']['status'])
        self.assertEqual([], self.ipc_calls(out))

    def test_actual_capability_table_admits_only_verified_c1_identity(self):
        out = self.run_case('capability_actual_table')
        self.assertEqual({'exact': True, 'wrong_version': False,
                          'wrong_source': False, 'wrong_sha': False,
                          'legacy_475': False}, out)

    def test_current_097_requires_exact_fixed_deployment_evidence(self):
        out = self.run_case('ipc_bad_old_descriptor')
        self.assertEqual('held', out['result']['status'])
        self.assertNotIn('processes', out['trace'])
        self.assertEqual([], self.ipc_calls(out))

    def test_capable_client_is_exact_old_executable_then_actual_exit_is_verified(self):
        out = self.run_case('ipc_accepted')
        self.assertEqual('started', out['result']['status'])
        self.assertEqual([r'ipc:D:\A_KJ\AI\Applications\AIControlTower\versions\0.9.7-20261007\AIControlTower.exe'], self.ipc_calls(out))
        self.assertGreaterEqual(out['trace'].count('processes'), 2)
        self.assertEqual(1, out['trace'].count('launch'))

    def test_clean_busy_or_pending_writes_can_retry_then_accept_once(self):
        for case in ('ipc_busy_accept', 'ipc_pending_accept'):
            with self.subTest(case=case):
                out = self.run_case(case)
                self.assertEqual('started', out['result']['status'])
                self.assertEqual(2, len(self.ipc_calls(out)))
                self.assertEqual(1, out['trace'].count('launch'))

    def test_accepted_but_original_still_alive_never_launches_or_requests_again(self):
        out = self.run_case('ipc_still_alive')
        self.assertEqual('held', out['result']['status'])
        self.assertEqual(1, len(self.ipc_calls(out)))
        self.assertNotIn('launch', out['trace'])

    def test_uncertain_transport_or_reply_is_not_retried_or_launched(self):
        for case in ('ipc_unknown', 'ipc_transport', 'ipc_inconsistent', 'ipc_bad_schema'):
            with self.subTest(case=case):
                out = self.run_case(case)
                self.assertEqual('outcome_unknown', out['result']['status'])
                self.assertEqual(1, len(self.ipc_calls(out)))
                self.assertNotIn('launch', out['trace'])

    def test_unsafe_or_rejected_exit_reply_is_held_without_retry(self):
        for case in ('ipc_identity_rejected', 'ipc_held_unknown', 'ipc_pending_request'):
            with self.subTest(case=case):
                out = self.run_case(case)
                self.assertEqual('held', out['result']['status'])
                self.assertEqual(1, len(self.ipc_calls(out)))
                self.assertNotIn('launch', out['trace'])

    def test_ipc_busy_retries_stop_at_original_monotonic_deadline(self):
        out = self.run_case('ipc_deadline')
        self.assertEqual('held', out['result']['status'])
        self.assertLessEqual(len(self.ipc_calls(out)), 3)
        self.assertNotIn('launch', out['trace'])

    def test_pid_reuse_after_accepted_reply_does_not_launch(self):
        out = self.run_case('ipc_pid_reused')
        self.assertEqual('held', out['result']['status'])
        self.assertEqual(1, len(self.ipc_calls(out)))
        self.assertNotIn('launch', out['trace'])

    def test_client_consuming_entire_deadline_does_not_sleep_past_it(self):
        out = self.run_case('ipc_deadline_consumed')
        self.assertEqual('held', out['result']['status'])
        self.assertEqual(1, len(self.ipc_calls(out)))
        self.assertNotIn('poll_wait', out['trace'])
        self.assertNotIn('launch', out['trace'])

    def test_real_client_boundary_uses_only_exact_old_executable_and_full_budget(self):
        out = self.run_case('ipc_client_boundary')
        self.assertEqual('graceful_exit_accepted', out['reply']['status'])
        self.assertEqual('held_timeout', out['short']['status'])
        self.assertEqual(1, out['calls'])
        boundary = out['boundary']
        self.assertEqual(str(self.root / 'old-runtime' / 'only-owned-client.exe'), boundary['file'])
        self.assertEqual('--request-manual-exit', boundary['arguments'])
        self.assertEqual(str(self.root / 'old-runtime'), boundary['workingDirectory'])
        self.assertTrue(boundary['redirectOut'])
        self.assertTrue(boundary['redirectError'])
        self.assertFalse(boundary['shell'])
        self.assertTrue(boundary['noWindow'])
        self.assertEqual('Hidden', boundary['windowStyle'])

    def test_exit_client_overrides_hostile_inherited_environment_with_verified_old_root(self):
        out = self.run_case('ipc_client_boundary')
        self.assertFalse(out['failed'])
        self.assertEqual(1, out['calls'])
        boundary = out['boundary']
        old = self.root / 'old-runtime'
        self.assertEqual(str(old / 'bundle-extract'), boundary['bundle'])
        self.assertEqual(str(old / 'runtime-temp'), boundary['temp'])
        self.assertEqual(str(old / 'runtime-temp'), boundary['tmp'])
        self.assertNotEqual(str(Path(out['targetRoot']) / 'bundle-extract'), boundary['bundle'])
        self.assertNotEqual(str(Path(out['targetRoot']) / 'runtime-temp'), boundary['temp'])

    def test_missing_old_runtime_directory_rejects_before_exit_client_boundary(self):
        out = self.run_case('ipc_client_env_missing')
        self.assertTrue(out['failed'])
        self.assertEqual(0, out['calls'])
        self.assertIsNone(out['boundary'])
        self.assertTrue((self.root / 'old-runtime' / 'bundle-extract').is_dir())
        self.assertFalse((self.root / 'old-runtime' / 'runtime-temp').exists())

    def test_reparse_old_runtime_directory_rejects_before_exit_client_boundary(self):
        out = self.run_case('ipc_client_env_reparse')
        self.assertTrue(out['failed'])
        self.assertEqual(0, out['calls'])
        self.assertIsNone(out['boundary'])
        self.assertTrue((self.root / 'old-runtime' / 'runtime-temp').is_dir())


if __name__ == '__main__':
    unittest.main()
