# AI Control Tower Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Windows의 AI 운영 도구 상태를 실제 증거로 집계하고 Jev 작업을 제어하는 .NET 9 WPF 관제탑을 제공한다.

**Architecture:** WPF UI는 `MainViewModel`에만 결합하고, 각 외부 도구 조회는 독립적인 `IStatusProvider` 구현으로 분리한다. 숨김 프로세스 실행·사용자에게 보이는 Jev 콘솔·설치 및 시작 프로그램 조작은 별도 서비스로 분리하며, 확인 불가능한 사실은 `Unknown`으로 표기한다.

**Tech Stack:** .NET 9, WPF, C# nullable reference types, self-contained win-x64 single-file publish, Windows Registry.

**Spec:** `docs/superpowers/specs/2026-09-20-ai-control-tower-design.md`

## Global Constraints

- 대상 프레임워크는 `net9.0-windows`이고 WPF를 사용한다.
- 상태는 실제 파일·프로세스·비대화형 명령 결과로만 판단하며 확인 실패는 `Unknown` 또는 `Error`다.
- 비밀번호, API 키, 토큰 값은 UI·앱 로그·저장소 기록에 표시하거나 저장하지 않는다.
- Desktop Commander와 상태 확인 프로세스는 숨김으로 실행하며 Jev 작업 콘솔만 보이게 한다.
- 기본 설치 경로는 `C:\Program Files\_My\AI\Applications\AIControlTower`, 권한 부족 시 `%LocalAppData%\AIControlTower`다.
- 기존 DesktopCommanderRemote 시작 파일은 먼저 백업하고, 관제탑 이외의 기존 시작 항목은 수정하지 않는다.
- 코드 주석은 영어로 작성한다.
- 완료 시 GitHub 인수인계·기록을 갱신하고 최신 사용자 지시에 따라 commit/push 한다.

---

### Task 1: 프로젝트 골격과 공통 상태 모델

**Files:**
- Create: `ai-control-tower/AIControlTower.sln`
- Create: `ai-control-tower/src/AIControlTower/AIControlTower.csproj`
- Create: `ai-control-tower/src/AIControlTower/App.xaml`
- Create: `ai-control-tower/src/AIControlTower/App.xaml.cs`
- Create: `ai-control-tower/src/AIControlTower/Models/StatusKind.cs`
- Create: `ai-control-tower/src/AIControlTower/Models/ToolStatus.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/IStatusProvider.cs`
- Create: `ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj`
- Create: `ai-control-tower/tests/AIControlTower.Tests/ToolStatusTests.cs`

**Interfaces:** Produces `enum StatusKind { Ready, Running, NotInstalled, NotConfigured, Error, Unknown }`, `record ToolStatus(string Id, string DisplayName, StatusKind Kind, string Detail, DateTimeOffset CheckedAt)`, and `IStatusProvider.CheckAsync(CancellationToken)`.

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void Unknown_status_is_not_a_success()
{
    var status = new ToolStatus("codex", "Codex", StatusKind.Unknown, "Not verified", DateTimeOffset.UtcNow);
    Assert.NotEqual(StatusKind.Ready, status.Kind);
    Assert.NotEqual(StatusKind.Running, status.Kind);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj --no-restore`

Expected: FAIL because the project and types do not exist.

- [ ] **Step 3: Create the WPF and test projects with nullable enabled**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

- [ ] **Step 4: Add the model and interface**

```csharp
public enum StatusKind { Ready, Running, NotInstalled, NotConfigured, Error, Unknown }
public sealed record ToolStatus(string Id, string DisplayName, StatusKind Kind, string Detail, DateTimeOffset CheckedAt);
public interface IStatusProvider { string Id { get; } Task<ToolStatus> CheckAsync(CancellationToken cancellationToken); }
```

- [ ] **Step 5: Run tests and build**

Run: `dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj; dotnet build ai-control-tower/AIControlTower.sln -c Release`

Expected: PASS and `0 Error(s)`.

### Task 2: 안전한 프로세스·환경·파일 검사 기반 상태 공급자

**Files:**
- Create: `ai-control-tower/src/AIControlTower/Services/ProcessRunner.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/EnvironmentProbe.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/CommandStatusProvider.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/DesktopCommanderStatusProvider.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/JevStatusProvider.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/CodexStatusProvider.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/GitHubCliStatusProvider.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/N8nStatusProvider.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/AiOpsRunnerStatusProvider.cs`
- Create: `ai-control-tower/tests/AIControlTower.Tests/CommandStatusProviderTests.cs`

**Interfaces:** Consumes Task 1 types. Produces `ProcessRunner.RunHiddenAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)` plus a status provider per required tool.

- [ ] **Step 1: Write the failing test**

```csharp
[Theory]
[InlineData(1, "", StatusKind.Error)]
[InlineData(-1, "", StatusKind.Unknown)]
public void Nonzero_or_timed_out_commands_do_not_report_ready(int exitCode, string output, StatusKind expected)
{
    Assert.Equal(expected, CommandStatusProvider.ToStatusKind(exitCode, output));
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj --filter CommandStatusProviderTests`

Expected: FAIL because `CommandStatusProvider` does not exist.

- [ ] **Step 3: Implement hidden process execution**

```csharp
var startInfo = new ProcessStartInfo(fileName, arguments)
{
    UseShellExecute = false,
    CreateNoWindow = true,
    WindowStyle = ProcessWindowStyle.Hidden,
    RedirectStandardOutput = true,
    RedirectStandardError = true
};
```

Trim captured diagnostics, redact secret assignment patterns, and never return environment-variable values.

- [ ] **Step 4: Implement providers**

Use process checks, safe file existence checks, and bounded non-interactive CLI checks: `gh auth status`, `codex login status`, `n8n --version`. Detect Jev launcher/API configuration only by command and environment-variable **presence**. A PowerShell execution-policy-blocked wrapper returns `Error`, not `Ready`. Detect Desktop Commander’s configured path and process without starting a visible console. Detect the named Actions Runner service when possible; permission failures yield `Unknown`.

- [ ] **Step 5: Run tests and build**

Run: `dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj; dotnet build ai-control-tower/AIControlTower.sln -c Release`

Expected: PASS and `0 Error(s)`.

### Task 3: 작업·전달 체인·현재 Git 활동 추적

**Files:**
- Create: `ai-control-tower/src/AIControlTower/Models/ActivitySnapshot.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/ActivityStore.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/GitActivityProvider.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/DeliveryChainProvider.cs`
- Create: `ai-control-tower/tests/AIControlTower.Tests/ActivityStoreTests.cs`

**Interfaces:** Consumes Task 1 and 2 APIs. Produces `ActivityStore.Start(string projectPath, string task, Process? process)`, `ActivityStore.CancelAsync()`, and `ActivitySnapshot`.

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void Start_records_task_and_elapsed_time()
{
    var store = new ActivityStore();
    store.Start("C:\\repo", "Check status", null);
    Assert.Equal("Check status", store.Current.Task);
    Assert.True(store.Current.Elapsed >= TimeSpan.Zero);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj --filter ActivityStoreTests`

Expected: FAIL because `ActivityStore` does not exist.

- [ ] **Step 3: Implement activity and Git state**

Record only the process started by this app. Cancellation calls `Kill(entireProcessTree: true)` only for that process. Query `git -C <path> branch --show-current` with a timeout; invalid paths and Git errors return `Unknown`.

- [ ] **Step 4: Implement delivery-chain states**

```csharp
new ToolStatus("gpt-to-desktop-commander", "GPT → Desktop Commander", StatusKind.Unknown,
    "The external GPT session cannot be verified locally.", DateTimeOffset.UtcNow);
```

Subsequent links are based only on local readiness/running evidence; message delivery is never invented.

- [ ] **Step 5: Run tests and build**

Run: `dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj; dotnet build ai-control-tower/AIControlTower.sln -c Release`

Expected: PASS and `0 Error(s)`.

### Task 4: WPF dashboard, auto-refresh, Jev command UX

**Files:**
- Create: `ai-control-tower/src/AIControlTower/MainWindow.xaml`
- Create: `ai-control-tower/src/AIControlTower/MainWindow.xaml.cs`
- Create: `ai-control-tower/src/AIControlTower/ViewModels/MainViewModel.cs`
- Create: `ai-control-tower/src/AIControlTower/ViewModels/ToolStatusViewModel.cs`
- Create: `ai-control-tower/src/AIControlTower/Commands/AsyncRelayCommand.cs`
- Create: `ai-control-tower/tests/AIControlTower.Tests/MainViewModelTests.cs`

**Interfaces:** Consumes Tasks 2–3 APIs. Produces `MainViewModel.RefreshAsync()`, `RunJevCommand`, `CancelJevCommand`, `Install`, `Uninstall`, `Restore`.

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public async Task Refresh_uses_provider_result()
{
    var viewModel = MainViewModel.ForTesting(new[] { new FakeProvider("codex", StatusKind.Unknown) });
    await viewModel.RefreshAsync();
    Assert.Equal(StatusKind.Unknown, viewModel.Statuses.Single().Kind);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj --filter MainViewModelTests`

Expected: FAIL because `MainViewModel` does not exist.

- [ ] **Step 3: Implement view models and timer**

Use `DispatcherTimer` with `Interval = TimeSpan.FromSeconds(5)` and a semaphore to prevent overlapping refreshes. Each tile shows explicit state, sanitized detail, and last checked time.

- [ ] **Step 4: Implement dashboard and Jev launch**

Use an accessible grid of status cards, activity panel, project path/task inputs, Run/Cancel controls, and maintenance controls. Before launch require Jev launcher and API-key presence. Use a visible `cmd.exe /c jev-codex ...` console only after validation and never put a secret into command arguments.

- [ ] **Step 5: Run tests and build**

Run: `dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj; dotnet build ai-control-tower/AIControlTower.sln -c Release`

Expected: PASS and `0 Error(s)`.

### Task 5: 설치·자동 시작·Desktop Commander 시작 파일 백업/복구

**Files:**
- Create: `ai-control-tower/src/AIControlTower/Services/InstallationService.cs`
- Create: `ai-control-tower/src/AIControlTower/Services/StartupMigrationService.cs`
- Create: `ai-control-tower/src/AIControlTower/Models/InstallationResult.cs`
- Create: `ai-control-tower/tests/AIControlTower.Tests/StartupMigrationServiceTests.cs`
- Create: `ai-control-tower/install/Install-AIControlTower.ps1`
- Create: `ai-control-tower/install/Uninstall-AIControlTower.ps1`
- Create: `ai-control-tower/install/Restore-DesktopCommanderStartup.ps1`

**Interfaces:** Produces `InstallationService.InstallAsync`, `UninstallAsync`, `RestoreAsync`, and an install journal stored alongside the application.

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void Migrate_creates_backup_before_disabling_target_startup_file()
{
    var result = service.BackupAndDisable(sourceFile, backupDirectory);
    Assert.True(File.Exists(result.BackupPath));
    Assert.False(File.Exists(sourceFile));
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj --filter StartupMigrationServiceTests`

Expected: FAIL because the migration service does not exist.

- [ ] **Step 3: Implement install fallback and registry registration**

Try Program Files, catch only authorization/access failures, then use `%LocalAppData%\AIControlTower`. Register a quoted executable path under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` as `AIControlTower`.

- [ ] **Step 4: Implement exact-target backup, silent launcher, uninstall, restore**

Target only `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\DesktopCommanderRemote.cmd` and `%LOCALAPPDATA%\DesktopCommanderAutoConnect\DesktopCommanderRemote_Auto.bat`. Copy bytes to an app-local backup directory with a timestamp and manifest before disabling. Create a replacement using `Start-Process -WindowStyle Hidden`, never modify other startup files. Restore only when manifest paths and backup hashes match.

- [ ] **Step 5: Run tests and build**

Run: `dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj; dotnet build ai-control-tower/AIControlTower.sln -c Release`

Expected: PASS and `0 Error(s)`.

### Task 6: 배포, 실제 실행, 인수인계 및 GitHub 정리

**Files:**
- Create: `ai-control-tower/README.md`
- Create: `ai-control-tower/CHANGELOG.md`
- Create: `04_COMMUNICATION/threads/T-0009-ai-control-tower/THREAD.md`
- Modify: `README.md`
- Modify: `01_CONTROL/AI_INSTALLATIONS.md`

**Interfaces:** Consumes published `AIControlTower.exe` and installation services. Produces `ai-control-tower/artifacts/win-x64/AIControlTower.exe` and verification evidence.

- [ ] **Step 1: Publish a self-contained single-file executable**

```powershell
dotnet publish ai-control-tower/src/AIControlTower/AIControlTower.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ai-control-tower/artifacts/win-x64
```

Expected: `ai-control-tower/artifacts/win-x64/AIControlTower.exe` exists.

- [ ] **Step 2: Verify binary and safety behavior**

Run the EXE, wait for the WPF process, inspect only its process presence and exit code, then close only that process. Exercise the no-credential Jev path and confirm it reports an error without printing values. Exercise providers against real current state and document Unknown/invalid login facts accurately.

- [ ] **Step 3: Run full verification**

```powershell
dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj -c Release
dotnet build ai-control-tower/AIControlTower.sln -c Release
Get-FileHash ai-control-tower/artifacts/win-x64/AIControlTower.exe -Algorithm SHA256
```

Expected: tests and build pass; record the published EXE hash.

- [ ] **Step 4: Write Korean operation handoff and update repository records**

Document exact source/publish paths, installation fallback, backup/restore, verified current external status, and known limits. Do not include credentials. Update the root README and installation records only with verified facts.

- [ ] **Step 5: Commit and push the completed handoff**

```powershell
git add ai-control-tower docs/superpowers README.md 01_CONTROL/AI_INSTALLATIONS.md 04_COMMUNICATION/threads/T-0009-ai-control-tower
git commit -m "feat: add AI Control Tower"
git push origin main
```

Expected: the local commit and `origin/main` match. If remote authentication or protection blocks the push, retain the local commit and report the exact unverified external step.

## Plan Self-Review

- Spec coverage: Tasks 1–5 implement WPF, all status sources, 5-second refresh, Jev control, safe state handling, hidden processes, installation, removal, restore, and startup migration. Task 6 implements publish, actual run, documentation, and GitHub handoff.
- Placeholder scan: no TBD/TODO entries or unspecified implementation steps remain.
- Type consistency: `ToolStatus`, `IStatusProvider`, `ActivityStore`, `MainViewModel`, and `InstallationService` are defined before their consumers.
