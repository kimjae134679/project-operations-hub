$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$required = @(
    'README.md',
    'AGENTS.md',
    'docs\ACCEPTANCE.md',
    'skills\ponytail-lite\SKILL.md',
    'templates\AGENTS.template.md'
)

$missing = @()
foreach ($item in $required) {
    $path = Join-Path $root $item
    if (-not (Test-Path $path)) {
        $missing += $item
    }
}

if ($missing.Count -gt 0) {
    Write-Error ('Missing required files: ' + ($missing -join ', '))
}

$agents = Get-Content (Join-Path $root 'AGENTS.md') -Raw
foreach ($needle in @('Default authorization', 'Completion contract', 'smallest complete implementation')) {
    if ($agents -notmatch [regex]::Escape($needle)) {
        Write-Error "AGENTS.md is missing required concept: $needle"
    }
}

Write-Output 'PASS: Astra Codex Workbench starter structure is valid.'
