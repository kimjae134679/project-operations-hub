using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class NavigationPresentationTests
{
    private static PropertyInfo Property(object target,string name)
    { var property=target.GetType().GetProperty(name); Assert.NotNull(property); return property; }
    private static T Get<T>(object target,string name)=>(T)Property(target,name).GetValue(target)!;
    private static void Set(object target,string name,object value)=>Property(target,name).SetValue(target,value);
    private static async Task Call(object target,string method)
    { var member=target.GetType().GetMethod(method); Assert.NotNull(member); await (Task)member.Invoke(target,null)!; }
    private static DocumentReaderViewModel Reader(Func<string,string,CancellationToken,Task<ReaderDocument>>? read=null)
        =>new(read??((root,path,ct)=>Task.FromResult(new ReaderDocument(path,path,"본문 안내\n다음 안내","markdown",""))));

    [Fact] public async Task DocumentBackForwardRestoresSearchSelectionAndSeparateScrollOffsets()
    {
        var vm=Reader(); await vm.OpenAsync("approved-A","A.md","안내 A");
        vm.SearchText="안내";vm.ShowRaw=true;vm.WrapText=false;
        Set(vm,"RawScrollOffset",81d);Set(vm,"FormattedScrollOffset",45d);Set(vm,"SelectionStart",3);Set(vm,"SelectionLength",2);
        await vm.OpenAsync("approved-B","B.md","안내 B");
        Assert.True(Get<bool>(vm,"CanGoBack"));await Call(vm,"GoBackAsync");
        Assert.Equal("A.md",vm.FilePath);Assert.Equal("안내",vm.SearchText);Assert.True(vm.ShowRaw);Assert.False(vm.WrapText);
        Assert.Equal(81d,Get<double>(vm,"RawScrollOffset"));Assert.Equal(45d,Get<double>(vm,"FormattedScrollOffset"));
        Assert.Equal(3,Get<int>(vm,"SelectionStart"));Assert.Equal(2,Get<int>(vm,"SelectionLength"));
        await Call(vm,"GoForwardAsync");Assert.Equal("B.md",vm.FilePath);
    }
    [Fact] public async Task HistoryRevalidatesOriginalApprovedRootRatherThanCachingPrivateBodies()
    {
        var reads=new List<(string,string)>();var held=false;
        var vm=Reader((root,path,ct)=>{reads.Add((root,path));return Task.FromResult(new ReaderDocument(path,path,held?"":"ordinary", "text",held?"읽기 보류":""));});
        await vm.OpenAsync("root-A","A.txt");await vm.OpenAsync("root-B","B.txt");held=true;
        await Call(vm,"GoBackAsync");Assert.Equal(("root-A","A.txt"),reads.Last());Assert.Empty(vm.RawText);Assert.NotEmpty(vm.Error);
    }
    [Fact] public async Task FreshDocumentClearsForwardAndBoundsMetadataHistory()
    {
        var vm=Reader();await vm.OpenAsync("root","A");await vm.OpenAsync("root","B");await Call(vm,"GoBackAsync");
        await vm.OpenAsync("root","C");Assert.False(Get<bool>(vm,"CanGoForward"));
        for(var i=0;i<80;i++)await vm.OpenAsync("root","item"+i);
        Assert.InRange(Get<int>(vm,"HistoryCount"),1,24);
    }
    [Fact] public async Task LateReadCannotOverwriteDocumentChosenByHistoryNavigation()
    {
        var delayed=new TaskCompletionSource<ReaderDocument>();
        var vm=Reader((root,path,ct)=>path=="delayed"?delayed.Task:Task.FromResult(new ReaderDocument(path,path,path,"text","")));
        await vm.OpenAsync("root","A");var old=vm.OpenAsync("root","delayed");await Call(vm,"GoBackAsync");
        delayed.SetResult(new("delayed","late","PRIVATE LATE","text",""));await old;
        Assert.Equal("A",vm.FilePath);Assert.Equal("A",vm.RawText);Assert.False(vm.IsLoading);
    }
    [Fact] public void ReturnToOriginRaisesOnlyAnInMemoryNavigationRequest()
    {
        var vm=Reader();var member=vm.GetType().GetEvent("ReturnRequested");Assert.NotNull(member);var count=0;
        member.AddEventHandler(vm,new EventHandler((_,_)=>count++));Set(vm,"CanReturnToOrigin",true);
        var request=vm.GetType().GetMethod("ReturnToOrigin");Assert.NotNull(request);request.Invoke(vm,null);Assert.Equal(1,count);
        Set(vm,"CanReturnToOrigin",false);request.Invoke(vm,null);Assert.Equal(1,count);
    }
    [Fact] public void OriginSelectionRestoresStableIdsSearchAndCommandWithoutSettingsWritesOrRecovery()
    {
        var root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks","nav-origin-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        var file=Path.Combine(root,"settings.json");File.WriteAllText(file,"{\"RootPath\":\"D:\\\\A_KJ\\\\AI\"}");
        var bytes=File.ReadAllBytes(file);var attrs=File.GetAttributes(file);var calls=0;
        using var vm=new MainViewModel(ControlTowerSettings.LoadReadOnly(file),false,()=>calls++);
        var command=new ProgramCommand{Name="검사",FileName="owned-example.exe",Arguments=["test"]};
        ProjectItem Project()=>new(){Id="project-A",Path=root,Functions=[new(){Id="function-A",Programs=[new(){Id="program-A",Commands=[command]},new(){Id="program-B"}]}]};
        vm.Projects.Add(Project());vm.SelectedProject=vm.Projects[0];vm.SelectedProgram=vm.SelectedProject.Functions[0].Programs[0];vm.SelectedCommand=command;
        vm.ProjectSearch="프로젝트";vm.ProgramSearch="검사";vm.ToolSearch="Codex";vm.WorkDashboard.Search="작업";vm.ManagementDashboard.Search="통합";
        var helper=typeof(MainViewModel).Assembly.GetType("AIControlTower.Services.WorkspaceReaderSelection");Assert.NotNull(helper);
        var capture=helper.GetMethod("Capture");Assert.NotNull(capture);var snapshot=capture.Invoke(null,[vm]);Assert.NotNull(snapshot);
        vm.Projects.Clear();vm.Projects.Add(Project());vm.SelectedProject=vm.Projects[0];vm.SelectedProgram=vm.SelectedProject.Functions[0].Programs[1];vm.ProgramSearch="바뀜";vm.ProjectSearch="";
        var restore=helper.GetMethod("Restore");Assert.NotNull(restore);restore.Invoke(snapshot,[vm]);
        Assert.Equal("program-A",vm.SelectedProgram?.Id);Assert.Equal(command,vm.SelectedCommand);Assert.Equal("프로젝트",vm.ProjectSearch);Assert.Equal("검사",vm.ProgramSearch);Assert.Equal("Codex",vm.ToolSearch);
        Assert.Equal("작업",vm.WorkDashboard.Search);Assert.Equal("통합",vm.ManagementDashboard.Search);Assert.Equal(0,calls);
        Assert.Equal(bytes,File.ReadAllBytes(file));Assert.Equal(attrs,File.GetAttributes(file));Assert.Single(Directory.GetFiles(root));
    }
    [Fact] public void InstallationOnlyDoesNotMakeCandidateDefaultVisibleButSearchCanFindIt()
    {
        var settings=ControlTowerSettings.LoadReadOnly(Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks","nav-missing-"+Guid.NewGuid().ToString("N"),"settings.json"));
        using var vm=new MainViewModel(settings,false,()=>throw new InvalidOperationException("No recovery"));
        var row=new ToolStatusViewModel(new("aider","Aider",StatusKind.Ready,"설치만 확인",DateTimeOffset.Now));vm.Statuses.Add(row);
        Assert.False(Get<bool>(row,"HasUsageEvidence"));Assert.Empty(vm.FilteredToolStatuses);
        vm.ToolSearch="Aider";Assert.Single(vm.FilteredToolStatuses);vm.ToolSearch="";Set(vm,"ShowOtherTools",true);Assert.Single(vm.FilteredToolStatuses);
        Assert.True(row.IsUserFacing);Assert.NotEqual("실행 중",row.StateLabel);
    }
    [Theory] [InlineData("n8n","n8n")] [InlineData("ai-ops-runner","GitHub Actions")] [InlineData("delivery-chain","Jev")] [InlineData("voicestudio","VoiceStudio")]
    public void ToolNamesAndRolesAreStableEvenWhenReadOnlyStatusUsesProviderId(string id,string product)
    {
        var vm=new ToolStatusViewModel(new(id,id,StatusKind.Unknown,"API 확인 없음",DateTimeOffset.Now));
        Assert.Contains(product,vm.DisplayName);Assert.DoesNotContain("등록된 도구의 확인 결과",vm.Purpose);
    }
    [Fact] public void MainTabsUseNamedIdentityAndRequestedTaskFirstOrder()
    {
        var source=XDocument.Load(Source("MainWindow.xaml"));var tabs=source.Descendants().First(e=>e.Name.LocalName=="TabControl");
        var names=tabs.Elements().Select(e=>(string?)e.Attribute(XName.Get("Name","http://schemas.microsoft.com/winfx/2006/xaml"))).ToArray();
        Assert.Equal(new[]{"ProjectsTab","WorkDashboardTab","ManagementProcessTab","PcConnectionTab","CommunicationTab","NoticeTab","DocumentReaderTab","ToolsTab","ServerTab"},names);
        var body=File.ReadAllText(Source("MainWindow.xaml.cs"));Assert.Contains("RestoreReaderOrigin",body);Assert.Contains("_restoringReaderOrigin",body);
    }
    [Fact] public void AppJevHandlerRequiresExplicitVerifiedModelBeforeInspectingLauncherOrSpawning()
    {
        var body=File.ReadAllText(Source("ViewModels/MainViewModel.cs"));var start=body.IndexOf("public async void RunJevTask()",StringComparison.Ordinal);
        var end=body.IndexOf("public Task CancelJevTaskAsync()",start,StringComparison.Ordinal);var method=body[start..end];
        var policy=method.IndexOf("JevExecutionPolicy.TryCreate(evidence.RequestedModel,evidence.IsVerified",StringComparison.Ordinal);
        Assert.True(policy>=0,"Only fresh installation/account-bound evidence permits execution.");
        Assert.True(method.IndexOf("JevExecutionEvidenceReader.ReadDefault",StringComparison.Ordinal)<policy);Assert.True(policy<method.IndexOf("JevNativeLauncher.Prepare",StringComparison.Ordinal));Assert.DoesNotContain("TryCreate(null,true",method);
        var run=typeof(MainViewModel).GetMethod("RunProgramAsync",BindingFlags.Instance|BindingFlags.NonPublic);Assert.NotNull(run);
        Assert.Contains(run.GetParameters(),p=>p.ParameterType.Name=="AiCompletionContract" && p.IsOptional);
    }
    [Fact] public void ProjectPanesOfferNamedSplittersWithoutNegativeOverflowDecoration()
    {
        var x=XDocument.Load(Source("MainWindow.xaml"));var splitters=x.Descendants().Where(e=>e.Name.LocalName=="GridSplitter").ToArray();
        Assert.Contains(splitters,e=>(string?)e.Attribute("AutomationProperties.Name")=="프로젝트 목록 너비 조절");
        Assert.Contains(splitters,e=>(string?)e.Attribute("AutomationProperties.Name")=="프로그램과 상세 너비 조절");
        Assert.Contains(splitters,e=>(string?)e.Attribute("AutomationProperties.Name")=="프로그램과 작업 기록 높이 조절");
        foreach(var border in x.Descendants().Where(e=>e.Name.LocalName=="Border" && e.Attribute("Margin") is not null))
            Assert.DoesNotContain(((string)border.Attribute("Margin")!).Split(','),n=>double.TryParse(n,out var value)&&value<0);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void ActualDetachedTabsExposeOverflowAndKeepFocusBorderWithinHitTarget(bool dark)=>OnSta(()=>
    {
        var resources=Resources(dark);var tabs=new TabControl{Resources=resources,Style=(Style)resources[typeof(TabControl)]};
        for(var i=0;i<9;i++)tabs.Items.Add(new TabItem{Header="긴 한글 프로젝트 작업 메뉴 "+i,Style=(Style)resources[typeof(TabItem)]});
        tabs.Measure(new Size(420,360));tabs.Arrange(new Rect(0,0,420,360));tabs.UpdateLayout();
        var header=Descendants(tabs).OfType<ScrollViewer>().FirstOrDefault(s=>s.Name=="TabHeaderScroll");Assert.NotNull(header);
        Assert.Equal(ScrollBarVisibility.Auto,header.HorizontalScrollBarVisibility);Assert.True(header.ScrollableWidth>0);
        var first=(TabItem)tabs.Items[0];Assert.True(first.ActualHeight>=40);
        var focus=Descendants(first).OfType<Border>().Single(b=>b.Name=="TabFocus");Assert.Equal(new Thickness(0),focus.Margin);
        Assert.True(focus.ActualWidth<=first.ActualWidth);Assert.True(focus.ActualHeight<=first.ActualHeight);
        var splitter=new GridSplitter{Resources=resources,Style=(Style)resources[typeof(GridSplitter)]};
        Assert.True(splitter.ShowsPreview);Assert.Equal(GridResizeBehavior.PreviousAndNext,splitter.ResizeBehavior);
    });
    private static string Source(string file)
    { var dir=new DirectoryInfo(AppContext.BaseDirectory);while(dir is not null&&!File.Exists(Path.Combine(dir.FullName,"ai-control-tower","src","AIControlTower","App.xaml")))dir=dir.Parent;Assert.NotNull(dir);return Path.Combine(dir.FullName,"ai-control-tower","src","AIControlTower",file); }
    private static ResourceDictionary Resources(bool dark)
    {
        var root=XDocument.Load(Source("App.xaml")).Root!;
        var x=new XElement(root.Name.Namespace+"ResourceDictionary",root.Attributes().Where(a=>a.IsNamespaceDeclaration),root.Elements().Single().Nodes());
        foreach(var declaration in x.Attributes().Where(a=>a.IsNamespaceDeclaration&&a.Value.StartsWith("clr-namespace:AIControlTower")&&!a.Value.Contains(";assembly=")))
        {var ns=declaration.Value;declaration.Value+=";assembly=AIControlTower";foreach(var e in x.Descendants().Where(e=>e.Name.NamespaceName==ns))e.Name=XName.Get(e.Name.LocalName,declaration.Value);}
        var dictionary=(ResourceDictionary)XamlReader.Parse(x.ToString(),new ParserContext{BaseUri=new Uri(Source("App.xaml"))});
        foreach(var color in ThemeService.Palette(dark))dictionary[color.Key]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(color.Value));
        return dictionary;
    }
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {yield return root;for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)foreach(var child in Descendants(VisualTreeHelper.GetChild(root,i)))yield return child;}
    private static void OnSta(Action action)
    {Exception? error=null;var thread=new Thread(()=>{try{action();}catch(Exception ex){error=ex;}});thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();if(error is not null)ExceptionDispatchInfo.Capture(error).Throw();}
}
