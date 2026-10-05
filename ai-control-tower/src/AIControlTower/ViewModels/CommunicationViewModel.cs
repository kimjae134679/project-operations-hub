using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed record CommunicationProjectRow(string Id, string Name, string Root, string Delivery, string Read, string Applied);
public sealed record CommunicationReceiptRow(string Project, string Actor, string State, string CheckedAt, string Note);
public sealed record NoticeRow(CommunicationNotice Notice, string ReadSummary)
{
    public string Title => Notice.Title;
    public string VersionLabel => $"버전 {Notice.Revision}";
    public string Body => Regex.Replace(Regex.Replace(Notice.Body, @"(?m)^#{1,6}\s*", ""), @"\[([^\]]+)\]\([^)]+\)", "$1").Replace("**", "").Replace("`", "");
}
public sealed record InboxRow(string Project, string Title, string Preview, string Body, string Path, string Time);

public sealed partial class MainViewModel
{
    private readonly CommunicationService _communication = new();
    private readonly CommunicationGitService _communicationGit = new();
    private readonly SemaphoreSlim _communicationLock = new(1,1);
    private DateTime _lastCommunication = DateTime.MinValue, _lastCommunicationNetwork = DateTime.MinValue;
    private CommunicationSnapshot? _communicationSnapshot;
    private NoticeRow? _selectedNotice;
    private InboxRow? _selectedInbox;
    private CommunicationProjectRow? _selectedCommunicationProject;
    private bool _isCommunicating;
    private string _communicationMessage = "공지와 프로젝트 전달 기록을 연결합니다.";
    private string _centralSyncMessage = "GitHub 연결 대기";
    private Dictionary<string,string> _communicationNames = new();
    private Dictionary<string,string> _communicationActorNames = new();
    public ObservableCollection<NoticeRow> Notices { get; } = [];
    public ObservableCollection<CommunicationProjectRow> CommunicationProjects { get; } = [];
    public ObservableCollection<InboxRow> InboxItems { get; } = [];
    public ObservableCollection<CommunicationReceiptRow> SelectedNoticeReceipts { get; } = [];
    public ObservableCollection<string> CommunicationErrors { get; } = [];
    private void InitializeCommunication() { }
    public bool AutoCommunication { get => _settings.AutoCommunication; set { _settings.AutoCommunication=value; SaveSettings(); OnPropertyChanged(); } }
    public bool AutoPublishCommunication { get => _settings.AutoPublishCommunication; set { _settings.AutoPublishCommunication=value; SaveSettings(); OnPropertyChanged(); } }
    public string CommunicationHubPath { get => _settings.CommunicationHubPath; set { _settings.CommunicationHubPath=value; _lastCommunicationNetwork=DateTime.MinValue; SaveSettings(); OnPropertyChanged(); } }
    public bool IsCommunicating { get => _isCommunicating; private set => SetProperty(ref _isCommunicating,value); }
    public string CommunicationMessage { get => _communicationMessage; private set => SetProperty(ref _communicationMessage,value); }
    public string CentralSyncMessage { get => _centralSyncMessage; private set => SetProperty(ref _centralSyncMessage,value); }
    public string CommunicationCheckedAt => _communicationSnapshot is null ? "아직 연결 전" : "마지막 확인 " + _communicationSnapshot.LastSync.LocalDateTime.ToString("HH:mm:ss");
    public bool NoNotices => Notices.Count==0;
    public bool NoInboxItems => InboxItems.Count==0;
    public NoticeRow? SelectedNotice { get => _selectedNotice; set { if(SetProperty(ref _selectedNotice,value)) UpdateNoticeReceipts(); } }
    public InboxRow? SelectedInbox { get => _selectedInbox; set => SetProperty(ref _selectedInbox,value); }
    public CommunicationProjectRow? SelectedCommunicationProject { get => _selectedCommunicationProject; set => SetProperty(ref _selectedCommunicationProject,value); }
    private string NameFor(string id) => _communicationNames.GetValueOrDefault(id,id);
    public IReadOnlyList<CommunicationTarget> GetCommunicationTargets()
    {
        var targets = new Dictionary<string,CommunicationTarget>(StringComparer.Ordinal);
        var ids = new Dictionary<string,string> { ["phonelol-current"]="PhoneLOL", ["audiobook"]="Mushoku-Audiobook", ["video-downloader"]="Video-Downloader", ["project-operations-hub"]="Control-Tower" };
        foreach(var entry in _catalog.Projects)
            if(ids.TryGetValue(entry.Id,out var id) && Directory.Exists(entry.Path)) targets[id]=new(id,entry.Path,entry.Name);
        foreach(var pair in _settings.CommunicationFolders)
            targets[pair.Key]=new(pair.Key,pair.Value,NameFor(pair.Key));
        return targets.Values.ToArray();
    }
    public void LinkCommunicationProject(string id,string folder)
    {
        if(!_communicationNames.ContainsKey(id) || !Directory.Exists(folder)) { CommunicationMessage="연결할 프로젝트와 실제 폴더를 선택하세요."; return; }
        _settings.CommunicationFolders[id]=Path.GetFullPath(folder); SaveSettings(); _lastCommunication=DateTime.MinValue;
    }
    public void OpenCommunicationFolder() => OpenExisting(SelectedCommunicationProject?.Root is { Length:>0 } root ? Path.Combine(root,"_통합소통") : null);
    public void OpenCollectedFile() => OpenExisting(SelectedInbox?.Path);
    public async Task SyncCommunicationAsync(bool force=false)
    {
        if(_disposed || !force && DateTime.UtcNow-_lastCommunication<TimeSpan.FromSeconds(15) || !await _communicationLock.WaitAsync(0)) return;
        try
        {
            IsCommunicating=true; _lastCommunication=DateTime.UtcNow;
            var network=force || DateTime.UtcNow-_lastCommunicationNetwork>TimeSpan.FromMinutes(1);
            CommunicationGitResult? prepare=null;
            if(network) { _lastCommunicationNetwork=DateTime.UtcNow; prepare=await _communicationGit.PrepareAsync(CommunicationHubPath,_lifetime.Token); CentralSyncMessage=prepare.Message; }
            var manifest=Path.Combine(CommunicationHubPath,"04_COMMUNICATION","announcements","manifest.json");
            if(!File.Exists(manifest)) { CommunicationMessage="공지를 아직 내려받지 못했습니다. 연결되면 다시 시도합니다."; return; }
            var snapshot=await _communication.SyncAsync(CommunicationHubPath,GetCommunicationTargets(),_lifetime.Token);
            if(network && prepare?.Success==true)
            {
                var published=await _communicationGit.SynchronizeAsync(CommunicationHubPath,snapshot.PublishablePaths,AutoPublishCommunication,_lifetime.Token);
                CentralSyncMessage=published.Message;
                if(published.Success) snapshot=await _communication.SyncAsync(CommunicationHubPath,GetCommunicationTargets(),_lifetime.Token);
            }
            _communicationSnapshot=snapshot;
            ReadCommunicationNames(manifest);
            UpdateCommunicationView(snapshot);
        }
        catch(OperationCanceledException) { }
        catch(Exception ex) { CommunicationMessage="소통 동기화 대기 · " + ProcessRunner.Sanitize(ex.Message); }
        finally { IsCommunicating=false; _communicationLock.Release(); }
    }
    private void ReadCommunicationNames(string path)
    {
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        _communicationNames=document.RootElement.GetProperty("projects").EnumerateArray().ToDictionary(p=>p.GetProperty("id").GetString()!,p=>p.GetProperty("name").GetString()!);
        _communicationActorNames=document.RootElement.TryGetProperty("participants",out var actors) ? actors.EnumerateArray().ToDictionary(p=>p.GetProperty("id").GetString()!,p=>p.GetProperty("name").GetString()!) : new();
    }
    private void UpdateCommunicationView(CommunicationSnapshot snapshot)
    {
        var selectedId=SelectedNotice?.Notice.Id;
        var selectedInboxPath=SelectedInbox?.Path;
        var projectId=SelectedCommunicationProject?.Id;
        Notices.Clear();
        foreach(var notice in snapshot.Notices)
        {
            var read=snapshot.Receipts.Where(r=>r.IsCurrent && r.Receipt.NoticeId==notice.Id).Select(r=>(r.Receipt.ProjectId,r.Receipt.ActorId)).Distinct().Count();
            Notices.Add(new(notice,read>0?$"확인 기록 {read}명":"확인 기록 없음"));
        }
        SelectedNotice=Notices.FirstOrDefault(n=>n.Notice.Id==selectedId)??Notices.FirstOrDefault();
        CommunicationProjects.Clear();
        foreach(var project in _communicationNames)
        {
            var state=snapshot.ProjectStates.FirstOrDefault(p=>p.ProjectId==project.Key);
            var applicable=snapshot.Notices.Where(n=>n.Targets.Contains("*")||n.Targets.Contains(project.Key)).ToArray();
            var receipts=snapshot.Receipts.Where(r=>r.IsCurrent && r.Receipt.ProjectId==project.Key).ToArray();
            var count=receipts.Select(r=>r.Receipt.NoticeId).Distinct().Count();
            var applied=receipts.Count(r=>r.Receipt.ApplicationStatus=="applied");
            CommunicationProjects.Add(new(project.Key,project.Value,state?.RootPath??"",state is null?"폴더 연결 필요":state.State,count==0?"미확인":$"{count}/{applicable.Length}개 공지 · {receipts.Select(r=>r.Receipt.ActorId).Distinct().Count()}명",applied==0?"—":$"적용 기록 {applied}건"));
        }
        SelectedCommunicationProject=CommunicationProjects.FirstOrDefault(p=>p.Id==projectId)??CommunicationProjects.FirstOrDefault();
        InboxItems.Clear();
        foreach(var item in snapshot.InboxItems.OrderByDescending(i=>i.CollectedAt))
            InboxItems.Add(new(NameFor(item.ProjectId),item.Title,item.Preview,item.Body,item.CentralPath,item.CollectedAt.LocalDateTime.ToString("MM/dd HH:mm")));
        SelectedInbox=InboxItems.FirstOrDefault(i=>i.Path==selectedInboxPath)??InboxItems.FirstOrDefault();
        CommunicationErrors.Clear(); foreach(var error in snapshot.Errors) CommunicationErrors.Add((error.ProjectId is null?"":NameFor(error.ProjectId)+" · ")+error.Message);
        CommunicationMessage=$"공지 {Notices.Count}개 · 폴더 연결 {snapshot.ProjectStates.Count}개 · 수집 자료 {InboxItems.Count}개";
        OnPropertyChanged(nameof(CommunicationCheckedAt)); OnPropertyChanged(nameof(NoNotices)); OnPropertyChanged(nameof(NoInboxItems));
        UpdateNoticeReceipts();
    }
    private void UpdateNoticeReceipts()
    {
        SelectedNoticeReceipts.Clear();
        if(_communicationSnapshot is null || SelectedNotice is null) return;
        foreach(var item in _communicationSnapshot.Receipts.Where(r=>r.IsCurrent&&r.Receipt.NoticeId==SelectedNotice.Notice.Id).OrderBy(r=>r.Receipt.ProjectId))
        {
            var r=item.Receipt;
            var state=r.ApplicationStatus switch { "applied"=>"적용 완료","not_applicable"=>"해당 없음","blocked"=>"막힘",_=>"읽음 · 적용 대기" };
            var time=DateTimeOffset.TryParse(r.CheckedAt,out var parsed)?parsed.LocalDateTime.ToString("MM/dd HH:mm"):r.CheckedAt;
            SelectedNoticeReceipts.Add(new(NameFor(r.ProjectId),_communicationActorNames.GetValueOrDefault(r.ActorId,r.ActorId),state,time,r.Note));
        }
    }
}
