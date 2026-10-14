using System.Reflection;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;
using Xunit;

namespace AIControlTower.Tests;

public sealed class CommunicationWorkspaceTests
{
    [Fact]
    public void NoticeCountsRecordsAndUniqueAiSeparatelyAndExcludesStaleReceipts()
    {
        using var vm = NewVm();
        var notice = new CommunicationNotice("N",2,"공지","본문","current",["A","B","C"],true);
        var receipts = new[] { Receipt("A","AI","s1","read",1), Receipt("A","AI","s2","applied",2), Receipt("B","AI","s3","blocked",3), new CommunicationReceiptItem(new() { NoticeId="N", ProjectId="C", ActorId="Old", Note="OLD" },false,"old") };
        Apply(vm,new([notice],receipts,[],[],[],DateTimeOffset.UtcNow,[]));
        Assert.Equal(3,vm.SelectedNotice!.CheckedCount);
        Assert.Equal(1,Value<int>(vm.SelectedNotice,"UniqueActorCount"));
        Assert.Equal(1,vm.SelectedNotice.WaitingCount);
        Assert.Equal(1,vm.SelectedNotice.BlockedCount);
        Assert.Equal(0,Value<int>(vm.SelectedNotice,"PendingApplicationCount"));
        Assert.Equal(4,vm.SelectedNoticeReceipts.Count);
        Assert.DoesNotContain("명",vm.SelectedNotice.ReceiptSummary);
        Assert.DoesNotContain(vm.SelectedNoticeReceipts,r=>r.Note=="OLD");
        vm.ReceiptSearch="s2";
        Assert.Single(vm.SelectedNoticeReceipts);
        vm.ReceiptSearch="적용 완료";
        Assert.Single(vm.SelectedNoticeReceipts);
        vm.ReceiptSearch=""; vm.ReceiptFilter="적용 대기";
        Assert.Single(vm.SelectedNoticeReceipts);
        Assert.Contains("추측하지 않습니다",Value<string>(vm,"NoticeAudienceLabel"));
    }
    [Fact]
    public void ExplicitProjectFilterAppliesImmediatelyAndAllProjectsRestoresList()
    {
        using var vm=NewVm();
        var items=new[]{ Item("A","one"),Item("B","two") };
        Apply(vm,new([],[],[],items,[],DateTimeOffset.UtcNow,[]));
        Assert.Equal(2,vm.InboxItems.Count);
        Set(vm,"InboxProjectFilterId","B");
        Assert.Equal("B",Assert.Single(vm.InboxItems).ProjectId);
        var selected=vm.SelectedInbox;
        Apply(vm,new([],[],[],items,[],DateTimeOffset.UtcNow.AddSeconds(5),[]));
        Assert.Equal("B",Value<string>(vm,"InboxProjectFilterId"));
        Assert.Same(selected,vm.SelectedInbox);
        Set(vm,"InboxProjectFilterId","");
        Assert.Equal(2,vm.InboxItems.Count);
        Assert.Same(vm,Value<MainViewModel>(vm,"Communication"));
    }
    [Fact]
    public void AccessibleLabelsNeverExposeArticleOrReceiptBody()
    {
        var row=new InboxRow("프로젝트","제목","preview","SECRET BODY","path","time") { Author="AI" };
        Assert.DoesNotContain("SECRET",Value<string>(row,"AutomationLabel"));
        var entry=new CommunicationEntryRow(new("id","댓글 제목","AI","time","SECRET BODY","hash",true),true);
        Assert.DoesNotContain("SECRET",Value<string>(entry,"AutomationLabel"));
        var receipt=new CommunicationReceiptRow("프로젝트","AI","읽음","time","SECRET NOTE");
        Assert.DoesNotContain("SECRET",Value<string>(receipt,"AutomationLabel"));
    }
    [Fact]
    public void NewViewsKeepReceiptPaneVisibleAndUseSafeItemNames()
    {
        var root=new DirectoryInfo(AppContext.BaseDirectory);
        while(root is not null && !File.Exists(Path.Combine(root.FullName,"ai-control-tower","AIControlTower.sln"))) root=root.Parent;
        Assert.NotNull(root);
        var views=Path.Combine(root!.FullName,"ai-control-tower","src","AIControlTower","Views");
        var notice=Path.Combine(views,"NoticeOverviewView.xaml");
        var communication=Path.Combine(views,"CommunicationWorkspaceView.xaml");
        Assert.True(File.Exists(notice),"Embedded notice workspace is missing");
        Assert.True(File.Exists(communication),"Post/comment workspace is missing");
        var n=System.Xml.Linq.XDocument.Load(notice);
        var c=System.Xml.Linq.XDocument.Load(communication);
        Assert.Contains(n.Descendants(),e=>e.Name.LocalName=="GridSplitter");
        Assert.DoesNotContain(n.Descendants(),e=>e.Name.LocalName=="Expander");
        foreach(var document in new[]{n,c}) Assert.Contains(document.Descendants(),e=>e.Name.LocalName=="Setter" && (string?)e.Attribute("Property")=="AutomationProperties.Name" && (string?)e.Attribute("Value")=="{Binding AutomationLabel}");
        Assert.Contains(c.Descendants(),e=>e.Name.LocalName=="ItemsControl" && (string?)e.Attribute("ItemsSource")=="{Binding CommunityComments}");
        Assert.DoesNotContain(c.Descendants(),e=>e.Name.LocalName=="ComboBox");
        Assert.DoesNotContain(c.Descendants(),e=>e.Attributes().Any(a=>a.Name.LocalName=="AutoHeight"));
        foreach(var document in new[]{n,c}) Assert.DoesNotContain(document.Descendants(),e=>(string?)e.Attribute("Text")=="{Binding CentralSyncMessage}");
    }
    private static CommunicationReceiptItem Receipt(string project,string actor,string session,string status,int hour)=>new(new() { NoticeId="N",Revision=2,ContentSha256="current",ProjectId=project,ActorId=actor,SessionId=session,ApplicationStatus=status,CheckedAt=$"2026-10-06T0{hour}:00:00Z",Note=session },true,session);
    private static CommunicationInboxItem Item(string project,string name)=>new(project,"hash",name,name,DateTimeOffset.UtcNow) { Title=name,Body="# 제목\n\n본문" };
    private static MainViewModel NewVm()=>new(new ControlTowerSettings { IsTemporary=true,CommunicationHubPath=Path.GetTempPath() },enablePolling:false);
    private static T Value<T>(object o,string property) { var p=o.GetType().GetProperty(property); Assert.NotNull(p); return (T)p!.GetValue(o)!; }
    private static void Set(object o,string property,object value) { var p=o.GetType().GetProperty(property); Assert.NotNull(p); p!.SetValue(o,value); }
    private static void Apply(MainViewModel vm,CommunicationSnapshot snapshot)=>typeof(MainViewModel).GetMethod("ApplyCommunicationSnapshot",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(vm,[snapshot]);
}
