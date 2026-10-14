using AIControlTower.Services;
using AIControlTower.Models;

namespace AIControlTower.ViewModels;

public sealed partial class MainViewModel
{
    private bool _manualExitFrozen;
    private int _manualTimerCycles;
    private int _manualDashboardReads;
    private bool _resumePcTimer, _resumeServerTimer, _resumeWatcher;
    private OwnedManualExitCoordinator? _manualExitCoordinator;
    public bool ManualExitFrozen => _manualExitFrozen;
    private bool ManualAdmissionClosed => IsManualControl && _manualExitFrozen;

    // Called only on the owner dispatcher. No task cancellation, remote stop or forced process exit.
    public Task<string> RequestManualExitAsync(Task initializeTask)
    {
        if (!IsManualControl || _disposed) return Task.FromResult("held_unknown");
        _manualExitCoordinator ??= new(() =>
        {
            var writes = _manualCollection.IsRunning || _registeredRecordPublishing.IsRunning || PcConnection.OwnedMutationInFlight;
            var idle = initializeTask.IsCompleted && _manualTimerCycles == 0 && _manualDashboardReads == 0 && _manualCollectionTask.IsCompleted && _recordPublicationTask.IsCompleted
                && PcConnection.OwnedActivitiesIdle && _communicationLock.CurrentCount == 1 && _discoveryLock.CurrentCount == 1
                && _refreshLock.CurrentCount == 1 && _serverLock.CurrentCount == 1 && _rosterLock.CurrentCount == 1
                && !WorkDashboard.IsRefreshing && !ManagementDashboard.IsRefreshing && Documents.OwnedReadsIdle;
            var unknown = initializeTask.IsFaulted || _manualCollectionTask.IsFaulted || _recordPublicationTask.IsFaulted;
            return (IsBusy, writes, idle, unknown);
        }, SetManualExitFreeze);
        return _manualExitCoordinator.RequestAsync(TimeSpan.FromSeconds(5));
    }
    private async Task<IReadOnlyList<WorkActivity>> ReadOwnedDashboardAsync(CancellationToken ct)
    {
        if(ManualAdmissionClosed)throw new InvalidOperationException("안전 종료 확인 중 · 새 작업 조회 보류");
        _manualDashboardReads++;
        try{return await _workDashboardService.ReadAsync(ct);}
        finally{_manualDashboardReads--;}
    }
    private async Task<string> ReadOwnedDetailsAsync(WorkActivity activity)
    {
        if(ManualAdmissionClosed)throw new InvalidOperationException("안전 종료 확인 중 · 새 상세 조회 보류");
        _manualDashboardReads++;
        try{return await _workDashboardService.DetailsAsync(activity);}
        finally{_manualDashboardReads--;}
    }
    private void SetManualExitFreeze(bool frozen)
    {
        if (_manualExitFrozen == frozen) return;
        _manualExitFrozen = frozen;
        PcConnection.FreezeOwnedAdmission(frozen);
        Documents.FreezeOwnedAdmission(frozen);
        if (frozen)
        {
            _resumePcTimer = _pcLiveTimer.IsEnabled; _resumeServerTimer = _serverLiveTimer.IsEnabled;
            _resumeWatcher = _watcher?.EnableRaisingEvents == true;
            _pcLiveTimer.Stop(); _serverLiveTimer.Stop();
            if (_watcher is not null) _watcher.EnableRaisingEvents = false;
        }
        else if (!_disposed)
        {
            if (_resumePcTimer) _pcLiveTimer.Start(); if (_resumeServerTimer) _serverLiveTimer.Start();
            if (_watcher is not null && _resumeWatcher) _watcher.EnableRaisingEvents = true;
        }
        Message = frozen ? "업데이트 준비 중" : "업데이트 보류";
        OnPropertyChanged(nameof(ManualExitFrozen));
    }
}
