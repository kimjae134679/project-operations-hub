using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using AIControlTower.Services;
using AIControlTower.ViewModels;

internal static class Program
{
    // Read-only model audit: no window, polling, service command, or settings save.
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length != 2)
            throw new ArgumentException("Usage: Audit <project-root> <private-output-json>");
        var root = Path.GetFullPath(args[0]);
        var output = Path.GetFullPath(args[1]);
        if (!Directory.Exists(Path.GetDirectoryName(output)))
            throw new ArgumentException("Private output directory must already exist.");
        var discovery = new ProjectDiscoveryService().Scan(root);
        var warnings = discovery.Warnings.ToList();
        var projects = new ProjectCatalogService().Apply(discovery.Projects, root,
            ProjectCatalogService.Load(), warnings);
        var targets = projects.Select(p => new
        {
            p.Id, p.DisplayName, p.Path, p.ManifestPath,
            programs = p.Functions.SelectMany(f => f.Programs).Select(x => new
            {
                x.Id, x.DisplayName, x.Path,
                pathExists = File.Exists(x.Path) || Directory.Exists(x.Path),
                x.ActionLabel, x.OpenActionLabel,
                loopbackUrl = Uri.TryCreate(x.ServiceUrl, UriKind.Absolute, out var url)
                    && url.IsLoopback ? url.AbsoluteUri : null,
                // Do not serialize command arguments: they may contain sensitive values.
                commands = x.Commands.Select(c => new
                {
                    c.Name, c.FileName,
                    absoluteExecutableExists = Path.IsPathRooted(c.FileName)
                        ? File.Exists(c.FileName) : (bool?)null,
                    c.TimeoutSeconds
                })
            })
        });
        var settings = new ControlTowerSettings
        {
            RootPath = root, IsTemporary = true, TransientReadOnly = true,
            TransientDataDirectory = Path.Combine(Path.GetDirectoryName(output)!, "unwritten-audit-data"),
            CommunicationHubPath = Path.Combine(root, "Projects", "project-operations-hub")
        };
        using var vm = new MainViewModel(settings, false, () => {}, new StartupPolicy(true), null);
        foreach (var project in projects) vm.Projects.Add(project);
        var a = projects.Single(p => p.Id == "Threads");
        var b = projects.First(p => p.Id != "Threads" && p.Functions.Any());
        var selected = a.Functions.SelectMany(f => f.Programs)
            .Single(x => x.Id.EndsWith("/upload-studio-open", StringComparison.Ordinal));
        vm.SelectedProject = a;
        vm.SelectedProgram = selected;
        vm.ProgramSearch = "검토";
        var originalProgram = vm.SelectedProgram!.Id;
        var originalSearch = vm.ProgramSearch;
        vm.SelectedProject = b;
        vm.SelectedProject = a;
        var transition = new
        {
            a = a.Id, b = b.Id, originalProgram, afterProgram = vm.SelectedProgram?.Id,
            originalSearch, afterSearch = vm.ProgramSearch,
            programPreserved = vm.SelectedProgram?.Id == originalProgram,
            searchPreserved = vm.ProgramSearch == originalSearch
        };
        vm.SelectedProgram = selected;
        vm.ProgramSearch = "검토";
        var reader = WorkspaceReaderSelection.Capture(vm);
        vm.SelectedProject = b;
        reader.Restore(vm);
        var readerRestore = new
        {
            projectPreserved = vm.SelectedProject?.Id == a.Id,
            programPreserved = vm.SelectedProgram?.Id == selected.Id,
            searchPreserved = vm.ProgramSearch == "검토", actualReaderUiClick = false
        };
        var assembly = typeof(MainViewModel).Assembly;
        var proof = new
        {
            checkedAt = DateTimeOffset.UtcNow,
            assemblyVersion = assembly.GetName().Version?.ToString(),
            assemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))),
            readOnly = true, serviceCommandsExecuted = 0, windowsCreated = 0,
            actualNativeUiAvailable = false, transition, readerRestore, projects = targets, warnings
        };
        File.WriteAllText(output, JsonSerializer.Serialize(proof, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            path = output, projects = projects.Count, transition, readerRestore,
            serviceCommandsExecuted = 0, windowsCreated = 0
        }));
    }
}
