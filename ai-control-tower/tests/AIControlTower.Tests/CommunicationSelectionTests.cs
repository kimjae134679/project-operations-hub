using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xml.Linq;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;
using Xunit;

namespace AIControlTower.Tests;

public sealed class CommunicationSelectionTests
{
    [Fact]
    public void DetachedTopicSelectionImmediatelyUpdatesBodyAndAllComments() => OnSta(() =>
    {
        using var vm = NewVm();
        Apply(vm, Snapshot());
        var (host, topics, entries, reader) = BindWorkspace(vm);
        Layout(host);
        foreach (var topic in new[] { vm.InboxItems[1], vm.InboxItems[2], vm.InboxItems[0] })
        {
            topics.SetCurrentValue(Selector.SelectedItemProperty, topic);
            Pump();
            Assert.Same(topic, vm.SelectedInbox);
            Assert.Equal(topic.ThreadEntries.Select(e => e.Identity), vm.Entries.Select(e => e.Entry.Identity));
            Assert.Equal(vm.CommunityComments, entries.Items.Cast<CommunityCommentRow>());
            Assert.NotNull(vm.SelectedEntry);
            Assert.Contains(topic.Title + " 본문", new TextRange(reader.Document.ContentStart, reader.Document.ContentEnd).Text);
        }
    });

    [Fact]
    public void TopicSelectionNotificationPublishesMatchingEntriesNotPreviousTopic() => OnSta(() =>
    {
        using var vm = NewVm();
        Apply(vm, Snapshot());
        var (host, topics, _, _) = BindWorkspace(vm);
        Layout(host);
        string[]? entriesAtSelection = null;
        string? bodyAtSelection = null;
        var target = vm.InboxItems[1];
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(MainViewModel.SelectedInbox) || !ReferenceEquals(vm.SelectedInbox, target)) return;
            entriesAtSelection = vm.Entries.Select(r => r.Entry.Identity).ToArray();
            bodyAtSelection = vm.SelectedBody;
        };
        topics.SetCurrentValue(Selector.SelectedItemProperty, target);
        Pump();
        Assert.Equal(target.ThreadEntries.Select(e => e.Identity), entriesAtSelection);
        Assert.Contains(target.Title + " 본문", bodyAtSelection);
    });

    [Fact]
    public void DetachedInlineCommentsStayVisibleAndUnchangedRefreshKeepsChosenTopic() => OnSta(() =>
    {
        using var vm = NewVm();
        Apply(vm, Snapshot());
        var (host, topics, entries, reader) = BindWorkspace(vm);
        Layout(host);
        var topic = vm.InboxItems[2];
        topics.SetCurrentValue(Selector.SelectedItemProperty, topic);
        Pump();
        var reply = Assert.Single(entries.Items.Cast<CommunityCommentRow>());
        Assert.True(reply.Entry.IsComment);
        Assert.Contains(topic.Title + " 답변", reply.Body);
        Assert.Contains(topic.Title + " 본문", new TextRange(reader.Document.ContentStart, reader.Document.ContentEnd).Text);
        Apply(vm, Snapshot());
        Pump();
        Assert.Same(topic, topics.SelectedItem);
        Assert.Same(reply, Assert.Single(entries.Items.Cast<CommunityCommentRow>()));
        Assert.Contains(topic.Title + " 답변", Assert.Single(vm.CommunityComments).Body);
    });

    [Fact]
    public void TopicIdentityHashIsStableAcrossReadStateAndTemplateSubscriptions()
    {
        var row = new InboxRow("프로젝트", "주제", "미리보기", "본문", "원본", "시각");
        var hash = row.GetHashCode();
        row.IsUnread = true;
        Assert.Equal(hash, row.GetHashCode());
        row.PropertyChanged += (_, _) => { };
        Assert.Equal(hash, row.GetHashCode());
        row.IsUnread = false;
        Assert.Equal(hash, row.GetHashCode());
        Assert.NotEqual(row, new InboxRow("프로젝트", "주제", "미리보기", "본문", "원본", "시각"));
    }

    [Fact]
    public void ActualViewTemplateContainerSelectionSurvivesQueuedBackgroundRefresh() => OnSta(() =>
    {
        using var vm = NewVm();
        Apply(vm, Snapshot());
        var view = ParseActualView();
        view.DataContext = vm;
        Layout(view);
        var topics = (ListBox)view.FindName("InboxList");
        var entries = (ItemsControl)view.FindName("CommentsList");
        var reader = (RichTextBox)view.FindName("PostBody");
        var target = vm.InboxItems[1];
        var snapshot = Snapshot();
        var refreshed = snapshot with
        {
            InboxItems = snapshot.InboxItems.Select((row, i) => i == 0 ? row with { Body = row.Body + "\n다른 주제의 새 기록", ContentSha256 = "changed" } : row).ToArray(),
            LastSync = snapshot.LastSync.AddMinutes(1)
        };
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => Apply(vm, refreshed)));
        var container = Assert.IsType<ListBoxItem>(topics.ItemContainerGenerator.ContainerFromItem(target));
        container.IsSelected = true; // Real selector/container route, no mouse/keyboard or Window.
        Pump();
        Assert.Same(target, topics.SelectedItem);
        Assert.Same(target, vm.SelectedInbox);
        Assert.Equal(target.ThreadEntries.Select(e => e.Identity), vm.Entries.Select(e => e.Entry.Identity));
        Assert.Equal(vm.CommunityComments, entries.Items.Cast<CommunityCommentRow>());
        Assert.Contains("운동앱 본문", new TextRange(reader.Document.ContentStart, reader.Document.ContentEnd).Text);
        var visibleReply = Assert.Single(VisualDescendants(entries).OfType<RichTextBox>());
        Assert.Contains("운동앱 답변", new TextRange(visibleReply.Document.ContentStart, visibleReply.Document.ContentEnd).Text);
        Assert.DoesNotContain("Obsidian 본문", vm.SelectedBody);
    });

    [Fact]
    public void PrimaryTopicNavigationUsesListSelectionAndHasNoSearchOrPreviousNextButtons()
    {
        var document = XDocument.Load(ViewPath());
        Assert.DoesNotContain(document.Descendants(), e => e.Name.LocalName == "Button" &&
            (string?)e.Attribute("Command") is "{Binding PreviousPostCommand}" or "{Binding NextPostCommand}");
        Assert.DoesNotContain(document.Descendants(), e => e.Name.LocalName is "TextBox" or "ComboBox" && ((string?)e.Attribute("Text") ?? (string?)e.Attribute("ItemsSource") ?? "").Contains("Search"));
    }

    [Fact]
    public void ReadingPaneDoesNotRepeatLongPolicyExplanationOrFullSourcePath()
    {
        var document = XDocument.Load(ViewPath());
        Assert.DoesNotContain(document.Descendants(), e => e.Name.LocalName == "TextBlock" &&
            (string?)e.Attribute("Text") is "{Binding UserReadExplanation}" or "{Binding SelectedEntry.SourceLabel}");
        Assert.DoesNotContain(document.Descendants(), e => (string?)e.Attribute("Text") == "{Binding CentralSyncMessage}");
    }

    private static (Grid Host, ListBox Topics, ItemsControl Entries, RichTextBox Reader) BindWorkspace(MainViewModel vm)
    {
        var document = XDocument.Load(ViewPath());
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        ListBox CopyList(string name)
        {
            var source = document.Descendants().Single(e => e.Name.LocalName == "ListBox" && (string?)e.Attribute(x + "Name") == name);
            // Exercise the actual view's bindings with real selectors, but no Window/application or input injection.
            var element = new XElement("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}ListBox",
                new XAttribute("ItemsSource", source.Attribute("ItemsSource")!.Value),
                new XAttribute("SelectedItem", source.Attribute("SelectedItem")!.Value));
            return (ListBox)XamlReader.Parse(element.ToString());
        }
        var topics = CopyList("InboxList");
        var commentSource = document.Descendants().Single(e => e.Name.LocalName == "ItemsControl" && (string?)e.Attribute(x + "Name") == "CommentsList");
        var entries = new ItemsControl();
        entries.SetBinding(ItemsControl.ItemsSourceProperty, new System.Windows.Data.Binding(nameof(MainViewModel.CommunityComments)));
        Assert.Equal("{Binding CommunityComments}", commentSource.Attribute("ItemsSource")!.Value);
        var reader = new RichTextBox { IsReadOnly = true };
        reader.SetBinding(NoticeDocument.TextProperty, new System.Windows.Data.Binding(nameof(MainViewModel.SelectedCommunityBody)));
        var host = new Grid { DataContext = vm };
        host.ColumnDefinitions.Add(new() { Width = new GridLength(250) });
        host.ColumnDefinitions.Add(new() { Width = new GridLength(250) });
        host.ColumnDefinitions.Add(new());
        Grid.SetColumn(entries, 1); Grid.SetColumn(reader, 2);
        host.Children.Add(topics); host.Children.Add(entries); host.Children.Add(reader);
        return (host, topics, entries, reader);
    }
    private static IEnumerable<DependencyObject> VisualDescendants(DependencyObject root)
    {
        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var descendant in VisualDescendants(child)) yield return descendant;
        }
    }
    private static UserControl ParseActualView()
    {
        var root = XDocument.Load(ViewPath()).Root!;
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        root.Attribute(x + "Class")!.Remove();
        foreach (var attribute in root.DescendantsAndSelf().Attributes().Where(a => a.Name.LocalName == "Click").ToArray()) attribute.Remove();
        var appPath = Path.Combine(SourceRoot(), "ai-control-tower", "src", "AIControlTower", "App.xaml");
        var app = XDocument.Load(appPath).Root!;
        var dictionary = new XElement(app.Name.Namespace + "ResourceDictionary", app.Attributes().Where(a => a.IsNamespaceDeclaration), app.Elements().Single().Nodes());
        foreach (var declaration in dictionary.Attributes().Where(a => a.IsNamespaceDeclaration && a.Value.StartsWith("clr-namespace:AIControlTower") && !a.Value.Contains(";assembly=")))
        {
            var ns = declaration.Value; declaration.Value += ";assembly=AIControlTower";
            foreach (var element in dictionary.Descendants().Where(e => e.Name.NamespaceName == ns)) element.Name = XName.Get(element.Name.LocalName, declaration.Value);
        }
        var resources = root.Element(root.Name.Namespace + "UserControl.Resources")!;
        var localResources = resources.Nodes().ToArray();
        resources.RemoveNodes();
        dictionary.Add(localResources);
        resources.Add(dictionary);
        var serviceNamespace = root.Attribute(XNamespace.Xmlns + "services")!;
        var originalServiceNamespace = serviceNamespace.Value;
        serviceNamespace.Value += ";assembly=AIControlTower";
        foreach (var attribute in root.DescendantsAndSelf().Attributes().Where(a => a.Name.NamespaceName == originalServiceNamespace).ToArray())
        {
            var owner = attribute.Parent!;
            var replacement = new XAttribute(XName.Get(attribute.Name.LocalName, serviceNamespace.Value), attribute.Value);
            attribute.Remove(); owner.Add(replacement);
        }
        // WPF loose XAML allows one root BaseUri, not nested xml:base. This view has no
        // relative asset URI; its embedded application resources do (Window.Icon).
        // Preserve every production style/setter and resolve those assets from App.xaml.
        return (UserControl)XamlReader.Parse(root.ToString(), new ParserContext { BaseUri = new Uri(appPath) });
    }

    private static CommunicationSnapshot Snapshot()
    {
        var at = DateTimeOffset.Parse("2026-10-07T10:00:00+09:00");
        return new([], [], [], new[] { "Obsidian", "운동앱", "원격" }.Select((name, i) => new CommunicationInboxItem("Test-" + i, "h-" + i, name + ".md", "isolated/" + i + ".md", at)
        {
            Title = name,
            Body = "## 2026-10-07 | Test | " + name + " 게시글\n" + name + " 본문\n## 2026-10-07 | Test | 댓글 " + name + " 답변\n" + name + " 답변"
        }).ToArray(), [], at, []);
    }
    private static MainViewModel NewVm() => new(new ControlTowerSettings
    {
        IsTemporary = true, TransientReadOnly = true, AutoCommunication = false, AutoPublishCommunication = false,
        CommunicationHubPath = Path.Combine(SourceRoot(), "checks", "detached-selection-absent"),
        TransientDataDirectory = Path.Combine(SourceRoot(), "checks", "detached-selection-absent"), CommunicationFolders = []
    }, enablePolling: false);
    private static void Apply(MainViewModel vm, CommunicationSnapshot snapshot) => typeof(MainViewModel)
        .GetMethod("ApplyCommunicationSnapshot", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(vm, [snapshot]);
    private static string ViewPath() => Path.Combine(SourceRoot(), "ai-control-tower", "src", "AIControlTower", "Views", "CommunicationWorkspaceView.xaml");
    private static string SourceRoot()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "ai-control-tower", "AIControlTower.sln"))) root = root.Parent;
        return root?.FullName ?? throw new InvalidOperationException("Source root missing");
    }
    private static void Layout(FrameworkElement host)
    {
        host.Measure(new Size(1000, 600)); host.Arrange(new Rect(0, 0, 1000, 600)); host.UpdateLayout(); Pump();
    }
    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }
    private static void OnSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() => { try { action(); } catch (Exception ex) { error = ex; } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); thread.Join();
        if (error is not null) ExceptionDispatchInfo.Capture(error).Throw();
    }
}
