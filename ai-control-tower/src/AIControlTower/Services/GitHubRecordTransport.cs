using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AIControlTower.Services;

/// <summary>Inherited gh authentication only; bounded raw JSON is private, never a UI/log payload.</summary>
public sealed class GitHubRecordTransport
{
    public const string GhPath = @"C:\Program Files\GitHub CLI\gh.exe";
    private const int MaximumResponseChars = 4 * 1024 * 1024;
    public static ProcessStartInfo BuildStartInfo(string method, string endpoint, bool hasBody)
    {
        if (method is not ("GET" or "POST") || !Regex.IsMatch(endpoint,
            @"^repos/[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+/[A-Za-z0-9_./?%=&:+-]+$|^repos/[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$", RegexOptions.CultureInvariant))
            throw new ArgumentException("Unsupported record API request");
        var info = new ProcessStartInfo(GhPath)
        {
            UseShellExecute=false, CreateNoWindow=true, WindowStyle=ProcessWindowStyle.Hidden,
            RedirectStandardInput=true, RedirectStandardOutput=true, RedirectStandardError=true,
            StandardOutputEncoding=Encoding.UTF8, StandardErrorEncoding=Encoding.UTF8
        };
        foreach (var arg in new[] {"api", "--hostname", "github.com", "--method", method, "--include", "-H",
            "Accept: application/vnd.github+json", "-H", "X-GitHub-Api-Version: 2022-11-28", endpoint}) info.ArgumentList.Add(arg);
        if (hasBody) { info.ArgumentList.Add("--input"); info.ArgumentList.Add("-"); }
        return info;
    }
    public async Task<(int StatusCode, string Json)> SendAsync(string method, string endpoint, string? body, CancellationToken ct)
    {
        if (body is { Length: > MaximumResponseChars }) throw new IOException("Record API request limit");
        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromSeconds(45));
        using var process=new Process { StartInfo=BuildStartInfo(method, endpoint, body is not null) };
        if (!process.Start()) throw new IOException("GitHub transport unavailable");
        try
        {
            var stdout=ReadBoundedAsync(process.StandardOutput, timeout.Token);
            var stderr=ReadBoundedAsync(process.StandardError, timeout.Token);
            if (body is not null) await process.StandardInput.WriteAsync(body.AsMemory(), timeout.Token).ConfigureAwait(false);
            process.StandardInput.Close();
            await Task.WhenAll(process.WaitForExitAsync(timeout.Token), stdout, stderr).ConfigureAwait(false);
            // gh --include emits the HTTP status separately from JSON. Never treat failed auth as a missing ref.
            var raw=await stdout.ConfigureAwait(false);
            var separator=raw.IndexOf("\r\n\r\n", StringComparison.Ordinal); var width=4;
            if (separator<0) { separator=raw.IndexOf("\n\n", StringComparison.Ordinal); width=2; }
            var first=raw.Split('\n', 2)[0].TrimEnd('\r');
            var match=Regex.Match(first, @"^HTTP/\S+ ([0-9]{3})(?:\s|$)", RegexOptions.CultureInvariant);
            if (!match.Success || separator<0) throw new IOException("GitHub HTTP response unavailable");
            var status=int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            if (process.ExitCode!=0 && status is >=200 and <300) throw new IOException("GitHub response not confirmed");
            return (status, raw[(separator+width)..]);
        }
        catch
        {
            try { if (!process.HasExited) process.Kill(entireProcessTree:true); } catch { /* No retry or raw diagnostics. */ }
            throw new IOException("GitHub transport result not confirmed");
        }
    }
    private static async Task<string> ReadBoundedAsync(StreamReader reader, CancellationToken ct)
    {
        var builder=new StringBuilder(); var buffer=new char[8192]; int read;
        while ((read=await reader.ReadAsync(buffer.AsMemory(), ct).ConfigureAwait(false))>0)
        { if (builder.Length+read>MaximumResponseChars) throw new IOException("GitHub response limit"); builder.Append(buffer,0,read); }
        return builder.ToString();
    }
}
