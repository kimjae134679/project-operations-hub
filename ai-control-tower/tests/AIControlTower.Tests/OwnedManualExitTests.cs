using System.Reflection;
using System.Text.Json;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class OwnedManualExitTests
{
    private static Type Service(string name)
    {
        var type = typeof(JobManager).Assembly.GetType("AIControlTower.Services." + name);
        Assert.NotNull(type); return type!;
    }
    private static async Task<string> Request(Func<(bool JobsBusy, bool WritesInFlight, bool ActivitiesIdle, bool UnknownActivity)> read,
        Action<bool> freeze, int timeoutMilliseconds = 60)
    {
        var type = Service("OwnedManualExitCoordinator");
        var coordinator = Activator.CreateInstance(type, read, freeze)!;
        return await (Task<string>)type.GetMethod("RequestAsync")!.Invoke(coordinator, [TimeSpan.FromMilliseconds(timeoutMilliseconds), CancellationToken.None])!;
    }
    [Fact]
    public async Task IdleExitFreezesAdmissionBeforeCheckingAndKeepsItFrozen()
    {
        var frozen = false;
        Assert.Equal("accepted", await Request(() => { Assert.True(frozen); return (false, false, true, false); }, value => frozen = value));
        Assert.True(frozen);
    }
    [Fact]
    public async Task OwnedJobsIncludingQueuedReservationsHoldAndResumeWithoutStopping()
    {
        var changes = new List<bool>();
        Assert.Equal("held_jobs", await Request(() => (true, false, true, false), changes.Add));
        Assert.Equal([true, false], changes);
    }
    [Fact]
    public async Task ActualWriteGateOutlivesCompletedWrapperAndRetainsFreezeOnTimeout()
    {
        var frozen = false;
        Assert.Equal("pending_writes", await Request(() => (false, true, true, false), value => frozen = value));
        Assert.True(frozen);
    }
    [Fact]
    public async Task ReadOnlyActivityTimeoutResumesSchedulingRatherThanExiting()
    {
        var frozen = false;
        Assert.Equal("held_timeout", await Request(() => (false, false, false, false), value => frozen = value));
        Assert.False(frozen);
    }
    [Fact]
    public async Task ActualWriteFinishingWithinBudgetCanExitAfterDrain()
    {
        var attempts = 0;
        Assert.Equal("accepted", await Request(() => (false, ++attempts < 3, true, false), _ => { }, 400));
        Assert.True(attempts >= 3);
    }
    [Fact]
    public async Task UnknownActivityFailsClosedAndResumesOnlyIfNoWritesAreInFlight()
    {
        var frozen = false;
        Assert.Equal("held_unknown", await Request(() => (false, false, true, true), value => frozen = value));
        Assert.False(frozen);
        Assert.Equal("pending_writes", await Request(() => (false, true, true, true), value => frozen = value));
        Assert.True(frozen);
    }
    [Fact]
    public async Task ReadinessExceptionAfterKnownActualWriteDoesNotReleaseAdmission()
    {
        var frozen = false; var reads = 0;
        Assert.Equal("pending_writes", await Request(() => ++reads == 1 ? (false, true, false, false) : throw new IOException("opaque readiness"), value => frozen = value));
        Assert.True(frozen);
    }
    [Fact]
    public async Task FirstReadinessExceptionRetainsFreezeBecauseActualWriteStateIsUnknown()
    {
        var frozen = false;
        Assert.Equal("held_unknown", await Request(() => throw new IOException("unknown initial readiness"), value => frozen = value));
        Assert.True(frozen);
    }
    [Fact]
    public void HeadlessExitArgumentsAreExactAndCannotSelectAnArbitraryPidOrAction()
    {
        var method = Service("OwnedManualExitProtocol").GetMethod("AcceptsArguments")!;
        bool Accept(params string[] args) => (bool)method.Invoke(null, [args])!;
        Assert.True(Accept("--request-manual-exit"));
        Assert.False(Accept("--request-manual-exit", "37456"));
        Assert.False(Accept("--request-manual-exit", "--manual-control"));
        Assert.False(Accept("--remote-control", "stop"));
    }
    [Fact]
    public void ProtocolRejectsUnknownDuplicateOversizedReplayAndWrongOwnerRequests()
    {
        var method = Service("OwnedManualExitProtocol").GetMethod("IsValidRequest")!;
        var nonce = new string('a', 64); var owner = new string('b', 64);
        bool Valid(string json) => (bool)method.Invoke(null, [json, nonce, owner])!;
        Assert.True(Valid(JsonSerializer.Serialize(new { action = "graceful_exit", nonce, owner })));
        Assert.False(Valid(JsonSerializer.Serialize(new { action = "kill", nonce, owner })));
        Assert.False(Valid(JsonSerializer.Serialize(new { action = "graceful_exit", nonce, owner, pid = 1 })));
        Assert.False(Valid($"{{\"action\":\"kill\",\"action\":\"graceful_exit\",\"nonce\":\"{nonce}\",\"owner\":\"{owner}\"}}"));
        Assert.False(Valid(JsonSerializer.Serialize(new { action = "graceful_exit", nonce = new string('c', 64), owner })));
        Assert.False(Valid(JsonSerializer.Serialize(new { action = "graceful_exit", nonce, owner = new string('c', 64) })));
        Assert.False(Valid(new string(' ', 4097) + JsonSerializer.Serialize(new { action = "graceful_exit", nonce, owner })));
    }
    [Fact]
    public void ExitHeadlessEntryPrecedesSettingsAndWindowAndManualDrainIncludesActualWorkGates()
    {
        var root = @"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\source\ai-control-tower\src\AIControlTower";
        var app = File.ReadAllText(Path.Combine(root, "App.xaml.cs"));
        var startup = app[app.IndexOf("protected override void OnStartup", StringComparison.Ordinal)..];
        Assert.Contains("TryStartManualExit", startup);
        Assert.True(startup.IndexOf("TryStartManualExit", StringComparison.Ordinal) < startup.IndexOf("TryStartRecordPublishing", StringComparison.Ordinal));
        var drainPath = Path.Combine(root, "ViewModels", "ManualExitViewModel.cs"); Assert.True(File.Exists(drainPath));
        var drain = File.ReadAllText(drainPath);
        foreach (var gate in new[] { "_manualCollection.IsRunning", "_registeredRecordPublishing.IsRunning", "_manualCollectionTask", "_recordPublicationTask", "PcConnection.OwnedActivitiesIdle", "_communicationLock", "_discoveryLock", "_serverLock", "_rosterLock", "initializeTask", "_manualTimerCycles" })
            Assert.Contains(gate, drain);
        Assert.DoesNotContain("StopAllOwned", drain); Assert.DoesNotContain("CloseMainWindow", drain);
    }
}
