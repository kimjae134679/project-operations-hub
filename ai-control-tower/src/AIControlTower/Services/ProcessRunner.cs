using System.Diagnostics;
using System.Text;
namespace AIControlTower.Services;
public sealed record ProcessRunResult(int ExitCode, string StandardOutput, string StandardError, bool TimedOut);
public sealed class ProcessRunner
{
    public async Task<ProcessRunResult> RunHiddenAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(timeout);
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
            }
        };
        var started = false;
        try
        {
            process.Start(); started = true;
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            try { await process.WaitForExitAsync(linked.Token).ConfigureAwait(false); }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(true);
                await process.WaitForExitAsync().ConfigureAwait(false);
                return new(-1, "", "", true);
            }
            return new(process.ExitCode, Sanitize(await output.ConfigureAwait(false)), Sanitize(await error.ConfigureAwait(false)), false);
        }
        catch (Exception ex)
        {
            if (started && !process.HasExited) { process.Kill(true); await process.WaitForExitAsync().ConfigureAwait(false); }
            return new(1, "", Sanitize(ex.Message), false);
        }
    }
    public static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var lines = value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Where(line => !System.Text.RegularExpressions.Regex.IsMatch(line, @"(?i)(api[_-]?key|authorization|bearer\s|secret|password|access[_-]?token|refresh[_-]?token|token=)"))
            .Select(line => line.Length > 240 ? line[..240] + "…" : line);
        return string.Join(Environment.NewLine, lines).Trim();
    }
}
