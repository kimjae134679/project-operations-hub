using System.Text.Json;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class CommunicationArticleTests
{
    [Fact]
    public void LegacyJsonDecodesKoreanAndKeepsNarrativeInsteadOfDumpingBraces()
    {
        var source=JsonSerializer.Serialize(new { schemaVersion=1, projectId="Example", summary="모든 음원을 여기서 듣습니다", outputs=new[]{"오늘 결과","이전 결과"}, notes="아직 실행을 확인하지 않았습니다" });
        var article=CommunicationArticle.Read(source,null);
        Assert.Contains("모든 음원을 여기서 듣습니다",article);
        Assert.Contains("오늘 결과",article);
        Assert.Contains("아직 실행을 확인하지 않았습니다",article);
        Assert.DoesNotContain("schemaVersion",article);
        Assert.DoesNotContain("\\u",article);
        Assert.Equal(source,new InboxRow("예시","제목","미리보기",source,"path","time").Body);
    }
    [Fact]
    public void HumanTaskKeepsWorkChecksAndNextStepsWhileRawEvidenceRemainsAvailable()
    {
        var record=new TaskExchangeView("id","Example","actor",2,"제목","요청","답변","in_progress",DateTimeOffset.Now,
            "## 받은 요청\n요청\n## 확인 결과\n- 실제 음원: 미실행\n  근거: /internal/technical.json\n## 남은 일\n- 음원 재생 확인\n## 기록 출처\n- 요청 출처: current_chat","");
        var article=CommunicationArticle.Read("source",record);
        Assert.Contains("실제 음원: 미실행",article);
        Assert.Contains("음원 재생 확인",article);
        Assert.DoesNotContain("/internal/technical.json",article);
        Assert.Contains("/internal/technical.json",record.Markdown);
    }
    [Fact]
    public void InvalidJsonIsExplicitlyUnparsedAndNotInventedAsCompleted()
    {
        var article=CommunicationArticle.Read("{invalid",null);
        Assert.Contains("형식을 읽지 못했습니다",article);
        Assert.Contains("{invalid",article);
        Assert.DoesNotContain("통과",article);
    }
    [Fact]
    public void LegacyArrayRetainsBothEntriesAndEmptyMetadataHasClearFallback()
    {
        Assert.Contains("둘째",CommunicationArticle.Read("[\"첫째\",\"둘째\"]",null));
        Assert.Contains("본문으로 표시할 내용이 없습니다",CommunicationArticle.Read("{\"schemaVersion\":1}",null));
    }
    [Fact]
    public void NewSettingsKeepOldDataCompatibleAndBoundInvalidRefreshValues()
    {
        var settings=JsonSerializer.Deserialize<ControlTowerSettings>("{\"RootPath\":\"not-a-project\",\"IsTemporary\":true}")!;
        settings.IsTemporary=true;
        using var vm=new MainViewModel(settings,enablePolling:false);
        Assert.Equal(3,vm.RosterRefreshSeconds);
        Assert.Equal(1400,vm.RefreshTurnMilliseconds);
        vm.RosterRefreshSeconds=1;
        vm.RefreshTurnMilliseconds=2000;
        Assert.Equal(1,settings.RosterRefreshSeconds);
        Assert.Equal(2000,settings.RefreshTurnMilliseconds);
        vm.RosterRefreshSeconds=0;
        vm.RefreshTurnMilliseconds=-1;
        Assert.Equal(1,vm.RosterRefreshSeconds);
        Assert.Equal(2000,vm.RefreshTurnMilliseconds);
    }
}
