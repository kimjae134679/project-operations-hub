using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32.SafeHandles;

namespace AIControlTower.Services;

public sealed record OwnedManualExitIdentity(int ProcessId, long StartUtcTicks, int SessionId, string ExecutablePath, string BuildHash)
{
    public static OwnedManualExitIdentity Current()
    {
        using var process = Process.GetCurrentProcess();
        return Capture(process);
    }
    internal static OwnedManualExitIdentity Capture(Process process)
    {
        _ = process.Handle; // Retain OS identity while querying, never select a process to stop.
        var path = Path.GetFullPath(process.MainModule?.FileName ?? throw new InvalidDataException("unknown_executable"));
        CheckNoReparse(path);
        var exeHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        // Unbundled apphost bytes alone do not identify the managed build.
        var build = exeHash + "\n" + typeof(OwnedManualExitIdentity).Assembly.ManifestModule.ModuleVersionId;
        return new(process.Id, process.StartTime.ToUniversalTime().Ticks, process.SessionId, path,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(build))));
    }
    public bool SameBuild(OwnedManualExitIdentity peer) => SessionId == peer.SessionId
        && string.Equals(ExecutablePath, peer.ExecutablePath, StringComparison.OrdinalIgnoreCase)
        && BuildHash == peer.BuildHash;
    public bool SameInstance(OwnedManualExitIdentity peer) => SameBuild(peer) && ProcessId == peer.ProcessId && StartUtcTicks == peer.StartUtcTicks;
    internal static void CheckNoReparse(string path)
    {
        for (var current = Path.GetFullPath(path); !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("reparse_path");
    }
}

/// <summary>Manual owner only. One bounded, same-build graceful-exit action; not a command/PID control server.</summary>
public sealed class OwnedManualExitEndpoint : IDisposable
{
    private sealed record Descriptor(int SchemaVersion, string Mode, OwnedManualExitIdentity Identity, string InstanceNonce, string PipeName)
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public string Owner => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this))));
    }
    private readonly Descriptor _descriptor;
    private readonly string _path;
    private readonly Func<Task<string>> _request;
    private readonly Action _accepted;
    private readonly CancellationTokenSource _lifetime = new();
    private NamedPipeServerStream? _pipe;
    private Task _server = Task.CompletedTask;
    private OwnedManualExitEndpoint(string path, Func<Task<string>> request, Action accepted)
    {
        ValidateDescriptorPath(path);
        _path = path; _request = request; _accepted = accepted;
        var identity = OwnedManualExitIdentity.Current(); var nonce = OwnedManualExitProtocol.Nonce();
        _descriptor = new(1, "manual-control", identity, nonce, $"AIControlTower.ManualExit.{identity.SessionId}.{identity.ProcessId}.{nonce}");
    }
    public static Task<OwnedManualExitEndpoint> StartAsync(string descriptorPath, Func<Task<string>> request, Action accepted)
    {
        var endpoint = new OwnedManualExitEndpoint(descriptorPath, request, accepted);
        try
        {
            endpoint._pipe = endpoint.CreatePipe();
            var directory = Path.GetDirectoryName(descriptorPath)!;
            Directory.CreateDirectory(directory); OwnedManualExitIdentity.CheckNoReparse(directory);
            var temporary = descriptorPath + "." + endpoint._descriptor.InstanceNonce + ".tmp";
            try
            {
                File.WriteAllText(temporary, JsonSerializer.Serialize(endpoint._descriptor), new UTF8Encoding(false));
                OwnedManualExitIdentity.CheckNoReparse(descriptorPath);
                File.Move(temporary, descriptorPath, true);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            endpoint._server = Task.Run(endpoint.ServeAsync);
            return Task.FromResult(endpoint);
        }
        catch { endpoint.Dispose(); throw; }
    }
    private NamedPipeServerStream CreatePipe() => new(_descriptor.PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
        PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly | PipeOptions.FirstPipeInstance);
    private async Task ServeAsync()
    {
        while (!_lifetime.IsCancellationRequested)
        {
            var pipe = _pipe!;
            try
            {
                await pipe.WaitForConnectionAsync(_lifetime.Token).ConfigureAwait(false);
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token); deadline.CancelAfter(TimeSpan.FromSeconds(12));
                if (!GetNamedPipeClientProcessId(pipe.SafePipeHandle, out var pid)) throw new InvalidDataException("peer_unknown");
                using var peer = Process.GetProcessById(checked((int)pid));
                if (!_descriptor.Identity.SameBuild(OwnedManualExitIdentity.Capture(peer)))
                { await SendAsync(pipe, JsonSerializer.Serialize(new { status = "identity_rejected" }), deadline.Token); throw new InvalidDataException("peer_rejected"); }
                var nonce = OwnedManualExitProtocol.Nonce(); // Fresh one-use nonce for this connection only.
                await SendAsync(pipe, JsonSerializer.Serialize(new { nonce, owner = _descriptor.Owner }), deadline.Token);
                var json = await ReceiveAsync(pipe, deadline.Token);
                if (!OwnedManualExitProtocol.IsValidRequest(json, nonce, _descriptor.Owner))
                { await SendAsync(pipe, JsonSerializer.Serialize(new { status = "identity_rejected" }), deadline.Token); throw new InvalidDataException("nonce_rejected"); }
                // Do not cancel a dispatched drain on transport loss; the client must reconcile unknown outcomes.
                var status = await _request().ConfigureAwait(false);
                if (status == "accepted") status = "graceful_exit_accepted";
                if (OwnedManualExitProtocol.ExitCode(status) == 6) status = "held_unknown";
                await SendAsync(pipe, JsonSerializer.Serialize(new { status }), deadline.Token);
                if (status == "graceful_exit_accepted") { _accepted(); return; }
            }
            catch (Exception) { if (_lifetime.IsCancellationRequested) return; }
            finally { pipe.Dispose(); }
            if (_lifetime.IsCancellationRequested) return;
            try { _pipe = CreatePipe(); } catch { return; } // Never fall back to an unauthenticated channel.
        }
    }
    public static async Task<string> RequestAsync(string descriptorPath, CancellationToken ct = default)
    {
        var dispatched = false;
        try
        {
            ValidateDescriptorPath(descriptorPath);
            if (!File.Exists(descriptorPath)) return "unsupported";
            var info = new FileInfo(descriptorPath); if (info.Length > OwnedManualExitProtocol.MaximumBytes) return "identity_rejected";
            var descriptor = ReadDescriptor(File.ReadAllText(descriptorPath));
            var self = OwnedManualExitIdentity.Current();
            if (!descriptor.Identity.SameBuild(self)) return "identity_rejected";
            using var ownerProcess = Process.GetProcessById(descriptor.Identity.ProcessId);
            if (!descriptor.Identity.SameInstance(OwnedManualExitIdentity.Capture(ownerProcess))) return "identity_rejected";
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct); deadline.CancelAfter(TimeSpan.FromSeconds(12));
            using var pipe = new NamedPipeClientStream(".", descriptor.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            await pipe.ConnectAsync(3000, deadline.Token).ConfigureAwait(false);
            if (!GetNamedPipeServerProcessId(pipe.SafePipeHandle, out var pid) || pid != descriptor.Identity.ProcessId || ownerProcess.HasExited
                || !descriptor.Identity.SameInstance(OwnedManualExitIdentity.Capture(ownerProcess))) return "identity_rejected";
            using var challenge = OwnedManualExitProtocol.Parse(await ReceiveAsync(pipe, deadline.Token), "nonce", "owner");
            var nonce = challenge.RootElement.GetProperty("nonce").GetString();
            if (!OwnedManualExitProtocol.Token(nonce) || challenge.RootElement.GetProperty("owner").GetString() != descriptor.Owner) return "identity_rejected";
            dispatched = true; // Even a partial write is ambiguous; never retry automatically.
            await SendAsync(pipe, JsonSerializer.Serialize(new { action = "graceful_exit", nonce, owner = descriptor.Owner }), deadline.Token);
            using var response = OwnedManualExitProtocol.Parse(await ReceiveAsync(pipe, deadline.Token), "status");
            var status = response.RootElement.GetProperty("status").GetString() ?? "outcome_unknown";
            return OwnedManualExitProtocol.ExitCode(status) == 6 ? "outcome_unknown" : status;
        }
        catch (Exception ex)
        {
            if (dispatched) return "outcome_unknown";
            return ex is TimeoutException or OperationCanceledException or FileNotFoundException or DirectoryNotFoundException ? "unsupported" : "identity_rejected";
        }
    }
    private static Descriptor ReadDescriptor(string json)
    {
        using var document = OwnedManualExitProtocol.Parse(json, "SchemaVersion", "Mode", "Identity", "InstanceNonce", "PipeName");
        using var identity = OwnedManualExitProtocol.Parse(document.RootElement.GetProperty("Identity").GetRawText(), "ProcessId", "StartUtcTicks", "SessionId", "ExecutablePath", "BuildHash");
        var descriptor = JsonSerializer.Deserialize<Descriptor>(json) ?? throw new InvalidDataException("invalid_owner");
        var id = descriptor.Identity;
        if (descriptor.SchemaVersion != 1 || descriptor.Mode != "manual-control" || id.ProcessId <= 0 || id.StartUtcTicks <= 0 || id.SessionId < 0
            || !Path.IsPathFullyQualified(id.ExecutablePath) || id.ExecutablePath != Path.GetFullPath(id.ExecutablePath)
            || !OwnedManualExitProtocol.Token(id.BuildHash) || !OwnedManualExitProtocol.Token(descriptor.InstanceNonce)
            || descriptor.PipeName != $"AIControlTower.ManualExit.{id.SessionId}.{id.ProcessId}.{descriptor.InstanceNonce}") throw new InvalidDataException("invalid_owner");
        return descriptor;
    }
    private static void ValidateDescriptorPath(string path)
    {
        if (!Path.IsPathFullyQualified(path) || !Path.GetFullPath(path).StartsWith(@"D:\", StringComparison.OrdinalIgnoreCase)
            || Path.GetFileName(path) != "manual-exit-owner.json") throw new InvalidDataException("invalid_descriptor_path");
        OwnedManualExitIdentity.CheckNoReparse(path);
    }
    private static async Task SendAsync(Stream stream, string json, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(json); if (bytes.Length > OwnedManualExitProtocol.MaximumBytes) throw new InvalidDataException("oversized");
        await stream.WriteAsync(BitConverter.GetBytes(bytes.Length), ct); await stream.WriteAsync(bytes, ct); await stream.FlushAsync(ct);
    }
    private static async Task<string> ReceiveAsync(Stream stream, CancellationToken ct)
    {
        var header = new byte[4]; await stream.ReadExactlyAsync(header, ct); var length = BitConverter.ToInt32(header);
        if (length < 1 || length > OwnedManualExitProtocol.MaximumBytes) throw new InvalidDataException("oversized");
        var bytes = new byte[length]; await stream.ReadExactlyAsync(bytes, ct); return new UTF8Encoding(false, true).GetString(bytes);
    }
    public void Dispose()
    {
        _lifetime.Cancel(); _pipe?.Dispose();
        try
        {
            ValidateDescriptorPath(_path);
            if (File.Exists(_path) && new FileInfo(_path).Length <= OwnedManualExitProtocol.MaximumBytes
                && ReadDescriptor(File.ReadAllText(_path)).Owner == _descriptor.Owner) File.Delete(_path);
        }
        catch (Exception) { } // Never erase another owner's descriptor.
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(SafePipeHandle pipe, out uint processId);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeServerProcessId(SafePipeHandle pipe, out uint processId);
}
