namespace AIControlTower.Services;

public sealed record QueueTask(string Id, string Instruction, string ProcessingPath);

public sealed class CommandQueueService
{
    public CommandQueueService(string? root = null) => RootPath = root ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower", "queue");
    public string RootPath { get; }
    public string InboxPath => Path.Combine(RootPath, "inbox");
    public string ProcessingPath => Path.Combine(RootPath, "processing");
    public string ArchivePath => Path.Combine(RootPath, "archive");
    public string ResultPath => Path.Combine(RootPath, "result");
    public void EnsureDirectories() { Directory.CreateDirectory(InboxPath); Directory.CreateDirectory(ProcessingPath); Directory.CreateDirectory(ArchivePath); Directory.CreateDirectory(ResultPath); }
    public QueueTask? TryClaimNext()
    {
        EnsureDirectories();
        var source = Directory.GetFiles(InboxPath, "*.txt").OrderBy(File.GetCreationTimeUtc).FirstOrDefault();
        if (source is null) return null;
        var target = Path.Combine(ProcessingPath, Path.GetFileName(source));
        try { File.Move(source, target); }
        catch (IOException) { return null; }
        var instruction = File.ReadAllText(target).Trim();
        if (string.IsNullOrWhiteSpace(instruction)) { File.Move(target, Path.Combine(ArchivePath, Path.GetFileName(target)), true); return null; }
        return new QueueTask(Path.GetFileNameWithoutExtension(target), instruction, target);
    }
}
