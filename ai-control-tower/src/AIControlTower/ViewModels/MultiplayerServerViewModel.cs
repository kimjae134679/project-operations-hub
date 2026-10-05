using System.Collections.ObjectModel;
using System.Diagnostics;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed partial class MainViewModel
{
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
    public void OpenServerFolder() => OpenExisting(ServerRootPath);
    public void OpenServerMailbox() => OpenExisting(Path.Combine(ServerRootPath,"_통합소통"));
    public void OpenServerRepository(bool release)
    {
        try { Process.Start(new ProcessStartInfo("https://github.com/kimjae134679/" + (release ? "PhoneLoL_02" : "PhoneLoL_02-Source")) { UseShellExecute=true }); }
        catch(Exception ex) { Message = "저장소 열기 실패 · " + ProcessRunner.Sanitize(ex.Message); }
    }
}
