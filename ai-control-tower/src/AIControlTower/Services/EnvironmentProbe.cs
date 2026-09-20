namespace AIControlTower.Services;

public static class EnvironmentProbe
{
    public static string? FindCommand(string command)
    {
        var candidates = command.Contains(Path.DirectorySeparatorChar) ? new[] { command } : BuildCandidates(command);
        return candidates.FirstOrDefault(File.Exists);
    }

    public static bool HasAnyEnvironmentVariable(params string[] names) => names.Any(name => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)));

    private static IEnumerable<string> BuildCandidates(string command)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var extensions = new[] { string.Empty, ".exe", ".cmd", ".bat" };
        foreach (var folder in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        foreach (var extension in extensions)
            yield return Path.Combine(folder.Trim('"'), command + extension);
    }
}
