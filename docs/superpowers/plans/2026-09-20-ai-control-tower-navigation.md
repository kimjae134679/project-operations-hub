# AI Control Tower Navigation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the missing left navigation, make every dashboard section reachable through one scroll surface, and move the existing Jev controls into a separate window without adding GPT scheduler features.

**Architecture:** `MainWindow` remains the shared application shell and owns one `MainViewModel`. A new `JevControlWindow` receives that same view model so status, task text, and cancellation state remain synchronized. The dashboard body becomes one scrollable vertical document while the sidebar invokes section navigation or opens the single Jev window instance.

**Tech Stack:** .NET 9, WPF/XAML, C#, xUnit

**Spec:** `docs/superpowers/specs/2026-09-20-ai-control-tower-navigation-design.md`

## Global Constraints

- Do not add GPT Prompt Scheduler, ChatGPT/browser integration, reservations, repeats, or handoff behavior.
- Keep `LocalExecutionPolicy.AllowLocalJevExecution` false and preserve every existing execution guard.
- Do not automate or click Codex approvals or Windows UAC prompts.
- Preserve install, uninstall, Desktop Commander restore, five-second refresh, and sensitive-value sanitization.
- Keep all user-facing status and permission explanations in Korean.

---

### Task 1: Expose Jev policy state to both windows

**Files:**
- Modify: `ai-control-tower/tests/AIControlTower.Tests/LocalExecutionPolicyTests.cs`
- Modify: `ai-control-tower/src/AIControlTower/ViewModels/MainViewModel.cs`

**Interfaces:**
- Produces: `bool MainViewModel.IsLocalJevExecutionAllowed`
- Produces: `string MainViewModel.LocalJevPolicyMessage`

- [ ] **Step 1: Write the failing policy-presentation test**

```csharp
[Fact]
public void MainViewModel_ExposesDisabledJevPolicy()
{
    using var viewModel = new MainViewModel();
    Assert.False(viewModel.IsLocalJevExecutionAllowed);
    Assert.Equal(LocalExecutionPolicy.LocalJevDisabledMessage, viewModel.LocalJevPolicyMessage);
}
```

- [ ] **Step 2: Run the targeted test and confirm the properties are missing**

Run:

```powershell
dotnet test tests\AIControlTower.Tests\AIControlTower.Tests.csproj -c Release --filter MainViewModel_ExposesDisabledJevPolicy
```

Expected: FAIL at compile time because the two properties do not exist.

- [ ] **Step 3: Add the minimal properties**

```csharp
public bool IsLocalJevExecutionAllowed => LocalExecutionPolicy.AllowLocalJevExecution;
public string LocalJevPolicyMessage => LocalExecutionPolicy.LocalJevDisabledMessage;
```

- [ ] **Step 4: Re-run the targeted test**

Expected: PASS with one matching test.

- [ ] **Step 5: Commit**

```powershell
git add ai-control-tower/src/AIControlTower/ViewModels/MainViewModel.cs ai-control-tower/tests/AIControlTower.Tests/LocalExecutionPolicyTests.cs
git commit -m "test: expose Jev policy state"
```

### Task 2: Add the separate Jev control window

**Files:**
- Create: `ai-control-tower/src/AIControlTower/JevControlWindow.xaml`
- Create: `ai-control-tower/src/AIControlTower/JevControlWindow.xaml.cs`
- Modify: `ai-control-tower/src/AIControlTower/MainWindow.xaml.cs`

**Interfaces:**
- Consumes: `MainViewModel.ProjectPath`, `TaskInput`, `ActivitySummary`, `Message`, `IsLocalJevExecutionAllowed`, and `LocalJevPolicyMessage`
- Produces: `MainWindow.OpenJevWindow()` and one live `JevControlWindow` instance

- [ ] **Step 1: Add a Jev control window with the existing bindings**

The XAML must include these bindings and no scheduler controls:

```xml
<TextBox Text="{Binding ProjectPath, UpdateSourceTrigger=PropertyChanged}" />
<TextBox Text="{Binding TaskInput, UpdateSourceTrigger=PropertyChanged}"
         AcceptsReturn="True" TextWrapping="Wrap" />
<Button Content="Jev 작업 실행"
        IsEnabled="{Binding IsLocalJevExecutionAllowed}"
        Click="RunJev_Click" />
<Button Content="작업 취소" Click="CancelJev_Click" />
<TextBlock Text="{Binding LocalJevPolicyMessage}" TextWrapping="Wrap" />
<TextBlock Text="{Binding ActivitySummary}" />
<TextBlock Text="{Binding Message}" />
```

- [ ] **Step 2: Wire the window to the shared view model**

```csharp
public JevControlWindow(MainViewModel viewModel)
{
    InitializeComponent();
    DataContext = viewModel;
}
```

Its handlers call only `RunJevTask()` and `CancelJevTaskAsync()`.

- [ ] **Step 3: Keep one Jev window instance in MainWindow**

```csharp
private JevControlWindow? _jevWindow;

private void OpenJevWindow()
{
    if (_jevWindow is { IsLoaded: true })
    {
        _jevWindow.Activate();
        return;
    }

    _jevWindow = new JevControlWindow(_viewModel) { Owner = this };
    _jevWindow.Closed += (_, _) => _jevWindow = null;
    _jevWindow.Show();
}
```

- [ ] **Step 4: Compile the WPF project**

Run:

```powershell
dotnet build src\AIControlTower\AIControlTower.csproj -c Release
```

Expected: build succeeds with zero errors and warnings.

- [ ] **Step 5: Commit**

```powershell
git add ai-control-tower/src/AIControlTower/JevControlWindow.xaml ai-control-tower/src/AIControlTower/JevControlWindow.xaml.cs ai-control-tower/src/AIControlTower/MainWindow.xaml.cs
git commit -m "feat: separate Jev control window"
```

### Task 3: Rebuild the dashboard shell and prevent clipped sections

**Files:**
- Modify: `ai-control-tower/src/AIControlTower/MainWindow.xaml`
- Modify: `ai-control-tower/src/AIControlTower/MainWindow.xaml.cs`

**Interfaces:**
- Consumes: the existing refresh/install/uninstall/restore handlers and `OpenJevWindow()` from Task 2
- Produces: sidebar navigation handlers `Home_Click`, `Status_Click`, `Jev_Click`, and `InstallSection_Click`

- [ ] **Step 1: Replace the root layout with sidebar plus full-content scroll**

Use this structural shape:

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="224"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <Border Grid.Column="0"><!-- sidebar buttons --></Border>
    <Grid Grid.Column="1">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        <Grid Grid.Row="0"><!-- header --></Grid>
        <ScrollViewer x:Name="DashboardScroll" Grid.Row="1"
                      VerticalScrollBarVisibility="Auto">
            <StackPanel>
                <!-- summary, status, console, install/restore, security -->
            </StackPanel>
        </ScrollViewer>
    </Grid>
</Grid>
```

Remove the Jev path/task editor from `MainWindow.xaml`. Keep a summary card and a sidebar button that call `Jev_Click`.

- [ ] **Step 2: Mark scroll targets and add navigation handlers**

```xml
<Border x:Name="StatusSection">...</Border>
<Border x:Name="InstallSection">...</Border>
```

```csharp
private void Home_Click(object sender, RoutedEventArgs e) => DashboardScroll.ScrollToTop();
private void Status_Click(object sender, RoutedEventArgs e) => StatusSection.BringIntoView();
private void InstallSection_Click(object sender, RoutedEventArgs e) => InstallSection.BringIntoView();
private void Jev_Click(object sender, RoutedEventArgs e) => OpenJevWindow();
```

- [ ] **Step 3: Keep all dashboard sections reachable**

Give the status table `MinHeight="280"`, the console `MinHeight="190"`, and place install/restore before the final security notice inside the same `StackPanel`. Do not introduce an inner vertical `ScrollViewer` around a single card.

- [ ] **Step 4: Build and run all unit tests**

Run:

```powershell
dotnet test tests\AIControlTower.Tests\AIControlTower.Tests.csproj -c Release
dotnet build AIControlTower.sln -c Release
```

Expected: every test passes; build reports zero warnings and errors.

- [ ] **Step 5: Commit**

```powershell
git add ai-control-tower/src/AIControlTower/MainWindow.xaml ai-control-tower/src/AIControlTower/MainWindow.xaml.cs
git commit -m "feat: add control tower navigation"
```

### Task 4: Publish, verify, document, and hand off

**Files:**
- Modify: `ai-control-tower/README.md`
- Modify: `000_사용자용/08_AI_관제탑.md`
- Modify: `04_COMMUNICATION/threads/T-0009-ai-control-tower/THREAD.md`

**Interfaces:**
- Consumes: the tested UI from Tasks 1–3
- Produces: updated installed EXE and GitHub handoff

- [ ] **Step 1: Publish the single-file Windows build**

```powershell
dotnet publish src\AIControlTower\AIControlTower.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o artifacts\win-x64
```

- [ ] **Step 2: Update the installed copy and verify hashes**

Copy the new EXE to `%LocalAppData%\AIControlTower\AIControlTower.exe`. Compare SHA-256 for the artifact and installed copy; they must be equal.

- [ ] **Step 3: Run the installed app and verify the UI**

Verify:

- sidebar is visible;
- all main sections are reachable with one vertical scrollbar;
- Jev task control opens as a separate window;
- the Jev run button remains disabled with the policy explanation;
- no GPT scheduler, reservation, browser, or permission auto-click UI appears.

- [ ] **Step 4: Update documentation and handoff**

Record the final paths, test/build results, hash, screenshot/manual validation scope, and the unchanged OpenAI-account-use policy.

- [ ] **Step 5: Commit and push**

```powershell
git add ai-control-tower/README.md 000_사용자용/08_AI_관제탑.md 04_COMMUNICATION/threads/T-0009-ai-control-tower/THREAD.md
git commit -m "docs: hand off navigation redesign"
git pull --rebase origin main
git push origin main
```
