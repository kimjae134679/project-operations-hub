using System.Diagnostics;
using AIControlTower.Models;

namespace AIControlTower.Services;

public sealed class ActivityStore
{
    private Process? _process;
    private DateTimeOffset? _startedAt;
    private string _projectPath = string.Empty;
    private string _task = string.Empty;
    public ActivitySnapshot Current => _startedAt is null ? ActivitySnapshot.Empty : new(_projectPath, _task, string.Empty, _process is { HasExited: false } ? "실행 중" : "완료", _startedAt, DateTimeOffset.Now - _startedAt.Value, _process is { HasExited: false });
    public void Start(string projectPath, string task, Process? process) { _projectPath = projectPath; _task = task; _process = process; _startedAt = DateTimeOffset.Now; }
    public Task CancelAsync()
    {
        if (_process is { HasExited: false }) _process.Kill(true);
        return Task.CompletedTask;
    }
}
