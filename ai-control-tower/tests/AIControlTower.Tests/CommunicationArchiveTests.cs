using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using AIControlTower.Services;
using Xunit;
namespace AIControlTower.Tests;

public sealed class CommunicationArchiveTests
{
    private const string Archive = "# 주제\n\n## 원본 기록: README.md\n\n### 주제 안내\n\n설명\n\n## 원본 기록: 001-sol.md\n\n### 001 — 작성 기록\n작성자: Sol\nDate: 2026-09-19 KST\n\n첫 기록\n\n## 원본 기록: 002-result.md\n\n### 002 — 결과\n\n날짜와 작성자 없음\n";
    [Fact]
    public void UndatedMergedOriginalsRemainSeparateAndDoNotInventComments()
    {
        var rows=CommunicationThreadParser.Parse(Archive,"topic/THREAD.md","주제");
        Assert.Equal(3,rows.Count);
        Assert.All(rows,r=>Assert.False(r.IsComment));
        Assert.Equal("주제 안내",rows[0].KindLabel);
        Assert.Equal("001-sol.md",Property(rows[1],"SourceName"));
        Assert.Equal("답변·기록",rows[1].KindLabel);
        Assert.Equal("Sol",rows[1].Author);
        Assert.Contains("2026-09-19",rows[1].TimeDisplay);
        Assert.Contains("미확인",rows[2].Author);
        Assert.Contains("미확인",rows[2].TimeDisplay);
    }
    [Fact]
    public void OriginalAndPreambleAreNotDiscardedBeforeFirstDatedEntry()
    {
        var rows=CommunicationThreadParser.Parse(Archive+"\n## 2026-10-06 | Codex | 최신 글\n\n새 내용","topic/THREAD.md","주제");
        Assert.Equal(4,rows.Count);
        Assert.Contains(rows,r=>r.Body.Contains("첫 기록"));
        Assert.Contains(rows,r=>r.Body.Contains("새 내용"));
        var plain=CommunicationThreadParser.Parse("# 주제\n\n보존할 머리말\n\n## 2026-10-06 | Codex | 최신\n\n내용","topic2","주제");
        Assert.Contains(plain,r=>r.Body.Contains("보존할 머리말"));
    }
    [Fact]
    public void FencedArchiveMarkersNeverBecomeRealRecords()
    {
        var body=Archive+"\n```md\n## 원본 기록: fake.md\n### 2026-10-06 | Fake | 댓글\n```\n";
        var rows=CommunicationThreadParser.Parse(body,"topic","주제");
        Assert.Equal(3,rows.Count);
        Assert.DoesNotContain(rows,r=>r.Author=="Fake");
    }
    [Fact]
    public void SelectionIdentitySurvivesEditedOriginalWhileContentHashChanges()
    {
        var first=CommunicationThreadParser.Parse(Archive,"topic","주제");
        var next=CommunicationThreadParser.Parse(Archive.Replace("첫 기록","첫 기록 수정"),"topic","주제");
        Assert.Equal(3,first.Count);
        Assert.Equal(first[1].Identity,next[1].Identity);
        Assert.NotEqual(first[1].ContentHash,next[1].ContentHash);
    }
    [Fact]
    public async Task StandaloneRecordsAndSameNamedDifferentBodiesAreBothReadable()
    {
        using var f=new Fixture();
        f.Write("04_COMMUNICATION/threads/T-test/THREAD.md","# 주제\n## 원본 기록: 191-sol.md\n### 합본의 제목\nDate: 2026-09-19 KST\n합본 내용");
        f.Write("04_COMMUNICATION/threads/T-test/191-sol.md","# 독립 기록 제목\n\n독립 내용");
        f.Write("04_COMMUNICATION/threads/T-test/192-sol.md","# 두 번째 독립 기록\n\n추가 내용");
        var snapshot=await new CommunicationService().ReadOnlyAsync(f.Root);
        Assert.Empty(snapshot.Errors);
        Assert.Equal(3,snapshot.InboxItems.Count);
        Assert.Contains(snapshot.InboxItems,r=>r.SourceName.EndsWith("191-sol.md") && r.Body!.Contains("독립 내용"));
        Assert.Contains(snapshot.InboxItems,r=>r.SourceName.EndsWith("THREAD.md") && r.Body!.Contains("합본 내용"));
        Assert.Empty(snapshot.PublishablePaths);
        Assert.Equal(4,Directory.GetFiles(f.Root,"*",SearchOption.AllDirectories).Length);
    }
    [Fact]
    public async Task StandaloneSecretAndOversizeRecordsAreHeldWithoutBeingDisplayed()
    {
        using var f=new Fixture();
        f.Write("04_COMMUNICATION/threads/T-test/201-sol.md","# 정상\n본문");
        f.Write("04_COMMUNICATION/threads/T-test/202-sol.md","# 보류\napi_key=fixture-not-a-real-key");
        f.Write("04_COMMUNICATION/threads/T-test/203-sol.md",new string('한',CommunicationService.MaximumFileBytes));
        var snapshot=await new CommunicationService().ReadOnlyAsync(f.Root);
        Assert.Equal("04_COMMUNICATION/threads/T-test/201-sol.md",Assert.Single(snapshot.InboxItems).SourceName);
        Assert.NotEmpty(snapshot.Errors);
        Assert.Empty(snapshot.PublishablePaths);
    }
    [Fact]
    public void CommunicationViewHasVerticalDetailSplitterAndSourceLabels()
    {
        var root=new DirectoryInfo(AppContext.BaseDirectory);
        while(root is not null && !File.Exists(Path.Combine(root.FullName,"ai-control-tower","AIControlTower.sln")))root=root.Parent;
        Assert.NotNull(root);
        var document=XDocument.Load(Path.Combine(root!.FullName,"ai-control-tower","src","AIControlTower","Views","CommunicationWorkspaceView.xaml"));
        Assert.Contains(document.Descendants(),e=>e.Name.LocalName=="GridSplitter" && e.Attribute("Grid.Row") is not null);
        Assert.Contains(document.Descendants(),e=>e.Name.LocalName=="TextBlock" &&
            (string?)e.Attribute("Text")=="{Binding SelectedEntry.SourceName}" &&
            (string?)e.Attribute("ToolTip")=="{Binding SelectedEntry.SourceLabel}");
    }
    private static string Property(object value,string name)
    {
        var p=value.GetType().GetProperty(name);Assert.NotNull(p);return (string)p!.GetValue(value)!;
    }
    private sealed class Fixture : IDisposable
    {
        private static readonly string Parent=@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks";
        public string Root { get; }=Path.Combine(Parent,"communication-archive-"+Guid.NewGuid().ToString("N"));
        public Fixture()=>Write("04_COMMUNICATION/announcements/manifest.json",JsonSerializer.Serialize(new {schemaVersion=1,projects=new[]{new {id="Test",required=true}},participants=Array.Empty<object>(),notices=Array.Empty<object>()}));
        public void Write(string relative,string body)
        {
            var path=Path.GetFullPath(Path.Combine(Root,relative));
            Assert.StartsWith(Path.GetFullPath(Parent)+Path.DirectorySeparatorChar,path,StringComparison.OrdinalIgnoreCase);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,body,new UTF8Encoding(false));
        }
        public void Dispose()
        {
            var root=Path.GetFullPath(Root);
            if(root.StartsWith(Path.GetFullPath(Parent)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase) && Directory.Exists(root))Directory.Delete(root,true);
        }
    }
}
