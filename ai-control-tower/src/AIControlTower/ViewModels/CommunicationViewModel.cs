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
public sealed record InboxRow(string Project, string Title, string Preview, string Body, string Path, string Time)
{
    public string ProjectId { get; init; } = "";
    public TaskExchangeView? Exchange { get; init; }
    public string? Warning { get; init; }
    public string StateLabel => Warning ?? Exchange?.StateLabel ?? "전달 자료";
    public string RevisionLabel => Exchange is { } record ? $"기록 {record.Revision} · {record.ActorId}" : "";
    public string ReadableBody => Exchange?.Markdown ?? (Body.TrimStart().StartsWith('{') || Body.TrimStart().StartsWith('[') ? "```json\n" + Body + "\n```" : Body);
}

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
    private bool _communicationPrepared;
    private string _communicationMessage = "공지와 프로젝트 전달 기록을 연결합니다.";
    private string _centralSyncMessage = "GitHub 연결 대기";
    private Dictionary<string,string> _communicationNames = new();
    private Dictionary<string,string> _communicationActorNames = new();
    private string _communicationViewFingerprint = "";
    private readonly List<InboxRow> _allInboxRows = [];
    private string _communicationSearch = "";
    private string _communicationStateFilter = "전체 상태";
    private bool _selectedProjectOnly, _showHistory, _rebuildingCommunication;
    public string[] CommunicationStateFilters { get; } = ["전체 상태", "남은 일", "진행 중", "완료", "막힘", "대기", "변경된 요청", "전달 자료", "기록 형식 확인 필요", "상충 기록"];
    public string CommunicationSearch { get => _communicationSearch; set { if (SetProperty(ref _communicationSearch, value)) FilterInbox(); } }
    public string CommunicationStateFilter { get => _communicationStateFilter; set { if (SetProperty(ref _communicationStateFilter, value)) FilterInbox(); } }
    public bool SelectedProjectOnly { get => _selectedProjectOnly; set { if (SetProperty(ref _selectedProjectOnly, value)) FilterInbox(); } }
    public bool ShowCommunicationHistory { get => _showHistory; set { if (SetProperty(ref _showHistory, value)) FilterInbox(); } }
    public string InboxCountLabel => $"{InboxItems.Count}개 표시 · 전체 {_allInboxRows.Count}개";
    public string InboxEmptyLabel => _allInboxRows.Count == 0 ? "아직 전달된 자료가 없습니다" : "조건에 맞는 기록이 없습니다";
    private void FilterInbox()
    {
        var selected = SelectedInbox;
        IEnumerable<InboxRow> rows = _allInboxRows;
        if (!ShowCommunicationHistory)
        {
            var latest = _allInboxRows.Where(r => r.Exchange is not null).GroupBy(r => (r.ProjectId, r.Exchange!.ActorId, r.Exchange.RecordId))
                .ToDictionary(g => g.Key, g => g.Max(r => r.Exchange!.Revision));
            rows = rows.Where(r => r.Exchange is null || r.Exchange.Revision == latest[(r.ProjectId, r.Exchange.ActorId, r.Exchange.RecordId)]);
        }
        if (SelectedProjectOnly) rows = rows.Where(r => r.ProjectId == SelectedCommunicationProject?.Id);
        if (CommunicationStateFilter == "남은 일") rows = rows.Where(r => r.Exchange?.Status is "pending" or "in_progress" or "blocked" || r.Warning is not null);
        else if (CommunicationStateFilter != "전체 상태") rows = rows.Where(r => r.StateLabel == CommunicationStateFilter);
        var query = CommunicationSearch.Trim();
        if (query.Length > 0) rows = rows.Where(r => (r.Project + "\n" + r.Title + "\n" + r.ReadableBody + "\n" + r.Body).Contains(query, StringComparison.OrdinalIgnoreCase));
        var visible = rows.ToArray();
        // Avoid collection resets on unchanged background refreshes and keep selected document identity.
        for (var i = 0; i < visible.Length; i++)
        {
            if (i < InboxItems.Count && ReferenceEquals(InboxItems[i], visible[i])) continue;
            var existing = InboxItems.IndexOf(visible[i]);
            if (existing >= 0) InboxItems.Move(existing, i);
            else InboxItems.Insert(i, visible[i]);
        }
        while (InboxItems.Count > visible.Length) InboxItems.RemoveAt(InboxItems.Count - 1);
        SelectedInbox = visible.FirstOrDefault(r => r.Path == selected?.Path) ?? visible.FirstOrDefault();
        OnPropertyChanged(nameof(NoInboxItems)); OnPropertyChanged(nameof(InboxCountLabel)); OnPropertyChanged(nameof(InboxEmptyLabel));
    }
    public ObservableCollection<NoticeRow> Notices { get; } = [];
    public ObservableCollection<CommunicationProjectRow> CommunicationProjects { get; } = [];
    public ObservableCollection<InboxRow> InboxItems { get; } = [];
    public ObservableCollection<CommunicationReceiptRow> SelectedNoticeReceipts { get; } = [];
    public ObservableCollection<string> CommunicationErrors { get; } = [];
    private void InitializeCommunication() { _communication.Diagnostic += line => OnJobLog("소통 진단 · " + line); }
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
    public CommunicationProjectRow? SelectedCommunicationProject { get => _selectedCommunicationProject; set { if (SetProperty(ref _selectedCommunicationProject,value) && SelectedProjectOnly && !_rebuildingCommunication) FilterInbox(); } }
    private string NameFor(string id) => _communicationNames.GetValueOrDefault(id,id);
    public IReadOnlyList<CommunicationTarget> GetCommunicationTargets()
    {
        var targets = new Dictionary<string,CommunicationTarget>(StringComparer.Ordinal);
        var ids = new Dictionary<string,string> { ["phonelol-current"]="PhoneLOL", ["audiobook"]="Mushoku-Audiobook", ["video-downloader"]="Video-Downloader", ["project-operations-hub"]="Control-Tower" };
        foreach(var entry in _catalog.Projects)
            if(ids.TryGetValue(entry.Id,out var id) && Directory.Exists(entry.Path)) targets[id]=new(id,entry.Path,entry.Name);
        if (Directory.Exists(ServerRootPath)) targets["PhoneLOL-Server"] = new("PhoneLOL-Server",ServerRootPath,"멀티의 신 서버");
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
        if(_disposed || !force && DateTime.UtcNow-_lastCommunication<TimeSpan.FromSeconds(15)) return;
        try
        {
            if (force) await _communicationLock.WaitAsync(_lifetime.Token);
            else if (!await _communicationLock.WaitAsync(0)) return;
        }
        catch (OperationCanceledException) { return; }
        try
        {
            IsCommunicating=true; _lastCommunication=DateTime.UtcNow;
            var network=force || DateTime.UtcNow-_lastCommunicationNetwork>TimeSpan.FromMinutes(1);
            CommunicationGitResult? prepare=null;
            if(network && !_settings.IsTemporary) { _lastCommunicationNetwork=DateTime.UtcNow; prepare=await _communicationGit.PrepareAsync(CommunicationHubPath,_lifetime.Token); _communicationPrepared=prepare.Success; CentralSyncMessage=prepare.Message; }
            else if (_settings.IsTemporary) CentralSyncMessage="화면 검증용 로컬 자료 · GitHub 변경 없음";
            if (!_settings.IsTemporary && !_communicationPrepared) { CommunicationMessage="전용 소통 저장소 연결 대기 · 기존 폴더의 자료는 보존합니다."; return; }
            var manifest=Path.Combine(CommunicationHubPath,"04_COMMUNICATION","announcements","manifest.json");
            if(!File.Exists(manifest)) { CommunicationMessage="공지를 아직 내려받지 못했습니다. 연결되면 다시 시도합니다."; return; }
            var snapshot=await _communication.SyncAsync(CommunicationHubPath,GetCommunicationTargets(),_lifetime.Token);
            if(network && prepare?.Success==true)
            {
                var published=await _communicationGit.SynchronizeAsync(CommunicationHubPath,snapshot.PublishablePaths,AutoPublishCommunication,_lifetime.Token);
                CentralSyncMessage=published.Message;
                if(published.Success) snapshot=await _communication.SyncAsync(CommunicationHubPath,GetCommunicationTargets(),_lifetime.Token);
            }
            if(!snapshot.Errors.Any(e=>e.Code=="manifest_invalid")) ReadCommunicationNames(manifest);
            ApplyCommunicationSnapshot(snapshot);
        }
        catch(OperationCanceledException) { }
        catch(Exception ex) { CommunicationMessage="소통 동기화 대기 · " + ProcessRunner.Sanitize(ex.Message); }
        finally { IsCommunicating=false; _communicationLock.Release(); }
    }
    internal void ApplyCommunicationSnapshot(CommunicationSnapshot snapshot)
    {
        if(snapshot.Errors.Any(e=>e.Code=="manifest_invalid"))
        {
            _communicationViewFingerprint="";
            CommunicationErrors.Clear();
            foreach(var error in snapshot.Errors) CommunicationErrors.Add(error.Message);
            CommunicationMessage="공지 읽기 재시도 대기 · 마지막 정상 내용을 유지합니다.";
            CentralSyncMessage=CommunicationMessage;
            return;
        }
        _communicationSnapshot=snapshot;
        UpdateCommunicationView(snapshot);
    }
    private void ReadCommunicationNames(string path)
    {
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        _communicationNames=document.RootElement.GetProperty("projects").EnumerateArray().ToDictionary(p=>p.GetProperty("id").GetString()!,p=>p.GetProperty("name").GetString()!);
        _communicationActorNames=document.RootElement.TryGetProperty("participants",out var actors) ? actors.EnumerateArray().ToDictionary(p=>p.GetProperty("id").GetString()!,p=>p.GetProperty("name").GetString()!) : new();
    }
    private void UpdateCommunicationView(CommunicationSnapshot snapshot)
    {
        var fingerprint = JsonSerializer.Serialize(new
        {
            snapshot.Notices,
            Receipts = snapshot.Receipts.Where(r=>r.IsCurrent).Select(r=>r.Receipt),
            Projects = snapshot.ProjectStates.Select(p=>new {p.ProjectId,p.RootPath,p.State,p.DeliveredCount}),
            Items = snapshot.InboxItems.Select(i=>new {i.ProjectId,i.ContentSha256,i.CentralPath,i.SourceName,i.CollectedAt,i.Title,i.Preview}),
            snapshot.Errors, Names = _communicationNames, Actors = _communicationActorNames
        });
        OnPropertyChanged(nameof(CommunicationCheckedAt));
        if (fingerprint == _communicationViewFingerprint) return;
        _communicationViewFingerprint = fingerprint;
        var selectedId=SelectedNotice?.Notice.Id;
        var selectedInboxPath = SelectedInbox?.Path;
        var existingInboxRows = _allInboxRows.ToDictionary(r => r.Path);
        _rebuildingCommunication = true;
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
        _allInboxRows.Clear();
        foreach(var item in snapshot.InboxItems.OrderByDescending(i=>i.CollectedAt))
        {
            var body = item.Body ?? "";
            var exchange = TaskExchangeDocument.Parse(body, item.ProjectId, out var warning);
            var row = new InboxRow(NameFor(item.ProjectId), exchange?.Title ?? item.Title,
                exchange?.RequestSummary ?? item.Preview, body, Path.Combine(CommunicationHubPath,item.CentralPath),
                (exchange?.UpdatedAt ?? item.CollectedAt).ToOffset(TimeSpan.FromHours(9)).ToString("MM/dd HH:mm") + " KST")
                { ProjectId = item.ProjectId, Exchange = exchange, Warning = warning };
            if (existingInboxRows.TryGetValue(row.Path, out var previous) && previous.Body == row.Body && previous.Time == row.Time && previous.Project == row.Project && previous.Title == row.Title && previous.Preview == row.Preview && previous.ProjectId == row.ProjectId && previous.Warning == warning)
                row = previous;
            _allInboxRows.Add(row);
        }
        var conflicts = _allInboxRows.Where(r => r.Exchange is not null)
            .GroupBy(r => (r.ProjectId, r.Exchange!.ActorId, r.Exchange.RecordId, r.Exchange.Revision))
            .Where(g => g.Select(r => r.Body).Distinct().Count() > 1).SelectMany(g => g).Select(r => r.Path).ToHashSet();
        for (var i = 0; i < _allInboxRows.Count; i++)
            if (conflicts.Contains(_allInboxRows[i].Path)) _allInboxRows[i] = _allInboxRows[i] with { Warning = "상충 기록" };
        _rebuildingCommunication = false;
        SelectedInbox = _allInboxRows.FirstOrDefault(r => r.Path == selectedInboxPath);
        FilterInbox();
        CommunicationErrors.Clear(); foreach(var error in snapshot.Errors) CommunicationErrors.Add((error.ProjectId is null?"":NameFor(error.ProjectId)+" · ")+error.Message);
        CommunicationMessage=$"공지 {Notices.Count}개 · 폴더 연결 {snapshot.ProjectStates.Count}개 · 수집 자료 {_allInboxRows.Count}개";
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
