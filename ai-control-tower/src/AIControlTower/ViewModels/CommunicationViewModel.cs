using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Input;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed record CommunicationProjectRow(string Id, string Name, string Root, string Delivery, string Read, string Applied);
public sealed record CommunicationReceiptRow(string Project, string Actor, string State, string CheckedAt, string Note)
{
    public string FilterKind { get; init; } = "확인";
    public string ApplicationStatus { get; init; } = "pending";
    public string ProjectId { get; init; } = "";
    public string ActorId { get; init; } = "";
    public string SessionId { get; init; } = "";
    public string AutomationLabel => $"{Project} · {Actor} · {State} · {CheckedAt}";
    public string IdentityLabel => SessionId.Length == 0 ? ActorId : $"{ActorId} · 세션 {SessionId}";
    public string StatusColorKey => FilterKind == "막힘" ? "Blocked" : ApplicationStatus is "applied" or "not_applicable" ? "Complete" : "Waiting";
}
public sealed record NoticeRow(CommunicationNotice Notice, string ReadSummary)
{
    public int CheckedCount { get; init; }
    public int WaitingCount { get; init; }
    public int BlockedCount { get; init; }
    public int UniqueActorCount { get; init; }
    public int PendingApplicationCount { get; init; }
    public string ReceiptSummary => $"읽음 기록 {CheckedCount}건 · 고유 AI {UniqueActorCount}개 · 미확인 프로젝트 {WaitingCount}개 · 적용 대기 {PendingApplicationCount}개 · 막힘 {BlockedCount}개";
    public string AutomationLabel => $"{Notice.Id} · {Title} · {VersionLabel}";
    public string HashLabel => $"{Notice.Id} · 버전 {Notice.Revision} · SHA-256 {Notice.ContentSha256}";
    public string Title => Notice.Title;
    public string VersionLabel => $"버전 {Notice.Revision}";
    public string Body => Regex.Replace(Regex.Replace(Notice.Body, @"(?m)^#{1,6}\s*", ""), @"\[([^\]]+)\]\([^)]+\)", "$1").Replace("**", "").Replace("`", "");
}
public sealed record InboxRow(string Project, string Title, string Preview, string Body, string Path, string Time) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool _isUnread;
    public bool IsUnread { get => _isUnread; set { if (_isUnread == value) return; _isUnread = value; PropertyChanged?.Invoke(this,new(nameof(IsUnread))); PropertyChanged?.Invoke(this,new(nameof(Unread))); PropertyChanged?.Invoke(this,new(nameof(UnreadLabel))); } }
    public bool Unread => IsUnread;
    public string UnreadLabel => IsUnread ? "안 읽음" : "읽음";
    public string Identity { get; init; } = "";
    public IReadOnlyList<CommunicationEntry> ThreadEntries { get; init; } = [];
    public int EntryCount => ThreadEntries.Count;
    public int CommentCount => ThreadEntries.Count(e=>e.IsComment);
    public int RevisionCount { get; init; } = 1;
    public string TimeDisplay => Time;
    public string ConversationSummary => Exchange is not null ? $"게시글 1 · 수정 이력 {RevisionCount}" : $"게시글 {EntryCount - CommentCount} · 댓글 {CommentCount}";
    public string StatusColorKey => Warning is not null || Exchange?.Status == "blocked" ? "Blocked" : Exchange?.Status == "completed" ? "Complete" : Exchange?.Status == "in_progress" ? "Running" : "Waiting";
    public string ProjectId { get; init; } = "";
    public TaskExchangeView? Exchange { get; init; }
    public string? Warning { get; init; }
    public string StateLabel => Warning ?? Exchange?.StateLabel ?? "전달 자료";
    public string RevisionLabel => Exchange is { } record ? $"기록 {record.Revision} · {record.ActorId}" : "";
    public string Author { get; init; } = "";
    public string ReadableBody => CommunicationArticle.Read(Body,Exchange);
    public string AutomationLabel => $"{Project} · {Title} · {Author} · {ConversationSummary}";
}

public sealed class CommunicationEntryRow(CommunicationEntry entry, bool unread) : ObservableObject
{
    public CommunicationEntry Entry { get; } = entry;
    public string Title => Entry.Title;
    public string Author => Entry.Author;
    public string TimeDisplay => Entry.TimeDisplay;
    public string Body => Entry.Body;
    public string KindLabel => Entry.KindLabel;
    public string AutomationLabel => $"{KindLabel} · {Title} · {Author} · {TimeDisplay}";
    private bool _isUnread = unread;
    public bool IsUnread { get => _isUnread; set { if (SetProperty(ref _isUnread,value)) { OnPropertyChanged(nameof(Unread)); OnPropertyChanged(nameof(UnreadLabel)); } } }
    public bool Unread => IsUnread;
    public string UnreadLabel => IsUnread ? "안 읽음" : "읽음";
}

internal sealed class CommunicationNavigationCommand(Action execute, Func<bool> canExecute) : ICommand
{
    public bool CanExecute(object? parameter) => canExecute();
    public void Execute(object? parameter) { if (canExecute()) execute(); }
    public event EventHandler? CanExecuteChanged;
    public void Changed() => CanExecuteChanged?.Invoke(this,EventArgs.Empty);
}

public sealed partial class MainViewModel
{
    // Binding seam for the communication workspace; all existing commands remain on this VM.
    public MainViewModel Communication => this;
    private string _inboxProjectFilterId = "";
    public ObservableCollection<CommunicationProjectRow> InboxProjectFilters { get; } = [];
    public string InboxProjectFilterId
    {
        get => _inboxProjectFilterId;
        set
        {
            if (!SetProperty(ref _inboxProjectFilterId,value ?? "")) return;
            if (_rebuildingCommunication) return;
            SelectedProjectOnly = false;
            if (_inboxProjectFilterId.Length > 0) SelectedCommunicationProject = CommunicationProjects.FirstOrDefault(p=>p.Id==_inboxProjectFilterId);
            FilterInbox();
        }
    }
    public string NoticeAudienceLabel => "미확인은 등록된 대상 프로젝트 기준입니다. 담당 AI 전체 명단은 없어 누락 AI 수를 추측하지 않습니다. 상태 요약은 AI·프로젝트별 최신 기록, 목록은 현재 버전의 세션별 기록입니다. 사용자 화면 읽음은 AI의 본인 확인 기록과 별개입니다.";
    public bool NoReceiptRows => SelectedNoticeReceipts.Count == 0;
    public string ReceiptEmptyLabel => SelectedNotice is null ? "공지를 선택하세요" : _allNoticeReceipts.Count == 0 ? "현재 버전의 실제 확인 기록과 등록 대상이 없습니다" : "검색·상태 조건에 맞는 기록이 없습니다";
    public string SelectedThreadHeader => SelectedInbox is null ? "주제 / 전달 자료를 선택하세요" : $"{SelectedInbox.Project} · {SelectedInbox.Title}";
    public string UserReadExplanation => "이 화면의 읽음 표시는 사용자 읽기 위치입니다. AI 공지 확인을 대신 기록하지 않습니다.";
    public string EntryReadPositionKey => SelectedEntry is null ? "" : SelectedEntry.Entry.Identity + "/" + SelectedEntry.Entry.ContentHash;
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
    private bool _onlyUnread, _selectingInternally;
    private CommunicationReadStore _communicationReadStore = new();
    private CommunicationEntryRow? _selectedEntry;
    private string _receiptFilter = "전체", _receiptSearch = "";
    private readonly List<CommunicationReceiptRow> _allNoticeReceipts = [];
    public bool OnlyUnread { get => _onlyUnread; set { if(SetProperty(ref _onlyUnread,value)) FilterInbox(); } }
    public ObservableCollection<CommunicationEntryRow> Entries { get; } = [];
    public CommunicationEntryRow? SelectedEntry { get => _selectedEntry; set { if(SetProperty(ref _selectedEntry,value)) { if(!_selectingInternally) MarkCommunicationViewed(); NotifyEntrySelection(); } } }
    public string SelectedBody => SelectedEntry?.Body ?? SelectedInbox?.ReadableBody ?? "";
    public string SelectedArticleBody => SelectedBody;
    public string EntryPositionLabel => SelectedEntry is null ? "" : $"{Entries.IndexOf(SelectedEntry)+1} / {Entries.Count}";
    public bool HasEntries => Entries.Count > 1;
    public ICommand PrevEntryCommand { get; private set; } = null!;
    public ICommand NextEntryCommand { get; private set; } = null!;
    public ICommand PreviousEntryCommand => PrevEntryCommand;
    public ICommand PreviousPostCommand { get; private set; } = null!;
    public ICommand NextPostCommand { get; private set; } = null!;
    public string[] ReceiptFilters { get; } = ["전체","확인","대기","적용 대기","적용 완료","막힘"];
    public string ReceiptFilter { get => _receiptFilter; set { if(SetProperty(ref _receiptFilter,value)) FilterNoticeReceipts(); } }
    public string ReceiptSearch { get => _receiptSearch; set { if(SetProperty(ref _receiptSearch,value)) FilterNoticeReceipts(); } }
    public string[] NoticeReceiptFilters => ReceiptFilters;
    public string NoticeReceiptFilter { get => ReceiptFilter; set => ReceiptFilter = value; }
    public string NoticeReceiptSearch { get => ReceiptSearch; set => ReceiptSearch = value; }
    public ObservableCollection<CommunicationReceiptRow> ReceiptRows => SelectedNoticeReceipts;
    public string SelectedNoticeSummary => SelectedNotice?.ReceiptSummary ?? "";
    public string ReceiptCountLabel => $"{SelectedNoticeReceipts.Count}개 항목 표시";
    public string[] CommunicationStateFilters { get; } = ["전체 상태", "남은 일", "진행 중", "완료", "막힘", "대기", "변경된 요청", "전달 자료", "기록 형식 확인 필요", "상충 기록"];
    public string CommunicationSearch { get => _communicationSearch; set { if (SetProperty(ref _communicationSearch, value)) FilterInbox(); } }
    public string CommunicationStateFilter { get => _communicationStateFilter; set { if (SetProperty(ref _communicationStateFilter, value)) FilterInbox(); } }
    public bool SelectedProjectOnly { get => _selectedProjectOnly; set { if (SetProperty(ref _selectedProjectOnly, value)) FilterInbox(); } }
    public bool ShowCommunicationHistory { get => _showHistory; set { if (SetProperty(ref _showHistory, value)) FilterInbox(); } }
    public int UnreadPostCount => _allInboxRows.GroupBy(r=>r.Identity).Count(g=>g.OrderByDescending(r=>r.Exchange?.Revision??0).First().IsUnread);
    public string InboxCountLabel => $"{InboxItems.Count}개 표시 · 안 읽음 {UnreadPostCount}개 · 전체 {_allInboxRows.Select(r=>r.Identity).Distinct().Count()}개";
    public string InboxEmptyLabel => _allInboxRows.Count == 0 ? "아직 전달된 자료가 없습니다" : "조건에 맞는 기록이 없습니다";
    private void FilterInbox()
    {
        var selected = SelectedInbox;
        IEnumerable<InboxRow> rows = _allInboxRows;
        if (!ShowCommunicationHistory)
        {
            var latest = _allInboxRows.GroupBy(r=>r.Identity).ToDictionary(g=>g.Key,g=>g.Max(r=>r.Exchange?.Revision ?? 0));
            rows = rows.Where(r=>r.Exchange is not null ? r.Exchange.Revision==latest[r.Identity] : ReferenceEquals(r,_allInboxRows.First(x=>x.Identity==r.Identity)));
        }
        if (InboxProjectFilterId.Length > 0) rows = rows.Where(r => r.ProjectId == InboxProjectFilterId);
        if (SelectedProjectOnly) rows = rows.Where(r => r.ProjectId == SelectedCommunicationProject?.Id);
        if (OnlyUnread) rows = rows.Where(r => r.IsUnread);
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
        _selectingInternally = true;
        SelectedInbox = visible.FirstOrDefault(r => r.Path == selected?.Path || r.Identity == selected?.Identity) ?? visible.FirstOrDefault();
        _selectingInternally = false;
        NotifyEntrySelection();
        OnPropertyChanged(nameof(NoInboxItems)); OnPropertyChanged(nameof(InboxCountLabel)); OnPropertyChanged(nameof(InboxEmptyLabel));
    }
    public ObservableCollection<NoticeRow> Notices { get; } = [];
    public ObservableCollection<CommunicationProjectRow> CommunicationProjects { get; } = [];
    public ObservableCollection<InboxRow> InboxItems { get; } = [];
    public ObservableCollection<CommunicationReceiptRow> SelectedNoticeReceipts { get; } = [];
    public ObservableCollection<string> CommunicationErrors { get; } = [];
    private void InitializeCommunication()
    {
        _communication.Diagnostic += line => OnJobLog("소통 진단 · " + line);
        _communicationReadStore = new(_settings.IsTemporary ? null : Path.Combine(ControlTowerSettings.DataDirectory,"communication-user-read.json"));
        PrevEntryCommand = new CommunicationNavigationCommand(()=>MoveEntry(-1),()=>Entries.IndexOf(SelectedEntry!)>0);
        NextEntryCommand = new CommunicationNavigationCommand(()=>MoveEntry(1),()=>SelectedEntry is not null && Entries.IndexOf(SelectedEntry)<Entries.Count-1);
        PreviousPostCommand = new CommunicationNavigationCommand(()=>MovePost(-1),()=>InboxItems.IndexOf(SelectedInbox!)>0);
        NextPostCommand = new CommunicationNavigationCommand(()=>MovePost(1),()=>SelectedInbox is not null && InboxItems.IndexOf(SelectedInbox)<InboxItems.Count-1);
    }
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
    public InboxRow? SelectedInbox { get => _selectedInbox; set { if(SetProperty(ref _selectedInbox,value)) { UpdateEntries(); if(!_selectingInternally && !_rebuildingCommunication) MarkCommunicationViewed(); } } }
    private void UpdateEntries()
    {
        var selectedId = SelectedEntry?.Entry.Identity;
        if (SelectedInbox is not null && Entries.Count == SelectedInbox.ThreadEntries.Count && Entries.Select(e=>e.Entry).SequenceEqual(SelectedInbox.ThreadEntries)) return;
        var internalSelection = _selectingInternally;
        _selectingInternally = true;
        Entries.Clear();
        if(SelectedInbox is not null) foreach(var entry in SelectedInbox.ThreadEntries) Entries.Add(new(entry,_communicationReadStore.IsUnread(entry.Identity,entry.ContentHash)));
        SelectedEntry = Entries.FirstOrDefault(e=>e.Entry.Identity==selectedId) ?? Entries.FirstOrDefault();
        _selectingInternally = internalSelection;
        NotifyEntrySelection();
    }
    public void MarkCommunicationViewed()
    {
        if (SelectedEntry is null || SelectedInbox is null) return;
        _communicationReadStore.MarkRead(SelectedEntry.Entry.Identity,SelectedEntry.Entry.ContentHash);
        SelectedEntry.IsUnread = false;
        foreach(var row in _allInboxRows.Where(r=>r.Identity==SelectedInbox.Identity)) row.IsUnread = row.ThreadEntries.Any(e=>_communicationReadStore.IsUnread(e.Identity,e.ContentHash));
        OnPropertyChanged(nameof(InboxCountLabel)); OnPropertyChanged(nameof(UnreadPostCount));
    }
    private void MoveEntry(int direction)
    {
        var index = Entries.IndexOf(SelectedEntry!) + direction;
        if(index>=0 && index<Entries.Count) SelectedEntry = Entries[index];
    }
    private void MovePost(int direction)
    {
        var index = InboxItems.IndexOf(SelectedInbox!) + direction;
        if(index>=0 && index<InboxItems.Count) SelectedInbox = InboxItems[index];
    }
    private void NotifyEntrySelection()
    {
        OnPropertyChanged(nameof(SelectedBody)); OnPropertyChanged(nameof(SelectedArticleBody));
        OnPropertyChanged(nameof(EntryPositionLabel)); OnPropertyChanged(nameof(HasEntries));
        OnPropertyChanged(nameof(SelectedThreadHeader)); OnPropertyChanged(nameof(EntryReadPositionKey));
        foreach(var command in new[]{PrevEntryCommand,NextEntryCommand,PreviousPostCommand,NextPostCommand}) (command as CommunicationNavigationCommand)?.Changed();
    }
    public CommunicationProjectRow? SelectedCommunicationProject { get => _selectedCommunicationProject; set { if (SetProperty(ref _selectedCommunicationProject,value) && SelectedProjectOnly && !_rebuildingCommunication) FilterInbox(); } }
    private string NameFor(string id) => id=="Shared-Communication" ? "공용 소통방" : _communicationNames.GetValueOrDefault(id,id);
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
        var inboxProjectFilterId=InboxProjectFilterId;
        Notices.Clear();
        foreach(var notice in snapshot.Notices)
        {
            var current=CurrentNoticeReceipts(notice.Id).ToArray();
            var read=current.Length;
            var latest = current.GroupBy(r=>(r.ProjectId,r.ActorId)).Select(g=>g.OrderByDescending(r=>DateTimeOffset.TryParse(r.CheckedAt,out var at)?at:DateTimeOffset.MinValue).First()).ToArray();
            Notices.Add(new(notice,read>0?$"확인 기록 {read}건 · 고유 AI {current.Select(r=>r.ActorId).Distinct().Count()}개":"확인 기록 없음") {
                CheckedCount=read, UniqueActorCount=current.Select(r=>r.ActorId).Distinct().Count(),
                WaitingCount=NoticeTargetProjects(notice).Distinct().Count(p=>!current.Any(r=>r.ProjectId==p)),
                PendingApplicationCount=latest.Count(r=>r.ApplicationStatus is not ("applied" or "not_applicable" or "blocked")),
                BlockedCount=latest.Count(r=>r.ApplicationStatus=="blocked") });
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
        InboxProjectFilters.Clear();
        InboxProjectFilters.Add(new("","전체 프로젝트","","","",""));
        foreach(var id in _communicationNames.Keys.Concat(snapshot.InboxItems.Select(i=>i.ProjectId)).Distinct())
            InboxProjectFilters.Add(CommunicationProjects.FirstOrDefault(p=>p.Id==id) ?? new(id,NameFor(id),"","","",""));
        InboxProjectFilterId=inboxProjectFilterId;
        OnPropertyChanged(nameof(InboxProjectFilterId));
        SelectedCommunicationProject=InboxProjectFilterId.Length > 0 ? CommunicationProjects.FirstOrDefault(p=>p.Id==InboxProjectFilterId) : CommunicationProjects.FirstOrDefault(p=>p.Id==projectId)??CommunicationProjects.FirstOrDefault();
        _allInboxRows.Clear();
        var parsed = snapshot.InboxItems.ToDictionary(i=>i.CentralPath,i=>
        {
            var exchange=TaskExchangeDocument.Parse(i.Body??"",i.ProjectId,out var warning);
            return (Exchange:exchange,Warning:warning);
        });
        var revisionCounts=parsed.Values.Where(v=>v.Exchange is not null).Select(v=>v.Exchange!)
            .GroupBy(e=>(e.ProjectId,e.ActorId,e.RecordId)).ToDictionary(g=>g.Key,g=>g.Select(e=>e.Revision).Distinct().Count());
        foreach(var item in snapshot.InboxItems.OrderByDescending(i=>i.CollectedAt))
        {
            var body = item.Body ?? "";
            var (exchange,warning) = parsed[item.CentralPath];
            var identity = exchange is not null ? item.ProjectId+"/task/"+exchange.ActorId+"/"+exchange.RecordId : item.ProjectId+"/file/"+item.SourceName;
            var author = exchange is null ? "" : _communicationActorNames.GetValueOrDefault(exchange.ActorId,exchange.ActorId);
            var time = (exchange?.UpdatedAt ?? item.CollectedAt).ToOffset(TimeSpan.FromHours(9)).ToString("MM/dd HH:mm") + " KST";
            var entries = exchange is null ? CommunicationThreadParser.Parse(body,identity,item.Title,author,time) : new[]{ new CommunicationEntry(identity+"/post",exchange.Title,author,time,CommunicationArticle.Read(body,exchange),item.ContentSha256) };
            if(exchange is null) author = string.Join(", ",entries.Select(e=>e.Author).Distinct().Take(3));
            if(item.IsThread && entries.Count>0) time=entries.Last().TimeDisplay;
            var row = new InboxRow(NameFor(item.ProjectId), exchange?.Title ?? item.Title,
                exchange?.RequestSummary ?? item.Preview, body, Path.Combine(CommunicationHubPath,item.CentralPath),
                time)
                { ProjectId = item.ProjectId, Exchange = exchange, Warning = warning, Author = author, Identity = identity, ThreadEntries = entries, IsUnread = entries.Any(e=>_communicationReadStore.IsUnread(e.Identity,e.ContentHash)), RevisionCount = exchange is null ? 1 : revisionCounts[(exchange.ProjectId,exchange.ActorId,exchange.RecordId)] };
            if (existingInboxRows.TryGetValue(row.Path, out var previous) && previous.Body == row.Body && previous.Time == row.Time && previous.Project == row.Project && previous.Title == row.Title && previous.Preview == row.Preview && previous.ProjectId == row.ProjectId && previous.Warning == warning && previous.Author == row.Author && previous.RevisionCount == row.RevisionCount)
                row = previous;
            _allInboxRows.Add(row);
        }
        var conflicts = _allInboxRows.Where(r => r.Exchange is not null)
            .GroupBy(r => (r.ProjectId, r.Exchange!.ActorId, r.Exchange.RecordId, r.Exchange.Revision))
            .Where(g => g.Select(r => r.Body).Distinct().Count() > 1).SelectMany(g => g).Select(r => r.Path).ToHashSet();
        for (var i = 0; i < _allInboxRows.Count; i++)
            if (conflicts.Contains(_allInboxRows[i].Path)) _allInboxRows[i] = _allInboxRows[i] with { Warning = "상충 기록" };
        _rebuildingCommunication = false;
        _selectingInternally = true;
        SelectedInbox = _allInboxRows.FirstOrDefault(r => r.Path == selectedInboxPath) ?? _allInboxRows.FirstOrDefault(r=>r.Identity==SelectedInbox?.Identity);
        _selectingInternally = false;
        FilterInbox();
        CommunicationErrors.Clear(); foreach(var error in snapshot.Errors) CommunicationErrors.Add((error.ProjectId is null?"":NameFor(error.ProjectId)+" · ")+error.Message);
        CommunicationMessage=$"공지 {Notices.Count}개 · 폴더 연결 {snapshot.ProjectStates.Count}개 · 수집 자료 {_allInboxRows.Count}개";
        OnPropertyChanged(nameof(CommunicationCheckedAt)); OnPropertyChanged(nameof(NoNotices)); OnPropertyChanged(nameof(NoInboxItems));
        UpdateNoticeReceipts();
    }
    private void UpdateNoticeReceipts()
    {
        _allNoticeReceipts.Clear();
        if(_communicationSnapshot is null || SelectedNotice is null) { FilterNoticeReceipts(); return; }
        foreach(var r in CurrentNoticeReceipts(SelectedNotice.Notice.Id).OrderBy(r=>r.ProjectId).ThenBy(r=>r.ActorId).ThenByDescending(r=>r.CheckedAt))
        {
            var state=r.ApplicationStatus switch { "applied"=>"적용 완료","not_applicable"=>"해당 없음","blocked"=>"막힘",_=>"읽음 · 적용 대기" };
            var time=DateTimeOffset.TryParse(r.CheckedAt,out var parsed)?parsed.ToOffset(TimeSpan.FromHours(9)).ToString("MM/dd HH:mm")+" KST":r.CheckedAt;
            _allNoticeReceipts.Add(new(NameFor(r.ProjectId),_communicationActorNames.GetValueOrDefault(r.ActorId,r.ActorId),state,time,r.Note) { FilterKind = r.ApplicationStatus=="blocked" ? "막힘" : "확인",ApplicationStatus=r.ApplicationStatus,ProjectId=r.ProjectId,ActorId=r.ActorId,SessionId=r.SessionId });
        }
        foreach(var id in NoticeTargetProjects(SelectedNotice.Notice).Where(id=>!_communicationSnapshot.Receipts.Any(r=>r.IsCurrent && r.Receipt.NoticeId==SelectedNotice.Notice.Id && r.Receipt.ProjectId==id)))
            _allNoticeReceipts.Add(new(NameFor(id),"담당자 확인 대기","미확인","—","현재 공지 버전의 확인 기록이 없습니다.") { FilterKind = "대기" });
        FilterNoticeReceipts();
        OnPropertyChanged(nameof(SelectedNoticeSummary));
    }
    private IEnumerable<string> NoticeTargetProjects(CommunicationNotice notice) => notice.Targets.Contains("*")
        ? _communicationNames.Count>0 ? _communicationNames.Keys : (_communicationSnapshot?.ProjectStates.Select(p=>p.ProjectId)??[]).Distinct()
        : notice.Targets;
    private IEnumerable<CommunicationReceipt> CurrentNoticeReceipts(string noticeId) => (_communicationSnapshot?.Receipts??[])
        // IsCurrent is computed by the existing strict revision/hash/identity receipt validator.
        .Where(r=>r.IsCurrent && r.Receipt.NoticeId==noticeId).Select(r=>r.Receipt);
    private void FilterNoticeReceipts()
    {
        var query=ReceiptSearch.Trim();
        var visible=_allNoticeReceipts.Where(r=>(ReceiptFilter=="전체" || ReceiptFilter=="확인" && r.FilterKind!="대기" || r.FilterKind==ReceiptFilter
            || ReceiptFilter=="적용 대기" && r.FilterKind!="대기" && r.ApplicationStatus is not ("applied" or "not_applicable" or "blocked")
            || ReceiptFilter=="적용 완료" && r.ApplicationStatus is "applied" or "not_applicable")
            && (query.Length==0 || (r.Project+" "+r.ProjectId+" "+r.Actor+" "+r.ActorId+" "+r.State+" "+r.CheckedAt+" "+r.Note+" "+r.SessionId).Contains(query,StringComparison.OrdinalIgnoreCase))).ToArray();
        for(var i=0;i<visible.Length;i++)
        {
            if(i<SelectedNoticeReceipts.Count && SelectedNoticeReceipts[i]==visible[i]) continue;
            var old=SelectedNoticeReceipts.IndexOf(visible[i]);
            if(old>=0) SelectedNoticeReceipts.Move(old,i);
            else SelectedNoticeReceipts.Insert(i,visible[i]);
        }
        while(SelectedNoticeReceipts.Count>visible.Length) SelectedNoticeReceipts.RemoveAt(SelectedNoticeReceipts.Count-1);
        OnPropertyChanged(nameof(ReceiptCountLabel)); OnPropertyChanged(nameof(NoReceiptRows)); OnPropertyChanged(nameof(ReceiptEmptyLabel));
    }
}
