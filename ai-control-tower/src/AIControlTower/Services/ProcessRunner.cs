using System.Diagnostics;

namespace AIControlTower.Services;

public sealed record ProcessRunResult(int ExitCode, string StandardOutput, string StandardError, bool TimedOut);

public sealed class ProcessRunner
{
    public async Task<ProcessRunResult> RunHiddenAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = process.StandardError.ReadToEndAsync(cancellationToken);
            var waitTask = process.WaitForExitAsync(cancellationToken);
            var completed = await Task.WhenAny(waitTask, Task.Delay(timeout, cancellationToken));
            if (completed != waitTask)
            {
                if (!process.HasExited) process.Kill(true);
                return new ProcessRunResult(-1, string.Empty, string.Empty, true);
            }
            return new ProcessRunResult(process.ExitCode, Sanitize(await output), Sanitize(await error), false);
        }
        catch (OperationCanceledException) { return new ProcessRunResult(-1, string.Empty, string.Empty, true); }
        catch (Exception ex) { return new ProcessRunResult(1, string.Empty, Sanitize(ex.Message), false); }
    }

    public static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var lines = value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Where(line => !line.Contains("api_key", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("token=", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("password=", StringComparison.OrdinalIgnoreCase))
            .Select(line => line.Length > 240 ? line[..240] + "…" : line);
        return string.Join(Environment.NewLine, lines).Trim();
    }
}
