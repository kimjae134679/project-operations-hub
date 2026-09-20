[CmdletBinding()]
param(
    [switch]$Once
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$HubRepository = "kimjae134679/project-operations-hub"
$RunnerRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectMapPath = Join-Path $RunnerRoot "project-map.json"
$StateRoot = Join-Path $env:LOCALAPPDATA "AiOpsRunner"
$LogRoot = Join-Path $StateRoot "logs"
$LockPath = Join-Path $StateRoot "runner.lock"
$ClaimMarker = "<!-- ai-ops:claimed -->"
$DoneMarker = "<!-- ai-ops:done -->"
$FailMarker = "<!-- ai-ops:failed -->"

function Write-RunnerLog {
    param([string]$Message)

    New-Item -ItemType Directory -Force -Path $LogRoot | Out-Null
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Add-Content -LiteralPath (Join-Path $LogRoot "runner.log") -Value "[$timestamp] $Message" -Encoding UTF8
}

function Invoke-External {
    param(
        [Parameter(Mandatory)] [string]$Command,
        [Parameter(Mandatory)] [string[]]$Arguments,
        [string]$WorkingDirectory
    )

    $previousLocation = Get-Location
    try {
        if ($WorkingDirectory) {
            Set-Location -LiteralPath $WorkingDirectory
        }

        $output = & $Command @Arguments 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "$Command failed with exit code $LASTEXITCODE.`n$($output -join [Environment]::NewLine)"
        }
        return $output
    }
    finally {
        Set-Location -LiteralPath $previousLocation
    }
}

function Test-RequiredCommand {
    param([string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command is missing: $Name"
    }
}

function Get-TaskDefinition {
    param([string]$Body)

    $match = [regex]::Match($Body, '(?s)```ai-task\s*(\{.*?\})\s*```')
    if (-not $match.Success) {
        throw "The issue body does not contain an ai-task JSON block."
    }

    $task = $match.Groups[1].Value | ConvertFrom-Json
    if ([string]::IsNullOrWhiteSpace([string]$task.project)) {
        throw "The task project is missing."
    }
    if ([string]::IsNullOrWhiteSpace([string]$task.instruction)) {
        throw "The task instruction is missing."
    }
    return $task
}

function Add-IssueComment {
    param(
        [int]$IssueNumber,
        [string]$Body
    )

    Invoke-External -Command "gh" -Arguments @(
        "issue", "comment", [string]$IssueNumber,
        "--repo", $HubRepository,
        "--body", $Body
    ) | Out-Null
}

function Get-PendingIssues {
    $json = Invoke-External -Command "gh" -Arguments @(
        "issue", "list",
        "--repo", $HubRepository,
        "--state", "open",
        "--limit", "50",
        "--json", "number,title,body,comments"
    )

    $issues = ($json -join [Environment]::NewLine) | ConvertFrom-Json
    return @($issues | Where-Object {
        $_.title -like "[[]AI-RUN[]]*" -and
        ((@($_.comments | ForEach-Object { $_.body }) -join "`n") -notlike "*$ClaimMarker*")
    })
}

function Invoke-Task {
    param(
        [Parameter(Mandatory)] $Issue,
        [Parameter(Mandatory)] $ProjectMap
    )

    $task = Get-TaskDefinition -Body $Issue.body
    $projectName = [string]$task.project
    $projectProperty = $ProjectMap.PSObject.Properties[$projectName]
    if ($null -eq $projectProperty) {
        throw "Project is not allow-listed: $projectName"
    }
    $project = $projectProperty.Value

    $repoPath = [string]$project.localPath
    $repository = [string]$project.repository
    $baseBranch = [string]$project.baseBranch
    if (-not (Test-Path -LiteralPath $repoPath -PathType Container)) {
        throw "Local project path was not found: $repoPath"
    }

    $resolvedRepoPath = (Resolve-Path -LiteralPath $repoPath).Path
    $status = Invoke-External -Command "git" -Arguments @("status", "--porcelain") -WorkingDirectory $resolvedRepoPath
    if (($status -join "").Trim().Length -gt 0) {
        throw "The target repository has uncommitted changes. Clean or commit them before automation runs."
    }

    $branchName = "ai-ops/issue-$($Issue.number)"
    Add-IssueComment -IssueNumber $Issue.number -Body "$ClaimMarker`n[B계정] Nova 작업 실행기가 요청을 접수했습니다.`n`n- 프로젝트: $projectName`n- 대상 저장소: $repository`n- 작업 브랜치: $branchName"

    Invoke-External -Command "git" -Arguments @("fetch", "origin", $baseBranch) -WorkingDirectory $resolvedRepoPath | Out-Null
    Invoke-External -Command "git" -Arguments @("checkout", $baseBranch) -WorkingDirectory $resolvedRepoPath | Out-Null
    Invoke-External -Command "git" -Arguments @("pull", "--ff-only", "origin", $baseBranch) -WorkingDirectory $resolvedRepoPath | Out-Null
    Invoke-External -Command "git" -Arguments @("checkout", "-b", $branchName) -WorkingDirectory $resolvedRepoPath | Out-Null

    $resultPath = Join-Path $LogRoot "issue-$($Issue.number)-result.txt"
    $prompt = @"
You are executing GitHub issue #$($Issue.number) through the local AI Ops Runner.

Follow the repository's current AGENTS.md and README instructions.
Modify only what the task requires. Do not deploy, publish, merge, delete user data, or change secrets.
Run relevant tests. Do not claim checks that were not run.

Task:
$($task.instruction)

At the end, summarize changed files, tests run, failures, and remaining risks.
"@

    Invoke-External -Command "jev-codex" -Arguments @(
        "exec",
        "--full-auto",
        "-C", $resolvedRepoPath,
        "-o", $resultPath,
        $prompt
    ) -WorkingDirectory $resolvedRepoPath | Out-Null

    $changed = Invoke-External -Command "git" -Arguments @("status", "--porcelain") -WorkingDirectory $resolvedRepoPath
    if (($changed -join "").Trim().Length -eq 0) {
        $summary = if (Test-Path -LiteralPath $resultPath) { Get-Content -LiteralPath $resultPath -Raw } else { "Codex completed without file changes." }
        Add-IssueComment -IssueNumber $Issue.number -Body "$DoneMarker`n작업은 실행됐지만 커밋할 파일 변경이 없습니다.`n`n$summary"
        Invoke-External -Command "gh" -Arguments @("issue", "close", [string]$Issue.number, "--repo", $HubRepository) | Out-Null
        return
    }

    Invoke-External -Command "git" -Arguments @("add", "--all") -WorkingDirectory $resolvedRepoPath | Out-Null
    Invoke-External -Command "git" -Arguments @("commit", "-m", "AI Ops: issue #$($Issue.number)") -WorkingDirectory $resolvedRepoPath | Out-Null
    Invoke-External -Command "git" -Arguments @("push", "-u", "origin", $branchName) -WorkingDirectory $resolvedRepoPath | Out-Null

    $prUrl = Invoke-External -Command "gh" -Arguments @(
        "pr", "create",
        "--repo", $repository,
        "--base", $baseBranch,
        "--head", $branchName,
        "--title", "AI Ops: $($Issue.title -replace '^\[AI-RUN\]\s*', '')",
        "--body", "Automated local execution for $HubRepository issue #$($Issue.number). Review and merge manually."
    )

    $summaryText = if (Test-Path -LiteralPath $resultPath) { Get-Content -LiteralPath $resultPath -Raw } else { "Result summary was not produced." }
    if ($summaryText.Length -gt 5000) {
        $summaryText = $summaryText.Substring(0, 5000) + "`n`n[truncated]"
    }

    Add-IssueComment -IssueNumber $Issue.number -Body "$DoneMarker`n[B계정] Nova 실행 완료.`n`n- Pull Request: $($prUrl -join '')`n- 자동 병합·배포: 수행하지 않음`n`n$summaryText"
    Invoke-External -Command "gh" -Arguments @("issue", "close", [string]$Issue.number, "--repo", $HubRepository) | Out-Null
}

New-Item -ItemType Directory -Force -Path $StateRoot, $LogRoot | Out-Null
if (Test-Path -LiteralPath $LockPath) {
    $lockAge = (Get-Date) - (Get-Item -LiteralPath $LockPath).LastWriteTime
    if ($lockAge.TotalMinutes -lt 60) {
        exit 0
    }
    Remove-Item -LiteralPath $LockPath -Force
}

New-Item -ItemType File -Path $LockPath -Force | Out-Null
try {
    Test-RequiredCommand -Name "git"
    Test-RequiredCommand -Name "gh"
    Test-RequiredCommand -Name "jev-codex"

    if (-not (Test-Path -LiteralPath $ProjectMapPath -PathType Leaf)) {
        throw "Project map was not found: $ProjectMapPath"
    }
    $projectMap = Get-Content -LiteralPath $ProjectMapPath -Raw | ConvertFrom-Json

    $pending = Get-PendingIssues
    foreach ($issue in $pending) {
        try {
            Write-RunnerLog "Starting issue #$($issue.number): $($issue.title)"
            Invoke-Task -Issue $issue -ProjectMap $projectMap
            Write-RunnerLog "Completed issue #$($issue.number)"
        }
        catch {
            $message = $_.Exception.Message
            if ($message.Length -gt 3000) {
                $message = $message.Substring(0, 3000) + " [truncated]"
            }
            Write-RunnerLog "Failed issue #$($issue.number): $message"
            try {
                Add-IssueComment -IssueNumber $issue.number -Body "$FailMarker`n[B계정] Nova 실행 실패.`n`n오류: $message"
            }
            catch {
                Write-RunnerLog "Could not report failure for issue #$($issue.number): $($_.Exception.Message)"
            }
        }
    }
}
finally {
    Remove-Item -LiteralPath $LockPath -Force -ErrorAction SilentlyContinue
}

if (-not $Once) {
    Write-RunnerLog "Scheduled run completed."
}
