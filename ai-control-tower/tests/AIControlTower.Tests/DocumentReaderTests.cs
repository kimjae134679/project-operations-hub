using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Documents;

namespace AIControlTower.Tests;

public sealed class DocumentReaderTests : IDisposable
{
    private readonly string _root = Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks", "reader-tests-" + Guid.NewGuid().ToString("N"));
    public DocumentReaderTests() => Directory.CreateDirectory(_root);
    private static Type ReaderType => typeof(AIControlTower.Services.ProcessRunner).Assembly.GetType("AIControlTower.Services.DocumentReaderService")!;
    private string Write(string name, string text) { var p = Path.Combine(_root, name); File.WriteAllText(p, text); return p; }
    private async Task<object> Read(string path, string? root = null)
    {
        Assert.NotNull(ReaderType);
        var method = ReaderType.GetMethod("ReadAsync")!;
        var task = (Task)method.Invoke(Activator.CreateInstance(ReaderType), [root ?? _root, path, CancellationToken.None])!;
        await task; return task.GetType().GetProperty("Result")!.GetValue(task)!;
    }
    private static T Value<T>(object o, string name) => (T)o.GetType().GetProperty(name)!.GetValue(o)!;
    [Fact] public void InternalReaderExists() => Assert.NotNull(ReaderType);
    [Fact] public async Task ReadsKoreanMarkdownWithoutChangingSource()
    {
        const string body = "# 사용 안내\n\n**입력**과 결과\n- 순서\n```powershell\nGet-Date\n```";
        var path = Write("프로젝트_사용안내.md", body); var before = File.ReadAllBytes(path); var d = await Read(path);
        Assert.True(Value<bool>(d,"Success")); Assert.Equal(body,Value<string>(d,"RawText"));
        Assert.Equal("markdown",Value<string>(d,"Format")); Assert.Equal(before,File.ReadAllBytes(path));
    }
    [Fact] public async Task RootEscapeAndRelativeRootsDoNotExposeContent()
    {
        var sibling = Write("outside.txt","DO NOT SHOW"); var inside = Path.Combine(_root,"inside"); Directory.CreateDirectory(inside);
        foreach (var root in new[]{inside,"relative-root"}) { var d = await Read(sibling,root); Assert.False(Value<bool>(d,"Success")); Assert.Empty(Value<string>(d,"RawText")); }
    }
    [Theory] [InlineData("config.json")] [InlineData("settings.json")] [InlineData(".env")] [InlineData("credentials.txt")] [InlineData("index.html")] [InlineData("run.ps1")]
    public async Task SensitiveNamesAndExecutableFormatsAreHeld(string name)
    { var d = await Read(Write(name,"PRIVATE ORIGINAL")); Assert.False(Value<bool>(d,"Success")); Assert.Empty(Value<string>(d,"RawText")); }
    [Fact] public async Task SecretValuesAreNotDisplayedEvenInAllowedTextFile()
    { var d=await Read(Write("작업.log","api_key = sk-" + new string('x',30))); Assert.False(Value<bool>(d,"Success")); Assert.Empty(Value<string>(d,"RawText")); }
    [Fact] public async Task LargeAndMissingFilesHaveExplicitReadErrors()
    {
        foreach(var p in new[]{Write("large.txt",new string('가',300_000)),Path.Combine(_root,"missing.md")})
        { var d=await Read(p); Assert.False(Value<bool>(d,"Success")); Assert.NotEmpty(Value<string>(d,"Error")); Assert.Empty(Value<string>(d,"RawText")); }
    }
    [Fact] public async Task JsonAndLogRemainLiteralAndUtf16BomIsReadable()
    {
        var json=await Read(Write("status.json","{\"state\":\"running\"}")); Assert.Equal("text",Value<string>(json,"Format"));
        var path=Path.Combine(_root,"utf16.txt"); File.WriteAllText(path,"한글 사용 안내",System.Text.Encoding.Unicode);
        Assert.Equal("한글 사용 안내",Value<string>(await Read(path),"RawText"));
    }
    [Fact] public async Task LongLinesWhitespaceAndCrLfAreNotTruncatedByProcessLogFormatting()
    {
        var body="  "+new string('가',900)+"  \r\n  끝  \r\n";
        Assert.Equal(body,Value<string>(await Read(Write("long.md",body)),"RawText"));
    }
    [Fact] public async Task TerminalEscapeSequencesAreStrippedWithoutRunningThem()
    { Assert.Equal("안내",Value<string>(await Read(Write("terminal.log","\u001b[31m안내\u001b[0m")),"RawText")); }
    [Fact] public void MarkdownRendererNeverCreatesClickableLinksImagesOrExecutableUi() => OnSta(() =>
    {
        var type=ReaderType; Assert.NotNull(type);
        var doc=(FlowDocument)type.GetMethod("RenderMarkdown")!.Invoke(null,["# 제목\n[외부](https://example.com)\n![이미지](../../private.png)\n<script>alert(1)</script>\n```cmd\nremove-data\n```",17d])!;
        Assert.DoesNotContain(doc.Blocks,b=>b is BlockUIContainer);
        Assert.DoesNotContain(doc.Blocks.OfType<Paragraph>().SelectMany(p=>p.Inlines),i=>i is Hyperlink || i is InlineUIContainer);
        Assert.Contains("remove-data",new TextRange(doc.ContentStart,doc.ContentEnd).Text);
    });
    [Fact] public void MarkdownPreviewBoundsWpfBlocksWithoutDroppingTheRawDocument() => OnSta(() =>
    {
        var body=string.Join("\n",Enumerable.Repeat("작은 줄",5000));
        var doc=(FlowDocument)ReaderType.GetMethod("RenderMarkdown")!.Invoke(null,[body,16d])!;
        Assert.True(doc.Blocks.Count<=2001); Assert.Contains("원문 보기",new TextRange(doc.ContentStart,doc.ContentEnd).Text);
    });
    [Fact] public async Task ViewModelSearchAndWrapKeepOriginalText()
    {
        var vmType=ReaderType.Assembly.GetType("AIControlTower.ViewModels.DocumentReaderViewModel"); Assert.NotNull(vmType);
        var vm=Activator.CreateInstance(vmType)!; var body="첫 줄 안내\n두 번째 안내\n끝"; var path=Write("검색.txt",body);
        await (Task)vmType.GetMethod("OpenAsync")!.Invoke(vm,[_root,path,"현재 문서"])!;
        vmType.GetProperty("SearchText")!.SetValue(vm,"안내"); Assert.Equal(2,Value<int>(vm,"MatchCount"));
        vmType.GetProperty("WrapText")!.SetValue(vm,false); Assert.Equal(body,Value<string>(vm,"RawText"));
        Assert.Equal("현재 문서",Value<string>(vm,"Title")); Assert.Equal(path,Value<string>(vm,"FilePath"));
    }
    [Fact] public async Task OlderAsyncReadCannotReplaceTheNewSelection()
    {
        var first=new TaskCompletionSource<AIControlTower.Models.ReaderDocument>();
        var vm=new AIControlTower.ViewModels.DocumentReaderViewModel((root,path,ct)=>path=="first"?first.Task:Task.FromResult(new AIControlTower.Models.ReaderDocument(path,"second","둘째","text","")));
        var old=vm.OpenAsync("root","first"); await vm.OpenAsync("root","second");
        first.SetResult(new("first","first","낡은 본문","text","")); await old;
        Assert.Equal("둘째",vm.RawText); Assert.Equal("second",vm.FilePath); Assert.False(vm.IsLoading);
    }
    [Fact] public async Task RootSiblingPrefixAndAlternateStreamsAreRejected()
    {
        var root=Path.Combine(_root,"project"); Directory.CreateDirectory(root); var sibling=Path.Combine(_root,"project-copy"); Directory.CreateDirectory(sibling);
        var outside=Path.Combine(sibling,"note.txt"); File.WriteAllText(outside,"외부 자료");
        Assert.False(Value<bool>(await Read(outside,root),"Success"));
        Assert.False(Value<bool>(await Read(Write("stream.txt","기본 자료")+":private"),"Success"));
    }
    [Fact] public void PcGuideUsesExplicitInternalReaderDelegateWithoutOpeningAnotherWindow()
    {
        var constructor=typeof(AIControlTower.ViewModels.PcConnectionViewModel).GetConstructors().FirstOrDefault(c=>c.GetParameters().Length==2);
        Assert.NotNull(constructor); string? seenRoot=null,seenPath=null,seenTitle=null;
        Func<string,string,string,Task> open=(root,path,title)=>{seenRoot=root;seenPath=path;seenTitle=title;return Task.CompletedTask;};
        using var vm=(AIControlTower.ViewModels.PcConnectionViewModel)constructor.Invoke([new AIControlTower.Services.ProjectBridgeService(_root),open]);
        vm.OpenGuideCommand.Execute(null);
        Assert.Equal(_root,seenRoot); Assert.Equal(Path.Combine(_root,"프로젝트_사용안내.md"),seenPath); Assert.Equal("공용 PC 연결",seenTitle);
    }
    [Fact] public void AllPrimaryToolGuideRoutesStopUsingStandaloneWindowFallbacks()
    {
        var source=new DirectoryInfo(AppContext.BaseDirectory);
        while(source is not null && !File.Exists(Path.Combine(source.FullName,"ai-control-tower","AIControlTower.sln"))) source=source.Parent;
        Assert.NotNull(source);
        foreach(var file in new[]{"ToolActionsViewModel.cs","PcConnectionViewModel.cs"})
        {
            var body=File.ReadAllText(Path.Combine(source!.FullName,"ai-control-tower","src","AIControlTower","ViewModels",file));
            Assert.DoesNotContain("new ProjectGuideWindow",body);
        }
    }
    [Fact] public void PcGuideWithoutReaderHostShowsErrorAndNeverFallsBackToAnotherApp()
    {
        using var vm=new AIControlTower.ViewModels.PcConnectionViewModel(new AIControlTower.Services.ProjectBridgeService(_root));
        vm.OpenGuideCommand.Execute(null); Assert.True(vm.HasError); Assert.Contains("내부 문서 읽기",vm.DetailText);
    }
    private static void OnSta(Action action)
    {
        Exception? error=null; var t=new Thread(()=>{try{action();}catch(Exception e){error=e;}}); t.SetApartmentState(ApartmentState.STA); t.Start(); t.Join();
        if(error is not null) ExceptionDispatchInfo.Capture(error).Throw();
    }
    public void Dispose()
    {
        var checks=Path.GetFullPath(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks")+Path.DirectorySeparatorChar;
        var resolved=Path.GetFullPath(_root);
        if(!resolved.StartsWith(checks,StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("reader-tests-",StringComparison.Ordinal)) throw new InvalidOperationException("Unexpected test cleanup root.");
        if(Directory.Exists(resolved)) Directory.Delete(resolved,true);
    }
}
