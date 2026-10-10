$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'Lifecycle.psm1') -Force
$script:checks=0
function Assert($value, $message) { if (!$value) { throw $message }; $script:checks++ }
function New-TestRuntime {
  $model = @{ running=$false; healthy=$false; foreign=$false; pid=$null; starts=0; stops=0; opens=0; clears=0 }
  $runtime = @{
    Read={ [pscustomobject]@{running=$model.running;healthy=$model.healthy;foreign=$model.foreign;pid=$model.pid} }.GetNewClosure()
    Spawn={ $model.starts++; $model.running=$true; $model.healthy=$true; $model.pid=123; return 123 }.GetNewClosure()
    Save={param($value) $model.pid=$value}.GetNewClosure()
    Stop={param($value) Assert ($value -eq $model.pid) 'wrong PID'; $model.stops++; $model.running=$false; $model.healthy=$false }.GetNewClosure()
    Clear={ $model.clears++; $model.pid=$null }.GetNewClosure()
    Open={ $model.opens++ }.GetNewClosure()
    Wait={}
  }
  return @{model=$model;runtime=$runtime}
}
$case=New-TestRuntime
Invoke-StudioLifecycle start $case.runtime | Out-Null
Invoke-StudioLifecycle start $case.runtime | Out-Null
Assert ($case.model.starts -eq 1) 'warm start created duplicate server'
Assert ($case.model.opens -eq 0) 'start unexpectedly opened browser'
Invoke-StudioLifecycle open $case.runtime | Out-Null
Assert ($case.model.opens -eq 1) 'open did not open browser'
Invoke-StudioLifecycle stop $case.runtime | Out-Null
Invoke-StudioLifecycle stop $case.runtime | Out-Null
Assert ($case.model.stops -eq 1) 'stop not idempotent'
$blocked=$false
try { Invoke-StudioLifecycle open $case.runtime | Out-Null } catch {$blocked=$true}
Assert $blocked 'open silently started stopped server'
Assert ($case.model.starts -eq 1) 'open changed running state'
$case=New-TestRuntime; $case.model.foreign=$true
foreach($action in @('start','stop','open')) {
  $blocked=$false
  try { Invoke-StudioLifecycle $action $case.runtime | Out-Null } catch {$blocked=$true}
  Assert $blocked ('foreign process not blocked: '+$action)
}
Assert ($case.model.starts+$case.model.stops+$case.model.opens -eq 0) 'foreign process mutated'
$case=New-TestRuntime; $case.model.running=$true; $case.model.pid=123
$blocked=$false
try { Invoke-StudioLifecycle start $case.runtime | Out-Null } catch {$blocked=$true}
Assert $blocked 'unhealthy owned server duplicated'
Invoke-StudioLifecycle stop $case.runtime | Out-Null
Assert ($case.model.stops -eq 1) 'unhealthy owned server cannot stop'
$case=New-TestRuntime
$case.runtime.Spawn={ $case.model.starts++; $case.model.running=$true; $case.model.pid=123; return 123 }.GetNewClosure()
$blocked=$false
try { Invoke-StudioLifecycle start $case.runtime | Out-Null } catch {$blocked=$true}
Assert $blocked 'startup timeout accepted as success'
Assert ($case.model.stops -eq 1 -and !$case.model.running) 'failed startup was not cleaned up'
Write-Output ('PASS: '+$script:checks+' lifecycle assertions (cold/warm/open/off/foreign/unhealthy/timeout)')
