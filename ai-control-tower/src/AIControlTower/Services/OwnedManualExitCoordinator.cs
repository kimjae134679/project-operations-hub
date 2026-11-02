namespace AIControlTower.Services;

/// <summary>Freeze admission, observe actual ownership gates, and never cancel/kill a job to obtain idle.</summary>
public sealed class OwnedManualExitCoordinator(
    Func<(bool JobsBusy, bool WritesInFlight, bool ActivitiesIdle, bool UnknownActivity)> read,
    Action<bool> freeze)
{
    private readonly SemaphoreSlim _request = new(1, 1);
    public async Task<string> RequestAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromSeconds(10)) throw new ArgumentOutOfRangeException(nameof(timeout));
        if (!await _request.WaitAsync(0, ct)) return "held_request_pending";
        var lastWritesInFlight = false;
        try
        {
            freeze(true);
            var elapsed = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                var state = read();
                lastWritesInFlight = state.WritesInFlight;
                if (state.JobsBusy) { freeze(false); return "held_jobs"; }
                if (state.UnknownActivity)
                { if (state.WritesInFlight) return "pending_writes"; freeze(false); return "held_unknown"; }
                if (!state.WritesInFlight && state.ActivitiesIdle) return "accepted";
                if (elapsed.Elapsed >= timeout || ct.IsCancellationRequested)
                { if (state.WritesInFlight) return "pending_writes"; freeze(false); return "held_timeout"; }
                await Task.Delay(25); // Do not cancel the actual operation or release its write gate.
            }
        }
        // A failed readiness read cannot prove that the write gate is clear. Keep admission frozen.
        catch { return lastWritesInFlight ? "pending_writes" : "held_unknown"; }
        finally { _request.Release(); }
    }
}
