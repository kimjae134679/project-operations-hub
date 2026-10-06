using System.Collections.ObjectModel;
using System.Diagnostics;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed record RosterRefreshOption(int Seconds, string Label);
public sealed record RefreshSpeedOption(int Milliseconds, string Label);

public sealed partial class MainViewModel
{
    public RosterRefreshOption[] RosterRefreshOptions { get; } = [new(1,"1초"),new(3,"3초"),new(5,"5초"),new(10,"10초")];
    public RefreshSpeedOption[] RefreshSpeedOptions { get; } = [new(1000,"빠르게"),new(1400,"보통"),new(2000,"느리게")];
    public int RosterRefreshSeconds
    {
        get => new[]{1,3,5,10}.Contains(_settings.RosterRefreshSeconds) ? _settings.RosterRefreshSeconds : 3;
        set
        {
            if (!new[]{1,3,5,10}.Contains(value) || value==RosterRefreshSeconds) return;
            _settings.RosterRefreshSeconds=value; _serverLiveTimer.Interval=TimeSpan.FromSeconds(value);
            SaveSettings(); OnPropertyChanged(); OnPropertyChanged(nameof(RosterStatus));
        }
    }
    public int RefreshTurnMilliseconds
    {
        get => new[]{1000,1400,2000}.Contains(_settings.RefreshTurnMilliseconds) ? _settings.RefreshTurnMilliseconds : 1400;
        set { if (!new[]{1000,1400,2000}.Contains(value) || value==RefreshTurnMilliseconds) return; _settings.RefreshTurnMilliseconds=value; SaveSettings(); OnPropertyChanged(); }
    }
    private bool _isRefreshingRosterManually;
    public bool IsRefreshingRosterManually { get => _isRefreshingRosterManually; private set { if(SetProperty(ref _isRefreshingRosterManually,value)) OnPropertyChanged(nameof(RosterStatus)); } }
    private readonly MultiplayerServerService _serverMonitor = new();
    private readonly SemaphoreSlim _serverLock = new(1,1);
    private DateTime _lastServerCheck = DateTime.MinValue;
    private MultiplayerServerSnapshot? _serverSnapshot;
    private string _serverSummaryMessage = "연결된 서버의 상태 변화가 소통 자료로 모입니다.";
    private bool _isCheckingServer;
    public ObservableCollection<MultiplayerModeCounts> ServerModes { get; } = [];
    public bool IsCheckingServer { get => _isCheckingServer; private set => SetProperty(ref _isCheckingServer,value); }
    public string ServerRootPath
    {
        get => _settings.CommunicationFolders.GetValueOrDefault("PhoneLOL-Server",_settings.MultiplayerServerRoot);
        set
        {
            if (!Directory.Exists(value)) return;
            _settings.MultiplayerServerRoot = Path.GetFullPath(value);
            _settings.CommunicationFolders["PhoneLOL-Server"] = _settings.MultiplayerServerRoot;
            _lastServerCheck = DateTime.MinValue; SaveSettings(); OnPropertyChanged();
        }
    }
    public MultiplayerServerSnapshot? ServerSnapshot => _serverSnapshot;
    public string ServerHealth => _serverSnapshot?.Health ?? "서버 확인 전";
    public string ServerOnline => _serverSnapshot?.Total?.Online.ToString() ?? "—";
    public string ServerPreparing => _serverSnapshot?.Total?.Preparing.ToString() ?? "—";
    public string ServerPlaying => _serverSnapshot?.Total?.Playing.ToString() ?? "—";
    public string ServerCheckedAt => _serverSnapshot is null ? "" : "마지막 확인 " + _serverSnapshot.FetchedAt.LocalDateTime.ToString("HH:mm:ss");
    public string ServerObservedAt => _serverSnapshot?.ObservedAt is { } time ? "서버 집계 " + time.LocalDateTime.ToString("HH:mm:ss") : "집계 미확인";
    public string ServerUpdatePolicy => _serverSnapshot?.Policy is { } p ? $"{p.LatestVersion} · 빌드 {p.LatestBuild}" : "미확인";
    public string ServerMinimumBuild => _serverSnapshot?.Policy is { } p ? p.MinimumBuild == 0 ? "최소 빌드 제한 없음" : $"빌드 {p.MinimumBuild} 이상" : "미확인";
    public string ServerBlockedBuilds => _serverSnapshot?.Policy is { } p ? p.BlockedBuilds.Count == 0 ? "차단 빌드 없음" : "차단 빌드 " + string.Join(", ",p.BlockedBuilds) : "미확인";
    public string ServerPolicyHealth => _serverSnapshot?.PolicyHealth ?? "정책 확인 전";
    public string ServerSummaryMessage { get => _serverSummaryMessage; private set => SetProperty(ref _serverSummaryMessage,value); }
    public async Task RefreshServerAsync(bool force = false)
    {
        if(IsReadOnlyView){ServerSummaryMessage="로컬 조회 · 서버/API 확인 없음 · 현재 생존 미확인";return;}
        if (_disposed || !force && DateTime.UtcNow-_lastServerCheck<TimeSpan.FromSeconds(15) || !await _serverLock.WaitAsync(0)) return;
        try
        {
            IsCheckingServer = true; _lastServerCheck=DateTime.UtcNow;
            _serverSnapshot = await _serverMonitor.FetchAsync(_lifetime.Token);
            ServerModes.Clear(); foreach(var mode in _serverSnapshot.Modes) ServerModes.Add(mode);
            if (!_settings.IsTemporary && AutoCommunication && Directory.Exists(ServerRootPath))
            {
                var summary = await _serverMonitor.WriteSummary(_serverSnapshot,ServerRootPath,_lifetime.Token);
                ServerSummaryMessage = summary.Warning ?? "상태가 바뀌면 소통 자료로 저장됩니다.";
            }
            else ServerSummaryMessage = Directory.Exists(ServerRootPath) ? "서버 상태 확인됨 · 자동 자료 수집은 설정에 따릅니다." : "서버 작업 폴더를 연결하면 공지와 상태 기록도 동기화됩니다.";
            foreach(var property in new[]{nameof(ServerSnapshot),nameof(ServerHealth),nameof(ServerOnline),nameof(ServerPreparing),nameof(ServerPlaying),nameof(ServerCheckedAt),nameof(ServerObservedAt),nameof(ServerUpdatePolicy),nameof(ServerMinimumBuild),nameof(ServerBlockedBuilds),nameof(ServerPolicyHealth)}) OnPropertyChanged(property);
        }
        catch(OperationCanceledException) { }
        finally { IsCheckingServer=false; _serverLock.Release(); }
    }
    private readonly MultiplayerRosterService _rosterMonitor = new();
    private readonly SemaphoreSlim _rosterLock = new(1, 1);
    private bool _isUpdatingRoster;
    private bool _rosterHealthy;
    private string _rosterHealth = "접속자 목록 확인 전";
    private DateTimeOffset? _rosterCheckedAt;
    public ObservableCollection<MultiplayerPlayer> ServerPlayers { get; } = [];
    public bool IsUpdatingRoster { get => _isUpdatingRoster; private set { SetProperty(ref _isUpdatingRoster, value); OnPropertyChanged(nameof(RosterStatus)); } }
    public bool RosterHealthy => _rosterHealthy;
    public bool RosterEmpty => _rosterHealthy && ServerPlayers.Count == 0;
    public string RosterStatus => IsRefreshingRosterManually ? "접속자 확인 중…" : _rosterHealthy ? $"{RosterRefreshSeconds}초마다 자동 갱신" : _rosterHealth;
    public string RosterCheckedAt => _rosterCheckedAt is { } time ? "마지막 확인 " + time.LocalDateTime.ToString("HH:mm:ss") : "";
    public async Task RefreshRosterAsync(bool manual = false)
    {
        if(IsReadOnlyView){_rosterHealth="로컬 조회 · 접속자/API 확인 없음";OnPropertyChanged(nameof(RosterStatus));return;}
        if (_disposed) return;
        try
        {
            if(manual) await _rosterLock.WaitAsync(_lifetime.Token);
            else if(!await _rosterLock.WaitAsync(0)) return;
        }
        catch(OperationCanceledException) { return; }
        try
        {
            IsUpdatingRoster = true; IsRefreshingRosterManually = manual;
            var result = await _rosterMonitor.FetchAsync(_lifetime.Token);
            _rosterHealthy = result.IsHealthy;
            _rosterHealth = result.IsHealthy ? "1초마다 자동 갱신" : result.Health + (ServerPlayers.Count > 0 ? " · 이전 목록" : "");
            if (result.IsHealthy)
            {
                _rosterCheckedAt = result.FetchedAt;
                if (!ServerPlayers.SequenceEqual(result.Players))
                {
                    // Keep unchanged items in place: no empty-list flash on each poll.
                    for (var i = 0; i < result.Players.Count; i++)
                    {
                        var player = result.Players[i];
                        if (i < ServerPlayers.Count && ServerPlayers[i] == player) continue;
                        if (i < ServerPlayers.Count && ServerPlayers[i].Nickname == player.Nickname) ServerPlayers[i] = player;
                        else { var existing = ServerPlayers.ToList().FindIndex(i, x => x.Nickname == player.Nickname); if (existing >= 0) ServerPlayers.Move(existing, i); else ServerPlayers.Insert(i, player); ServerPlayers[i] = player; }
                    }
                    while (ServerPlayers.Count > result.Players.Count) ServerPlayers.RemoveAt(ServerPlayers.Count - 1);
                }
            }
            foreach (var property in new[] { nameof(RosterHealthy), nameof(RosterEmpty), nameof(RosterStatus), nameof(RosterCheckedAt) }) OnPropertyChanged(property);
        }
        catch (OperationCanceledException) { }
        finally { IsUpdatingRoster = false; IsRefreshingRosterManually = false; OnPropertyChanged(nameof(RosterStatus)); _rosterLock.Release(); }
    }
    public void OpenServerFolder() => OpenExisting(ServerRootPath);
    public void OpenServerMailbox() => OpenExisting(Path.Combine(ServerRootPath,"_통합소통"));
    public void OpenServerRepository(bool release)
    {
        if(BlockOperation())return;
        try { Process.Start(new ProcessStartInfo("https://github.com/kimjae134679/" + (release ? "PhoneLoL_02" : "PhoneLoL_02-Source")) { UseShellExecute=true }); }
        catch(Exception ex) { Message = "저장소 열기 실패 · " + ProcessRunner.Sanitize(ex.Message); }
    }
}
