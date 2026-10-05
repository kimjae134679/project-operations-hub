using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AIControlTower.ViewModels;

namespace AIControlTower.Services;

/// <summary>Opt-in native WPF smoke/capture harness; no mock data or external UI automation.</summary>
public static class UiVerification
{
    public static async Task RunAsync(MainWindow window, string outputDirectory, bool consolidateRemote)
    {
        Directory.CreateDirectory(outputDirectory);
        await window.InitializeAsync();
        var vm = (MainViewModel)window.DataContext;
        var contrastChecks = new List<object>();
        vm.SelectedProject = vm.Projects.FirstOrDefault(p => p.Id == "project-operations-hub") ?? vm.Projects.FirstOrDefault();
        window.UpdateLayout();
        var program = vm.SelectedProject?.Functions.SelectMany(f => f.Programs).FirstOrDefault(p => p.Id.EndsWith("/validation"));
        if (program is not null)
        {
            vm.ProgramSearch = program.DisplayName;
            await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
            var list = Walk(window).OfType<ListBox>().FirstOrDefault(l => l.Items.Contains(program));
            if (list is not null) list.SelectedItem = program;
            else vm.SelectedProgram = program;
            vm.SelectedCommand = program.Commands.FirstOrDefault(c => c.Arguments.SequenceEqual(new[] { "--version" }));
            if (vm.SelectedCommand is not null) await vm.LaunchProgramAsync();
        }
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        contrastChecks.Add(CheckTextColor(window, "ProjectTitle", "wide"));
        contrastChecks.Add(CheckTextColor(window, "InspectorTitle", "wide"));
        Capture(window, Path.Combine(outputDirectory, "management.png"));
        var logExpander = window.FindName("LogExpander") as Expander;
        if (logExpander is not null)
        {
            logExpander.IsExpanded = true;
            await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
            Capture(window, Path.Combine(outputDirectory, "workbench-logs.png"));
            logExpander.IsExpanded = false;
        }
        var selectedProject = vm.SelectedProject;
        vm.ProjectSearch = selectedProject?.DisplayName ?? "";
        vm.ProgramSearch = program?.DisplayName ?? "";
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        var searchVerified = vm.FilteredProjects.Any(p => p == selectedProject) && vm.FilteredFunctions.Sum(f => f.Programs.Count) == 1
            && vm.FilteredFunctions.SelectMany(f => f.Programs).Single() == program;
        if (!searchVerified) throw new InvalidOperationException("Project/program search did not preserve actual matching program identity.");
        Capture(window, Path.Combine(outputDirectory, "workbench-search.png"));
        vm.ProjectSearch = ""; vm.ProgramSearch = "";
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        var tabs = Walk(window).OfType<TabControl>().First();
        tabs.SelectedIndex = 1;
        if (consolidateRemote) { await vm.ConsolidateRemoteStartupAsync(); await vm.EnsureRemoteRunningAsync(); }
        await vm.RefreshAsync();
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        Capture(window, Path.Combine(outputDirectory, "connections.png"));
        await vm.SyncCommunicationAsync(true);
        tabs.SelectedIndex = 2;
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        if (vm.Notices.Count == 0) throw new InvalidOperationException("Actual notices did not load.");
        vm.SelectedNotice = vm.Notices.FirstOrDefault(n => n.Notice.Id == "N-0005") ?? vm.Notices.First();
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        Capture(window, Path.Combine(outputDirectory, "notices.png"));
        window.Width = 1060; window.Height = 720;
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        CheckVisibleControl(window, "NoticeBody");
        Capture(window, Path.Combine(outputDirectory, "notices-compact.png"));
        window.Width = 1460; window.Height = 920;
        tabs.SelectedIndex = 3;
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        Capture(window, Path.Combine(outputDirectory, "communication.png"));
        window.Width = 1060; window.Height = 720;
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        CheckVisibleControl(window, "CommunicationProjectList");
        CheckVisibleControl(window, "InboxList");
        Capture(window, Path.Combine(outputDirectory, "communication-compact.png"));
        window.Width = 1460; window.Height = 920;
        tabs.SelectedIndex = 4;
        await vm.RefreshServerAsync(true);
        await vm.RefreshRosterAsync();
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        contrastChecks.Add(CheckTextColor(window, "ServerHeading", "wide"));
        Capture(window, Path.Combine(outputDirectory, "server.png"));
        window.Width = 1060; window.Height = 720;
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        CheckVisibleControl(window, "ServerModePanel");
        contrastChecks.Add(CheckTextColor(window, "ServerHeading", "compact"));
        Capture(window, Path.Combine(outputDirectory, "server-compact.png"));
        window.Width = 1460; window.Height = 920;
        tabs.SelectedIndex = 0;
        var oldWidth = window.Width; var oldHeight = window.Height;
        window.Width = 1060; window.Height = 720;
        if (logExpander is not null) logExpander.IsExpanded = true;
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        var compactControls = CheckInspectorControls(window);
        contrastChecks.Add(CheckTextColor(window, "ProjectTitle", "compact"));
        contrastChecks.Add(CheckTextColor(window, "InspectorTitle", "compact"));
        Capture(window, Path.Combine(outputDirectory, "management-compact.png"));
        if (logExpander is not null) logExpander.IsExpanded = false;
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        CheckInspectorControls(window);
        window.Width = oldWidth; window.Height = oldHeight;
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        var game = vm.Projects.FirstOrDefault(p => p.DisplayName == "멀티의 신");
        if (game is not null)
        {
            vm.SelectedProject = game;
            vm.SelectedProgram = game.Functions.SelectMany(f => f.Programs).FirstOrDefault(p => p.Id.EndsWith("/unity-project"));
            await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
            CheckInspectorControls(window);
            Capture(window, Path.Combine(outputDirectory, "editor.png"));
            vm.SelectedProgram = game.Functions.SelectMany(f => f.Programs).FirstOrDefault(p => p.Id.EndsWith("/latest-apk"));
            await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
            var gameList = Walk(window).OfType<ListBox>().FirstOrDefault(l => l.Items.Contains(vm.SelectedProgram));
            if (gameList is not null) gameList.SelectedItem = vm.SelectedProgram;
            await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
            var selections=Walk(window).OfType<ListBox>().Where(l=>l.SelectedItem is AIControlTower.Models.ProgramItem).ToArray();
            if (selections.Length!=1 || selections[0].SelectedItem != vm.SelectedProgram) throw new InvalidOperationException("Multiple program groups retain stale selection.");
            Capture(window, Path.Combine(outputDirectory, "workbench.png"));
            window.Width = 1060; window.Height = 720;
            await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
            CheckInspectorControls(window);
            Capture(window, Path.Combine(outputDirectory, "workbench-compact.png"));
            window.Width = oldWidth; window.Height = oldHeight;
            await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render);
        }
        var wheelChecks = await MouseWheelVerification.VerifyListsAsync(window);
        var summary = new
        {
            CheckedAt = DateTimeOffset.Now, vm.ProjectsCount, vm.ProgramsCount, vm.RunningCount,
            Project = vm.SelectedProject?.Name, Program = vm.SelectedProgram?.Name, ProgramState = vm.SelectedProgram?.Status,
            Background = window.Background.ToString(), Text = window.Foreground.ToString(), FontSize = window.FontSize,
            Controls = Walk(window).OfType<Button>().Where(b => b.Content is string).Select(b => new { Name = b.Content, b.IsEnabled, b.ActualWidth, b.ActualHeight }).ToArray(),
            CompactControls = compactControls,
            SearchVerified = searchVerified,
            MouseWheelChecks = wheelChecks,
            ContrastChecks = contrastChecks,
            KeyColors = new { Text = ThemeColor(window, "TextBrush"), Canvas = ThemeColor(window, "CanvasBrush"), Surface = ThemeColor(window, "SurfaceBrush") },
            ValidationState = program?.Status,
            Catalog = vm.Projects.Select(p => new { p.DisplayName, p.Description, p.RoleLabel, p.Path, p.EvidenceDate, Programs = p.Functions.SelectMany(f => f.Programs).Select(item => new { item.DisplayName, item.KindLabel, item.ActionLabel, item.Path, item.CanLaunch, item.HasEditorLauncher }).ToArray() }).ToArray(),
            Statuses = vm.Statuses.Select(s => new { s.DisplayName, s.State, s.Detail }).ToArray(),
            RemoteConsolidationRequested = consolidateRemote,
            Notices = vm.Notices.Select(n => new { n.Title, n.VersionLabel, n.ReadSummary }).ToArray(),
            CommunicationProjects = vm.CommunicationProjects.ToArray(),
            CollectedItems = vm.InboxItems.Count,
            CommunicationIssues = vm.CommunicationErrors.ToArray(),
            CommunicationDiagnostics = vm.JobLogs.Where(line=>line.Contains("소통 진단 · ")).ToArray(),
            Server = vm.ServerSnapshot,
            Roster = new { vm.RosterHealthy, PlayerCount = vm.ServerPlayers.Count, vm.RosterStatus, vm.RosterCheckedAt },
            Note = "Native running WPF client rendered at 96 dpi. PNGs exclude the OS titlebar; actual data and command results."
        };
        File.WriteAllText(Path.Combine(outputDirectory, "ui-verification.json"), JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));
    }
    private static string ThemeColor(MainWindow window, string key) =>
        (window.FindResource(key) as SolidColorBrush)?.Color.ToString()
        ?? throw new InvalidOperationException("Expected solid theme color: " + key);

    private static object CheckTextColor(MainWindow window, string name, string layout)
    {
        var text = window.FindName(name) as TextBlock
            ?? throw new InvalidOperationException("Missing body text verification target: " + name);
        var expected = window.FindResource("TextBrush") as SolidColorBrush
            ?? throw new InvalidOperationException("TextBrush must be a solid color.");
        var actual = text.Foreground as SolidColorBrush;
        var visible = text.IsVisible && text.ActualWidth > 0 && text.ActualHeight > 0;
        var matches = actual is not null && actual.Color == expected.Color && actual.Opacity == expected.Opacity;
        if (!visible || !matches)
            throw new InvalidOperationException($"Body text color regression in {layout}/{name}: visible={visible}, actual={text.Foreground}, expected={expected}.");
        return new { Name = name, Layout = layout, IsVisible = visible, Foreground = actual!.Color.ToString(), ExpectedTextColor = expected.Color.ToString(), MatchesTextBrush = matches };
    }
    private static void CheckVisibleControl(MainWindow window, string name)
    {
        var element = (FrameworkElement)window.FindName(name);
        var root = (FrameworkElement)window.Content;
        var bounds = element.TransformToAncestor(root).TransformBounds(new Rect(0,0,element.ActualWidth,element.ActualHeight));
        if (!element.IsVisible || element.ActualWidth < 100 || element.ActualHeight < 80 || bounds.Right > root.ActualWidth + 1 || bounds.Bottom > root.ActualHeight + 1)
            throw new InvalidOperationException("Communication control clipped: " + name + " " + bounds);
    }
    private static object[] CheckInspectorControls(MainWindow window)
    {
        var inspector = (FrameworkElement)window.FindName("ProgramInspector");
        var available = new Rect(0, 0, inspector.ActualWidth, inspector.ActualHeight);
        var controls = Walk(inspector).OfType<FrameworkElement>()
            .Where(e => e.IsVisible && new[] { "CommandSelector", "LaunchProgramButton", "OpenProgramButton", "StopProgramButton" }.Contains(e.Name)).ToArray();
        var results = new List<object>();
        foreach (var control in controls)
        {
            var bounds = control.TransformToAncestor(inspector).TransformBounds(new Rect(0, 0, control.ActualWidth, control.ActualHeight));
            var visible = control.ActualHeight > 0 && bounds.Top >= -1 && bounds.Left >= -1 && bounds.Bottom <= available.Bottom + 1 && bounds.Right <= available.Right + 1;
            results.Add(new { Name = control is Button button ? button.Content : "실행 명령", VisibleInsideInspector = visible, Bounds = bounds.ToString() });
            if (!visible) throw new InvalidOperationException($"Compact inspector clips {control.GetType().Name}: {bounds}; available {available}");
        }
        return results.ToArray();
    }
    private static void Capture(Window window, string path)
    {
        window.UpdateLayout();
        var content = (FrameworkElement)window.Content;
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(content.ActualWidth + content.Margin.Left + content.Margin.Right), (int)Math.Ceiling(content.ActualHeight + content.Margin.Top + content.Margin.Bottom), 96, 96, PixelFormats.Pbgra32);
        var background = new DrawingVisual();
        // VisualBrush renders the content in its own coordinates, without its Window margin offset.
        using (var drawing = background.RenderOpen())
        {
            var bounds = new Rect(content.Margin.Left, content.Margin.Top, content.ActualWidth, content.ActualHeight);
            drawing.DrawRectangle(window.Background, null, new Rect(0,0,bitmap.Width,bitmap.Height));
            drawing.DrawRectangle(new VisualBrush(content) { Stretch = Stretch.Fill }, null, bounds);
        }
        bitmap.Render(background);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); encoder.Save(stream);
    }
    private static IEnumerable<DependencyObject> Walk(DependencyObject element)
    {
        yield return element;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            foreach (var child in Walk(VisualTreeHelper.GetChild(element, i))) yield return child;
    }
}
