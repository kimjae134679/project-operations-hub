using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class OwnedManualExitPipeTests
{
    private const string FixtureBase = @"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\manual-exit-pipe";
    private static string Fixture()
    {
        var directory = Path.Combine(FixtureBase, Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        return Path.Combine(directory, "manual-exit-owner.json");
    }
    private static void Cleanup(string path)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        Assert.StartsWith(Path.GetFullPath(FixtureBase) + Path.DirectorySeparatorChar, directory);
        foreach (var file in Directory.EnumerateFiles(directory)) File.Delete(file);
        Directory.Delete(directory);
    }
    private static async Task<string> Read(Stream stream)
    {
        var header = new byte[4]; await stream.ReadExactlyAsync(header); var count = BitConverter.ToInt32(header);
        Assert.InRange(count, 1, 4096); var bytes = new byte[count]; await stream.ReadExactlyAsync(bytes); return Encoding.UTF8.GetString(bytes);
    }
    private static async Task Send(Stream stream, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json); await stream.WriteAsync(BitConverter.GetBytes(bytes.Length)); await stream.WriteAsync(bytes); await stream.FlushAsync();
    }
    [Fact]
    public async Task CurrentUserOwnedPipeHoldsThenAcceptsWithoutGuiOrProcessStop()
    {
        var path = Fixture(); var requests = 0; var accepted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var endpoint = await OwnedManualExitEndpoint.StartAsync(path, () => Task.FromResult(++requests == 1 ? "held_jobs" : "accepted"), () => accepted.TrySetResult());
        try
        {
            Assert.Equal("held_jobs", await OwnedManualExitEndpoint.RequestAsync(path)); Assert.False(accepted.Task.IsCompleted);
            Assert.Equal("graceful_exit_accepted", await OwnedManualExitEndpoint.RequestAsync(path));
            await accepted.Task.WaitAsync(TimeSpan.FromSeconds(5)); Assert.Equal(2, requests);
        }
        finally { endpoint.Dispose(); Assert.False(File.Exists(path)); Cleanup(path); }
    }
    [Fact]
    public async Task ActualPipeRejectsWrongNonceAndNextConnectionGetsFreshChallenge()
    {
        var path = Fixture(); var requests = 0; using var endpoint = await OwnedManualExitEndpoint.StartAsync(path, () => { requests++; return Task.FromResult("held_jobs"); }, () => Assert.Fail("No exit for a rejected request."));
        try
        {
            using var owner = JsonDocument.Parse(File.ReadAllText(path)); var pipeName = owner.RootElement.GetProperty("PipeName").GetString()!;
            string? previous = null;
            for (var index = 0; index < 2; index++)
            {
                using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await pipe.ConnectAsync(3000);
                using var challenge = JsonDocument.Parse(await Read(pipe)); var nonce = challenge.RootElement.GetProperty("nonce").GetString()!;
                if (previous is not null) Assert.NotEqual(previous, nonce);
                await Send(pipe, JsonSerializer.Serialize(new { action = "graceful_exit", nonce = previous ?? new string('0', 64), owner = challenge.RootElement.GetProperty("owner").GetString() }));
                using var response = JsonDocument.Parse(await Read(pipe)); Assert.Equal("identity_rejected", response.RootElement.GetProperty("status").GetString());
                previous = nonce;
            }
            Assert.Equal(0, requests);
            Assert.Equal("held_jobs", await OwnedManualExitEndpoint.RequestAsync(path)); Assert.Equal(1, requests);
        }
        finally { endpoint.Dispose(); Cleanup(path); }
    }
    [Fact]
    public void SameAccountOtherAppBuildSessionAndPidReuseCannotMatchOwner()
    {
        var own = OwnedManualExitIdentity.Current();
        Assert.False(own.SameBuild(own with { ExecutablePath = @"D:\fixture\other.exe" }));
        Assert.False(own.SameBuild(own with { BuildHash = new string('0', 64) }));
        Assert.False(own.SameBuild(own with { SessionId = own.SessionId + 1 }));
        Assert.False(own.SameInstance(own with { StartUtcTicks = own.StartUtcTicks + 1 }));
        Assert.False(own.SameInstance(own with { ProcessId = own.ProcessId + 1 }));
    }
    [Fact]
    public async Task ActualSameAccountForeignPowerShellPipePeerIsRejectedBeforeAnyDrain()
    {
        var path = Fixture(); var requests = 0;
        using var endpoint = await OwnedManualExitEndpoint.StartAsync(path, () => { requests++; return Task.FromResult("held_jobs"); }, () => Assert.Fail("Foreign peer cannot exit owner."));
        try
        {
            using var owner = JsonDocument.Parse(File.ReadAllText(path));
            var script = Path.Combine(Path.GetDirectoryName(path)!, "foreign-pipe-client.ps1");
            File.WriteAllText(script, """
                param([string]$PipeName)
                $ErrorActionPreference='Stop'
                $pipe=[IO.Pipes.NamedPipeClientStream]::new('.', $PipeName, [IO.Pipes.PipeDirection]::InOut)
                try {
                    $pipe.Connect(3000)
                    function Read-Exact([int]$count) {
                        $buffer=New-Object byte[] $count; $offset=0
                        while($offset -lt $count){$read=$pipe.Read($buffer,$offset,$count-$offset); if($read -eq 0){throw 'unexpected EOF'}; $offset+=$read}
                        return ,$buffer
                    }
                    $header=Read-Exact 4; $length=[BitConverter]::ToInt32($header,0)
                    if($length -lt 1 -or $length -gt 4096){throw 'invalid frame'}
                    $body=Read-Exact $length
                    $response=[Text.Encoding]::UTF8.GetString($body)|ConvertFrom-Json
                    [Console]::WriteLine([string]$response.status)
                } finally {$pipe.Dispose()}
                """, new UTF8Encoding(false));
            var pipeName = owner.RootElement.GetProperty("PipeName").GetString()!;
            var result = await new ProcessRunner().RunHiddenAsync("powershell.exe", $"-NoProfile -NonInteractive -File \"{script}\" -PipeName \"{pipeName}\"", TimeSpan.FromSeconds(8), CancellationToken.None);
            Assert.False(result.TimedOut); Assert.Equal(0, result.ExitCode);
            Assert.Equal("identity_rejected", result.StandardOutput.Trim()); Assert.Equal(0, requests);
            Assert.Equal("held_jobs", await OwnedManualExitEndpoint.RequestAsync(path)); Assert.Equal(1, requests);
        }
        finally { endpoint.Dispose(); Cleanup(path); }
    }
    [Fact]
    public async Task TamperedOwnerIsRejectedBeforeCoordinatorAndDisposeDoesNotEraseOtherOwner()
    {
        var path = Fixture(); var requests = 0; using var endpoint = await OwnedManualExitEndpoint.StartAsync(path, () => { requests++; return Task.FromResult("accepted"); }, () => Assert.Fail("Tampered owner must not exit."));
        try
        {
            var json = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(path))!;
            json["Identity"]!["BuildHash"] = new string('0', 64); File.WriteAllText(path, json.ToJsonString());
            Assert.Equal("identity_rejected", await OwnedManualExitEndpoint.RequestAsync(path)); Assert.Equal(0, requests);
            endpoint.Dispose(); Assert.True(File.Exists(path));
        }
        finally { endpoint.Dispose(); Cleanup(path); }
    }
    [Fact]
    public async Task LostResponseAfterDispatchIsUnknownAndNeverAutomaticallyReissuesRequest()
    {
        var path = Fixture(); var requests = 0;
        var dispatched = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var endpoint = await OwnedManualExitEndpoint.StartAsync(path, async () => { requests++; dispatched.TrySetResult(); await release.Task; return "held_timeout"; }, () => Assert.Fail("No accepted drain."));
        using var cancellation = new CancellationTokenSource();
        try
        {
            var request = OwnedManualExitEndpoint.RequestAsync(path, cancellation.Token);
            await dispatched.Task.WaitAsync(TimeSpan.FromSeconds(5)); cancellation.Cancel();
            Assert.Equal("outcome_unknown", await request); Assert.Equal(1, requests);
        }
        finally { release.TrySetResult(); endpoint.Dispose(); Cleanup(path); }
    }
    [Fact]
    public async Task MissingLegacyChannelIsUnsupportedAndHasNoSideEffects()
    {
        var path = Fixture();
        try { Assert.Equal("unsupported", await OwnedManualExitEndpoint.RequestAsync(path)); Assert.Empty(Directory.EnumerateFiles(Path.GetDirectoryName(path)!)); }
        finally { Cleanup(path); }
    }
}
