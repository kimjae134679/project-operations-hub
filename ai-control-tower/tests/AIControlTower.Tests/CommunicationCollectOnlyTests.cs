using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class CommunicationCollectOnlyTests
{
    // Owned fixtures stay on D and are retained; no operational projects or clone are consulted.
    private readonly string _fixture = Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\collect-only", Guid.NewGuid().ToString("N"));
    private readonly string _sink = Path.Combine(@"D:\A_KJ\AI\ControlTowerData\collected-communication", "test-" + Guid.NewGuid().ToString("N"));
    private string Hub => Path.Combine(_fixture,"board");
    private string Project => Path.Combine(_fixture,"project");
    private string Outbox => Path.Combine(Project,"_통합소통","보낼자료");
    private string Received => Path.Combine(Project,"_통합소통","받은공지");
    private string Receipts => Path.Combine(Project,"_통합소통","확인기록");
    private const string Notice = "# Notice\nRead personally.\n";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private CommunicationTarget Target => new("PhoneLOL",Project);

    public CommunicationCollectOnlyTests()
    {
        var board=Path.Combine(Hub,"04_COMMUNICATION","announcements");
        Directory.CreateDirectory(Path.Combine(board,"notices"));Directory.CreateDirectory(Project);
        File.WriteAllText(Path.Combine(board,"notices","N-1.md"),Notice);
        File.WriteAllText(Path.Combine(board,"manifest.json"),JsonSerializer.Serialize(new {
            schemaVersion=1,projects=new[]{new{id="PhoneLOL",name="Phone",required=true}},participants=new[]{new{id="Sol"}},
            notices=new[]{new{id="N-1",revision=1,title="Notice",path="notices/N-1.md",targets=new[]{"PhoneLOL"},required=true,active=true,contentSha256=CommunicationService.ContentHash(Notice)}}
        },JsonOptions));
        File.WriteAllText(Path.Combine(Project,"AGENTS.md"),"# Original\nKeep exactly.\n");
    }

    private async Task<JsonElement> Collect(string? sink=null, IEnumerable<CommunicationTarget>? targets=null, TimeSpan? settle=null, CancellationToken ct=default)
    {
        var method=typeof(CommunicationService).GetMethod("CollectOnlyAsync",new[]{typeof(string),typeof(IEnumerable<CommunicationTarget>),typeof(string),typeof(CancellationToken)});
        Assert.NotNull(method);
        var task=(Task)method!.Invoke(new CommunicationService(settle??TimeSpan.Zero),new object[]{Hub,targets??new[]{Target},sink??_sink,ct})!;
        await task;
        return JsonSerializer.SerializeToElement(task.GetType().GetProperty("Result")!.GetValue(task),JsonOptions);
    }
    private static JsonElement[] Records(JsonElement result)=>result.GetProperty("records").EnumerateArray().ToArray();
    private static JsonElement[] Errors(JsonElement result)=>result.GetProperty("errors").EnumerateArray().ToArray();
    private static Dictionary<string,string> Tree(string root)
    {
        return Directory.EnumerateFileSystemEntries(root,"*",SearchOption.AllDirectories).Append(root).ToDictionary(p=>Path.GetRelativePath(root,p),p=>
            ((int)File.GetAttributes(p))+"|"+(File.Exists(p)?Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)))+"|"+File.GetLastWriteTimeUtc(p).Ticks:"directory"));
    }
    private static void Unchanged(Dictionary<string,string> before,string root)
    {
        var after=Tree(root);Assert.Equal(before.Count,after.Count);
        foreach(var item in before)Assert.Equal(item.Value,after[item.Key]);
    }
    private void Put(string folder,string name,string body)
    {Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,name),body);}

    [Fact]
    public async Task CollectsOnlyExistingOutboxReceivedNoticeAndRealReceiptPreservingAllSources()
    {
        Put(Outbox,"task.json","{\"schemaVersion\":1,\"status\":\"in_progress\"}");Put(Received,"N-1.md",Notice);
        var receipt=new CommunicationReceipt {NoticeId="N-1",Revision=1,ContentSha256=CommunicationService.ContentHash(Notice),ProjectId="PhoneLOL",ActorId="Sol",SessionId="own-session",CheckedAt="2026-10-07T00:00:00Z",ApplicationStatus="pending",Note="Personally read",Evidence=["AGENTS.md"]};
        Put(Receipts,CommunicationService.ReceiptFileName(receipt.ActorId,receipt.SessionId),JsonSerializer.Serialize(receipt,JsonOptions));
        File.SetAttributes(Path.Combine(Project,"AGENTS.md"),FileAttributes.ReadOnly);
        var hubBefore=Tree(Hub);var projectBefore=Tree(Project);
        var result=await Collect();var records=Records(result);
        Assert.Empty(Errors(result));Assert.Equal(3,records.Length);
        Assert.Equal(new[]{"notice-receipt","outbox","received-notice"},records.Select(r=>r.GetProperty("kind").GetString()).Order().ToArray());
        Assert.False(result.GetProperty("centralShared").GetBoolean());Assert.False(result.GetProperty("createsAcknowledgement").GetBoolean());
        foreach(var record in records)
        {
            Assert.Equal("PhoneLOL",record.GetProperty("projectId").GetString());
            var path=record.GetProperty("storedPath").GetString()!;Assert.StartsWith(_sink+Path.DirectorySeparatorChar,path);
            Assert.Equal(record.GetProperty("contentSha256").GetString(),CommunicationService.ContentHash(File.ReadAllText(path)));
            Assert.True(File.Exists(record.GetProperty("sourcePath").GetString()));Assert.NotEmpty(record.GetProperty("sourceName").GetString()!);
        }
        Unchanged(hubBefore,Hub);Unchanged(projectBefore,Project);
    }

    [Fact]
    public async Task MissingMailboxesRemainMissingAndDoNotFabricateDeliveryOrAcknowledgement()
    {
        var before=Tree(Project);var result=await Collect();Assert.Empty(Records(result));Assert.Empty(Errors(result));Unchanged(before,Project);
        Assert.False(Directory.Exists(_sink));Assert.False(Directory.Exists(Received));Assert.False(Directory.Exists(Receipts));
    }

    [Fact]
    public async Task ContentAddressedDedupRetainsEachSourceWithoutOverwritingFilesOrTimestamps()
    {
        Put(Outbox,"one.md","# Shared\r\nBody\r\n");Put(Outbox,"two.md","# Shared\nBody\n");
        var first=await Collect();var records=Records(first);Assert.Equal(2,records.Length);
        Assert.Single(records.Select(r=>r.GetProperty("storedPath").GetString()).Distinct());
        Assert.Equal(2,records.Select(r=>r.GetProperty("sourceName").GetString()).Distinct().Count());
        var before=Tree(_sink);var second=await Collect();Assert.Equal(2,Records(second).Length);Unchanged(before,_sink);
    }

    [Theory]
    [InlineData("outside")][InlineData("prefix-sibling")][InlineData("traversal")][InlineData("board")][InlineData("project")][InlineData("clone")]
    public async Task RejectsUnapprovedSinkBeforeAnyWrite(string mode)
    {
        Put(Outbox,"one.md","Body");
        var sink=mode switch {"outside"=>Path.Combine(_fixture,"outside"),"prefix-sibling"=>@"D:\A_KJ\AI\ControlTowerData\collected-communication-evil\test", "traversal"=>Path.Combine(_sink,"..","..","escape"),"board"=>Hub,"project"=>Project,_=>_sink};
        if(mode=="clone")Directory.CreateDirectory(Path.Combine(sink,".git"));
        var hubBefore=Tree(Hub);var projectBefore=Tree(Project);var sinkBefore=Directory.Exists(sink)?Tree(sink):null;
        var result=await Collect(sink);Assert.Empty(Records(result));Assert.NotEmpty(Errors(result));Unchanged(hubBefore,Hub);Unchanged(projectBefore,Project);
        if(sinkBefore is not null)Unchanged(sinkBefore,sink);else Assert.False(Directory.Exists(sink));
    }

    [Theory]
    [InlineData("id")][InlineData("root")][InlineData("unknown")]
    public async Task RejectsAmbiguousOrUnregisteredTargetsBeforeWriting(string mode)
    {
        Put(Outbox,"one.md","Body");
        var targets=mode=="id"?new[]{Target,Target}:mode=="root"?new[]{Target,new CommunicationTarget("Other",Project)}:new[]{new CommunicationTarget("Unknown",Project)};
        var result=await Collect(targets:targets);Assert.Empty(Records(result));Assert.NotEmpty(Errors(result));Assert.False(Directory.Exists(_sink));
    }

    [Theory]
    [InlineData("secret")][InlineData("invalid-json")][InlineData("oversize")][InlineData("unsupported")][InlineData("secret-name")]
    public async Task HoldsUnsafeDocumentsWithoutCreatingSink(string mode)
    {
        var name=mode=="secret-name"?"password=private-value.md":mode=="invalid-json"?"bad.json":mode=="unsupported"?"script.ps1":"item.md";
        var body=mode switch {"secret"=>"access_token=private-value", "invalid-json"=>"{unfinished", "oversize"=>new string('a',CommunicationService.MaximumFileBytes+1),_=>"Body"};
        Put(Outbox,name,body);var before=Tree(Project);var result=await Collect();Assert.Empty(Records(result));Assert.False(Directory.Exists(_sink));Unchanged(before,Project);
        if(mode!="unsupported")Assert.NotEmpty(Errors(result));
    }

    [Fact]
    public async Task ChangedNoticeAndWrongReceiptOwnershipNeverBecomeCollectedEvidence()
    {
        Put(Received,"N-1.md","Changed body");Put(Receipts,"arbitrary.json","{}");
        var result=await Collect();Assert.Empty(Records(result));Assert.NotEmpty(Errors(result));Assert.False(Directory.Exists(_sink));
    }

    [Fact]
    public async Task SettlingDocumentIsHeldWithoutSourceOrSinkMutation()
    {
        Put(Outbox,"item.md","Body");var before=Tree(Project);var result=await Collect(settle:TimeSpan.FromDays(1));
        Assert.Empty(Records(result));Assert.NotEmpty(Errors(result));Unchanged(before,Project);Assert.False(Directory.Exists(_sink));
    }

    [Fact]
    public async Task ExistingConflictingContentIsPreservedAndHeldNeverOverwritten()
    {
        Put(Outbox,"item.md","Body");var first=await Collect();var path=Assert.Single(Records(first)).GetProperty("storedPath").GetString()!;
        File.WriteAllText(path,"Independently edited");var before=Tree(_sink);var result=await Collect();
        Assert.Empty(Records(result));Assert.NotEmpty(Errors(result));Unchanged(before,_sink);
    }

    [Fact]
    public async Task ReparseOutboxAndSinkAreRejectedWithoutFollowingLinks()
    {
        var outside=Path.Combine(_fixture,"outside");Directory.CreateDirectory(outside);File.WriteAllText(Path.Combine(outside,"item.md"),"Body");
        Directory.CreateDirectory(Path.GetDirectoryName(Outbox)!);Directory.CreateSymbolicLink(Outbox,outside);
        var result=await Collect();Assert.Empty(Records(result));Assert.NotEmpty(Errors(result));Assert.False(Directory.Exists(_sink));
        var linkSink=Path.Combine(@"D:\A_KJ\AI\ControlTowerData\collected-communication","link-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.GetDirectoryName(linkSink)!);Directory.CreateSymbolicLink(linkSink,outside);
        var before=Tree(outside);result=await Collect(linkSink);Assert.Empty(Records(result));Assert.NotEmpty(Errors(result));Unchanged(before,outside);
    }

    [Fact]
    public async Task InvalidBoardFailsClosedBeforeCollectingOutbox()
    {
        Put(Outbox,"item.md","Body");File.WriteAllText(Path.Combine(Hub,"04_COMMUNICATION","announcements","manifest.json"),"{}");
        var result=await Collect();Assert.Empty(Records(result));Assert.NotEmpty(Errors(result));Assert.False(Directory.Exists(_sink));
    }
}
