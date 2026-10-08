using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;
using AIControlTower.Views;

// Offscreen UI QA only. No App/MainWindow, Show, startup initialization, synchronization,
// collection, Git, network, bridge, execution, or persistent read/receipt write is invoked.
internal static class Program
{
    private const string AllowedOutput = @"C:\Users\user\Documents\Codex\2026-10-08\task-7\qa-preview";
    private const string Hub = @"D:\A_KJ\AI\ControlTowerData\communication-hub";
    private static readonly DateTimeOffset FixtureTime = DateTimeOffset.Parse("2026-10-08T10:00:00Z");
    private static readonly MethodInfo Apply = typeof(MainViewModel).GetMethod("ApplyCommunicationSnapshot", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingMethodException("ApplyCommunicationSnapshot");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [STAThread]
    private static int Main(string[] args)
    {
        var output = Path.GetFullPath(args.Length == 0 ? AllowedOutput : args[0]).TrimEnd(Path.DirectorySeparatorChar);
        if (!output.Equals(AllowedOutput, StringComparison.OrdinalIgnoreCase)
            && !output.StartsWith(AllowedOutput + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return 2;
        Directory.CreateDirectory(output);
        var protectedBefore = ProtectSources();
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown, Resources = Resources() };
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        var reports = new List<object>();
        var code = 0;
        Dispatcher.CurrentDispatcher.BeginInvoke(new Action(async () =>
        {
            try
            {
                // The service's explicit read-only entrypoint never performs delivery/collection/status writes.
                var actual = await new CommunicationService().ReadOnlyAsync(Hub);
                var scoped = actual with
                {
                    InboxItems = actual.InboxItems.Where(i => i.ProjectId == "Control-Tower"
                        || i.ThreadGroup == "04_COMMUNICATION/threads/T-0009-ai-control-tower").ToArray(),
                    ProjectStates = actual.ProjectStates.Where(i => i.ProjectId == "Control-Tower").ToArray()
                };
                Check(scoped.InboxItems.Count > 0, "Real hub read returned no scoped source records");
                using (var vm = NewViewModel(output))
                {
                    ApplySnapshot(vm, scoped);
                    var view = NewView(vm);
                    var row = vm.InboxItems.FirstOrDefault(r => r.Identity.Contains("T-0009-ai-control-tower", StringComparison.Ordinal))
                        ?? vm.InboxItems.First(r => r.Exchange is null);
                    vm.SelectedInbox = row;
                    await CaptureMatrix(view, vm, output, "actual-hub", reports);
                    reports.Add(new { Source = "actual read-only hub", ScopedSourceRecords = scoped.InboxItems.Count,
                        DisplayedTopics = vm.InboxItems.Count, vm.CommunityListSummary, Errors = actual.Errors.Select(e => new { e.Code, e.Message }).ToArray(),
                        Selected = row.Title, row.CountLabel, row.ExtraCountLabel, row.ReplyStateLabel });
                }

                // All synthetic records are in-memory only; none are stored in any project inbox or THREAD.
                using (var vm = NewViewModel(output))
                {
                    var fixtures = new[]
                    {
                        Topic("zero", "요청: 댓글 없는 검증 요청", 0),
                        Topic("many", "요청: 댓글 여러 개", 3),
                        Topic("long", "요청: 댓글 12개와 긴 본문", 12),
                        Topic("missing", "본문 조회 미확인", 0) with { Body = null },
                        TaskRecord("completed", "완료 상태 검증", "completed", "실제 전달한 완료 답변", "current_chat"),
                        TaskRecord("answered", "실제 응답 상태 검증", "in_progress", "실제 전달한 진행 답변", "current_chat"),
                        TaskRecord("waiting", "답변 대기 상태 검증", "in_progress", "최종 답변 대기", "unknown")
                    };
                    ApplySnapshot(vm, Snapshot(fixtures));
                    var view = NewView(vm);
                    foreach (var id in new[] { "zero", "many", "long", "missing", "completed", "answered", "waiting" })
                    {
                        var row = Find(vm, id);
                        if (id == "zero") Check(row.CountsKnown && row.CommentCount == 0 && row.ReplyStatusKey == "Waiting", "Zero-comment request classification");
                        if (id is "many" or "long") Check(row.CommentCount == (id == "many" ? 3 : 12) && row.ReplyStatusKey == "Answered", "Explicit comment count/answered state");
                        if (id == "missing") Check(!row.CountsKnown && row.CountLabel.Contains("미확인") && !row.CountLabel.Contains("댓글 0"), "Missing body fabricated a zero");
                        if (id == "completed") Check(row.ReplyStatusKey == "Complete" && row.CommentCount == 0, "Completed task state/zero comments");
                        if (id == "answered") Check(row.ReplyStatusKey == "Answered" && row.CommentCount == 0, "Task response incorrectly became comments");
                        if (id == "waiting") Check(row.ReplyStatusKey == "Waiting", "Pending final response became answered");
                        vm.SelectedInbox = row;
                        Check(vm.CommunityComments.Count == row.CommentCount, "Projected comment card count");
                        await CaptureMatrix(view, vm, output, "fixture-" + id, reports);
                        if (id == "long")
                        {
                            var reader = (ScrollViewer)view.FindName("ArticleReader");
                            reader.ScrollToEnd(); await Flush(); Layout(view, 700, 560);
                            Check(reader.ScrollableHeight > 0 && reader.VerticalOffset > 0, "Many-comment article did not scroll");
                            Capture(view, 700, 560, Path.Combine(output, "fixture-long-comments-bottom-700-dark.png"));
                        }
                    }
                    // Exercise the actual rendered comment button/code-behind route. The store is memory-only.
                    var previous = Find(vm, "many");
                    vm.SelectedInbox = previous;
                    await Flush(); Layout(view, 700, 560);
                    Check(previous.UnreadCommentCount == 3 && vm.CommunityComments.All(c => c.IsUnread), "Three unread fixture comments before clicking");
                    var clickedCommentIds = new HashSet<string>(StringComparer.Ordinal);
                    for (var remaining = 2; remaining >= 0; remaining--)
                    {
                        var button = Descendants(view).OfType<Button>().First(b => Equals(b.Content, "이 댓글 읽음 표시")
                            && b.DataContext is CommunityCommentRow { IsUnread: true });
                        var comment = (CommunityCommentRow)button.DataContext;
                        Check(clickedCommentIds.Add(comment.Entry.Identity), "Rendered read button reused an already-read comment");
                        button.BringIntoView(); await Flush(); Layout(view, 700, 560);
                        Check(button.Visibility == Visibility.Visible && button.ActualWidth > 0 && button.ActualHeight > 0, "Comment read button did not render");
                        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        await Flush(); Layout(view, 700, 560);
                        Check(previous.UnreadCommentCount == remaining, "Actual comment button did not decrement the topic badge");
                    }
                    Check(!previous.HasNewComments && previous.UnreadCommentCount == 0, "Fixture comment read state did not clear");
                    Check(vm.CommunityComments.All(c => !c.IsUnread && c.ReadLabel == "읽음"), "Rendered comment labels did not become read");
                    await CaptureMatrix(view, vm, output, "fixture-three-comments-marked-read", reports);
                    var withNew = fixtures.Select(i => i.SourceName.Contains("/many/", StringComparison.Ordinal)
                        ? Topic("many", "요청: 댓글 여러 개", 4) : i).ToArray();
                    ApplySnapshot(vm, Snapshot(withNew));
                    var refreshed = Find(vm, "many");
                    Check(refreshed.CommentCount == 4 && refreshed.HasNewComments && refreshed.UnreadCommentCount == 1, "New comment did not preserve three already-read identities");
                    vm.SelectedInbox = refreshed;
                    Check(vm.CommunityComments.Count(c => c.IsUnread) == 1 && vm.CommunityComments.Count(c => c.ReadLabel == "읽음") == 3,
                        "Comment-card read labels lost three previous read identities");
                    await CaptureMatrix(view, vm, output, "fixture-one-new-comment", reports);
                    reports.Add(new { Scenario = "Actual comment read-button route", ButtonClicks = clickedCommentIds.Count,
                        AllThreePreviouslyRead = true, NewCommentCountAfterAppend = refreshed.UnreadCommentCount,
                        PersistentReadStoreWrites = 0 });
                }

                using (var vm = NewViewModel(output))
                {
                    ApplySnapshot(vm, new([], [], [], [], [new(null, "thread_retry", "격리 검증용 읽기 실패")], FixtureTime, []));
                    Check(vm.InboxItems.Count == 0 && vm.CommunityListSummary.Contains("읽기 실패") && vm.CommunityListSummary.Contains("미확인"), "Read failure presented a fabricated empty count");
                    await CaptureMatrix(NewView(vm), vm, output, "fixture-read-failure", reports);
                }

                using (var vm = NewViewModel(output))
                {
                    var readable = Topic("failed-group", "요청: 같은 주제 일부 원문 읽기 실패", 2);
                    var missingPath = "isolated-memory-only/failed-group/unreadable.md";
                    var unreadable = new CommunicationInboxItem("isolated-ui-qa", CommunicationService.ContentHash("unavailable"),
                        missingPath, missingPath, FixtureTime)
                    { Body = null, Title = "같은 주제의 조회 실패 원문", IsThread = true,
                        ThreadGroup = readable.ThreadGroup, IsStandaloneThreadRecord = true };
                    ApplySnapshot(vm, new([], [], [], [readable, unreadable],
                        [new("isolated-ui-qa", "thread_retry", "격리 검증: 같은 주제의 원문 하나를 읽지 못함")], FixtureTime, []));
                    Check(vm.InboxItems.Count == 1, "Same-group sources did not remain one topic");
                    var row = vm.InboxItems.Single();
                    Check(!row.CountsKnown && row.ContentIssue == "읽기 실패" && row.CountLabel.Contains("읽기 실패")
                        && !row.CountLabel.Contains("댓글 0") && row.ReplyStatusKey == "Unknown", "Partial thread read failure presented known/zero counts");
                    Check(!row.HasNewComments && row.NewCommentLabel.Contains("미확인"), "Failed thread reported a known new-comment count");
                    vm.SelectedInbox = row;
                    await CaptureMatrix(NewView(vm), vm, output, "fixture-same-group-partial-read-failure", reports);
                }

                using (var vm = NewViewModel(output))
                {
                    var records = Enumerable.Range(0, 600).Select(i => Topic("stress-" + i.ToString("D3"), "요청: 격리 긴 목록 검증 " + i + " · 실제 업무 아님", i % 4)).ToArray();
                    ApplySnapshot(vm, Snapshot(records));
                    Check(vm.InboxItems.Count == 600, "600-topic fixture projection");
                    var view = NewView(vm);
                    foreach (var dark in new[] { false, true })
                    {
                        ApplyDetachedTheme(view,dark);
                        await Flush(); Layout(view, 700, 560);
                        var list = (ListBox)view.FindName("InboxList");
                        var last = vm.InboxItems[^1];
                        list.ScrollIntoView(last); await Flush(); Layout(view, 700, 560);
                        list.SelectedItem = last; await Flush(); Layout(view, 700, 560);
                        var scroll = Descendants(list).OfType<ScrollViewer>().First();
                        scroll.ScrollToEnd(); await Flush(); Layout(view, 700, 560);
                        var realized = Descendants(list).OfType<ListBoxItem>().Count();
                        Check(realized > 0 && realized < 600, "600-topic list did not virtualize");
                        Check(list.ItemContainerGenerator.ContainerFromItem(last) is ListBoxItem, "Final topic was not realized after scrolling");
                        Check(ReferenceEquals(vm.SelectedInbox, last), "Final topic selection lost identity");
                        Check(vm.CommunityComments.Count == last.CommentCount, "Final topic rendered wrong comments");
                        Capture(view, 700, 560, Path.Combine(output, $"fixture-600-topics-last-700-{(dark ? "dark" : "light")}.png"));
                        reports.Add(new { Scenario = "600 in-memory topics", Width = 700, Height = 560, Dark = dark,
                            RealizedRows = realized, LastSelected = last.Title, LastRowRealized = true, vm.CommunityListSummary });
                    }
                }
                Check(app.Windows.Count == 0, "A Window was created by the offscreen host");
                Check(SourcesEqual(protectedBefore, ProtectSources()), "Protected THREAD/user-read bytes or existence changed during QA");
                File.WriteAllText(Path.Combine(output, "communication-ui-proof.json"), JsonSerializer.Serialize(new
                {
                    Pass = true, ApplicationType = app.GetType().FullName,
                    Rendering = "CommunicationWorkspaceView measured/arranged into RenderTargetBitmap; no Window, no HWND, no Show",
                    NativeWindowsCreated = 0, NativeWindowsShown = 0, AllHwndsZero = true,
                    OperationalDeployment = false, PollingEnabled = false,
                    BridgeCommands = 0, SynchronizationCalls = 0, CollectionCalls = 0,
                    PersistentUserReadWrites = 0, NoticeAcknowledgements = 0, CommentWrites = 0,
                    FixtureStorage = "memory only", ProtectedSourceBytesUnchanged = true,
                    ProtectedSourcesBefore = protectedBefore, ProtectedSourcesAfter = ProtectSources(), Reports = reports
                }, JsonOptions));
            }
            catch (Exception ex)
            {
                code = 1;
                File.WriteAllText(Path.Combine(output, "communication-ui-error.txt"), ex.ToString());
                File.WriteAllText(Path.Combine(output, "communication-ui-proof.json"), JsonSerializer.Serialize(new
                { Pass = false, Rendering = "Offscreen view only; no operational window", Error = ex.ToString(),
                    ProtectedSourceBytesUnchanged = SourcesEqual(protectedBefore, ProtectSources()), Reports = reports }, JsonOptions));
            }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        }));
        Dispatcher.Run();
        return code;
    }

    private static MainViewModel NewViewModel(string output) => new(new ControlTowerSettings
    {
        RootPath = output, CommunicationHubPath = Hub, TransientReadOnly = true, IsTemporary = true,
        TransientDataDirectory = output, ReduceMotion = true, AutoCommunication = false, AutoPublishCommunication = false,
        MultiplayerServerRoot = output, CommunicationFolders = [], ContinuousStatePaths = []
    }, enablePolling: false);

    private static CommunicationWorkspaceView NewView(MainViewModel vm)
    {
        Check(vm.IsReadOnlyView && !vm.CanRunRegisteredTasks && !vm.CanReadLiveStatus, "Read-only admission");
        var view = new CommunicationWorkspaceView { DataContext = vm };
        view.SetResourceReference(Control.ForegroundProperty, "TextBrush");
        view.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        return view;
    }
    private static void ApplySnapshot(MainViewModel vm, CommunicationSnapshot snapshot) => Apply.Invoke(vm, [snapshot]);
    private static CommunicationSnapshot Snapshot(IReadOnlyList<CommunicationInboxItem> items) => new([], [], [], items, [], FixtureTime, []);
    private static InboxRow Find(MainViewModel vm, string id) => vm.InboxItems.Single(r => r.Path.Contains("/" + id + "/", StringComparison.Ordinal)
        || r.Path.Contains("\\" + id + "\\", StringComparison.Ordinal));

    private static CommunicationInboxItem Topic(string id, string title, int comments)
    {
        var path = "isolated-memory-only/" + id + "/THREAD.md";
        // Keep the fixture's post heading distinct from the parser's explicit comment marker.
        // The visible topic title may describe comment scenarios, but is not a reply label.
        var body = "# " + title + "\n\n## 2026-10-08 10:00 | QA | 요청: 격리 검증 " + id + "\n작성자: 격리 QA\n실제 업무가 아닌 메모리 검증 자료입니다.\n";
        for (var i = 0; i < comments; i++)
            body += $"\n## 2026-10-08 10:{i + 1:D2} | QA | 댓글: 검증 {i + 1}\n작성자: 격리 QA\n" + string.Join("\n", Enumerable.Repeat("이 댓글은 메모리 검증 자료이며 실제 응답이나 사용자 기록을 생성하지 않습니다.", 3)) + "\n";
        return new("isolated-ui-qa", CommunicationService.ContentHash(body), path, path, FixtureTime)
        { Body = body, Title = title, Preview = "격리 검증 자료 · 실제 업무 아님", IsThread = true,
            ThreadGroup = "isolated-memory-only/" + id, IsStandaloneThreadRecord = false };
    }
    private static CommunicationInboxItem TaskRecord(string id, string title, string state, string response, string responseSource)
    {
        var body = JsonSerializer.Serialize(new
        {
            schemaVersion = 1, recordType = "task_exchange", recordId = id, revision = 1, title,
            projectId = "isolated-ui-qa", actorId = "isolated-qa", sessionId = "memory-only",
            receivedAt = FixtureTime.ToString("O"), updatedAt = FixtureTime.ToString("O"),
            request = new { summary = "격리 QA 요청", details = "실제 사용자 요청이나 수신 기록 아님", source = "isolated_fixture" },
            response = new { summary = response, details = "메모리 상태 검증용", source = responseSource }, status = state,
            workDone = new[] { "격리 fixture" }, verification = Array.Empty<object>(),
            nextActions = state == "completed" ? Array.Empty<string>() : ["격리 다음 단계"], blockers = Array.Empty<string>(), supersedes = Array.Empty<string>()
        });
        var path = "isolated-memory-only/" + id + "/content.json";
        return new("isolated-ui-qa", CommunicationService.ContentHash(body), path, path, FixtureTime)
        { Body = body, Title = title, Preview = "메모리 task fixture" };
    }

    private static async Task CaptureMatrix(CommunicationWorkspaceView view, MainViewModel vm, string output, string scenario, List<object> reports)
    {
        foreach (var dark in new[] { false, true })
        foreach (var size in new[] { (Width: 1060, Height: 720), (Width: 700, Height: 560) })
        {
            ApplyDetachedTheme(view,dark); await Flush(); Layout(view, size.Width, size.Height);
            var list = (ListBox)view.FindName("InboxList");
            if (vm.SelectedInbox is { } selected) list.ScrollIntoView(selected);
            await Flush(); Layout(view, size.Width, size.Height);
            if (vm.SelectedInbox is { } selectedRow)
            {
                var scroll = Descendants(list).OfType<ScrollViewer>().First();
                scroll.ScrollToVerticalOffset(vm.InboxItems.IndexOf(selectedRow));
                await Flush(); Layout(view, size.Width, size.Height);
            }
            Check(PresentationSource.FromVisual(view) is null, "A native presentation source was created");
            Check(Application.Current.Windows.Cast<Window>().All(w => new WindowInteropHelper(w).Handle == IntPtr.Zero), "Unexpected nonzero HWND");
            Check(view.ActualWidth <= size.Width + 1 && list.ActualWidth > 0, "Small view layout bounds");
            if (vm.SelectedInbox is { } row && list.ItemContainerGenerator.ContainerFromItem(row) is ListBoxItem container)
            {
                var texts = Descendants(container).OfType<TextBlock>().ToArray();
                var rowBounds = container.TransformToAncestor(list).TransformBounds(new Rect(container.RenderSize));
                Check(rowBounds.Bottom > 0 && rowBounds.Top < list.ActualHeight, "Selected topic is outside the captured viewport");
                Check(texts.Any(t => t.Text == row.CountLabel), "List count label missing");
                Check(texts.Any(t => t.Text == row.ReplyStateLabel), "List text/icon status badge missing");
                Check(texts.Where(t => t.Text == row.CountLabel || t.Text == row.ReplyStateLabel).All(t => t.ActualWidth > 0 && t.ActualHeight > 0), "Count/status badges collapsed");
                foreach (var text in texts.Where(t => t.Text == row.CountLabel || t.Text == row.ReplyStateLabel))
                {
                    var bounds = text.TransformToAncestor(container).TransformBounds(new Rect(text.RenderSize));
                    Check(bounds.Left >= -1 && bounds.Right <= container.ActualWidth + 1, "Count/status text extends beyond the narrow topic row");
                    Check(text.DesiredSize.Width <= text.ActualWidth + 1, "Count/status text desired width exceeds its arranged width");
                    if (text.Text == row.ReplyStateLabel && VisualTreeHelper.GetParent(text) is Border badge)
                    {
                        var badgeBounds = badge.TransformToAncestor(container).TransformBounds(new Rect(badge.RenderSize));
                        Check(badgeBounds.Left >= -1 && badgeBounds.Right <= container.ActualWidth + 1, "Status badge right edge exceeds narrow topic row");
                    }
                }
            }
            Capture(view, size.Width, size.Height, Path.Combine(output, $"{scenario}-{size.Width}-{(dark ? "dark" : "light")}.png"));
            reports.Add(new { Scenario = scenario, size.Width, size.Height, Dark = dark, vm.CommunityListSummary,
                Selected = vm.SelectedInbox?.Title, CountLabel = vm.SelectedInbox?.CountLabel, ExtraCountLabel = vm.SelectedInbox?.ExtraCountLabel,
                ReplyStateLabel = vm.SelectedInbox?.ReplyStateLabel, NewCommentLabel = vm.SelectedInbox?.NewCommentLabel,
                CommentCards = vm.CommunityComments.Count, NativePresentationSource = false });
        }
    }
    private static Dictionary<string, string> ProtectSources()
    {
        var paths = Directory.EnumerateFiles(Path.Combine(Hub, "04_COMMUNICATION", "threads"), "THREAD.md", SearchOption.AllDirectories).ToList();
        paths.Add(Path.Combine(ControlTowerSettings.DataDirectory, "communication-user-read.json"));
        paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower", "communication-user-read.json"));
        return paths.Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(p => p,
            p => File.Exists(p) ? Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(p))) : "absent", StringComparer.OrdinalIgnoreCase);
    }
    private static bool SourcesEqual(Dictionary<string, string> before, Dictionary<string, string> after) =>
        before.Count == after.Count && before.All(p => after.TryGetValue(p.Key, out var hash) && hash == p.Value);
    private static void ApplyDetachedTheme(FrameworkElement view,bool dark)
    {
        ThemeService.Apply(dark);
        // A detached view has no Window through which WPF broadcasts Application resource
        // invalidation. Mirror the current palette locally to exercise the same actual styles.
        foreach(System.Collections.DictionaryEntry resource in Application.Current.Resources)
            if(resource.Key is string key && resource.Value is Brush) view.Resources[key]=resource.Value;
    }
    private static ResourceDictionary Resources()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ai-control-tower", "src", "AIControlTower", "App.xaml"))) directory = directory.Parent;
        if (directory is null) throw new DirectoryNotFoundException("Hub source root not found");
        var path = Path.Combine(directory.FullName, "ai-control-tower", "src", "AIControlTower", "App.xaml");
        var root = XDocument.Load(path).Root!;
        var dictionary = new XElement(root.Name.Namespace + "ResourceDictionary", root.Attributes().Where(a => a.IsNamespaceDeclaration), root.Elements().Single().Nodes());
        foreach (var declaration in dictionary.Attributes().Where(a => a.IsNamespaceDeclaration && a.Value.StartsWith("clr-namespace:AIControlTower") && !a.Value.Contains(";assembly=")))
        {
            var ns = declaration.Value; declaration.Value += ";assembly=AIControlTower";
            foreach (var element in dictionary.Descendants().Where(e => e.Name.NamespaceName == ns)) element.Name = XName.Get(element.Name.LocalName, declaration.Value);
        }
        return (ResourceDictionary)XamlReader.Parse(dictionary.ToString(), new ParserContext { BaseUri = new Uri(path) });
    }
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            foreach (var child in Descendants(VisualTreeHelper.GetChild(root, i))) yield return child;
    }
    private static void Check(bool condition, string name) { if (!condition) throw new InvalidOperationException(name); }
    private static Task Flush() => Dispatcher.CurrentDispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;
    private static void Layout(FrameworkElement root, int width, int height)
    { root.Measure(new Size(width, height)); root.Arrange(new Rect(0, 0, width, height)); root.UpdateLayout(); }
    private static void Capture(FrameworkElement root, int width, int height, string path)
    {
        var canvas = new DrawingVisual();
        using (var dc = canvas.RenderOpen()) dc.DrawRectangle((Brush)Application.Current.Resources["CanvasBrush"], null, new Rect(0, 0, width, height));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32); bitmap.Render(canvas); bitmap.Render(root);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); encoder.Save(stream);
    }
}
