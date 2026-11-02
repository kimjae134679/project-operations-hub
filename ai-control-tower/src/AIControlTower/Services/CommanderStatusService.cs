using System.Globalization;
using System.Text.Json;

namespace AIControlTower.Services;

// Connector observations are sanitized metadata, not device credentials or a live quota API.
public sealed record CommanderStatusSnapshot(bool ObservationFresh=false,DateTimeOffset? ObservedAtUtc=null,
    bool? Online=null,bool? Authenticated=null,double? RemainingPercent=null,bool SupervisorFresh=false)
{
    public bool RemoteCallsReady=>ObservationFresh&&Online==true&&Authenticated==true&&RemainingPercent is >0;
    public string DisplayText
    {
        get
        {
            var connection=Online is null?"Online 상태 미확인":Online==true?"Online":"Offline";
            var auth=Authenticated is null?"인증 미확인":Authenticated==true?"인증 확인됨":"인증 확인 필요";
            var quota=RemainingPercent is null?"원격 호출 잔여량 미확인":"원격 호출 잔여 "+RemainingPercent.Value.ToString("0.##",CultureInfo.InvariantCulture)+"%";
            var at=ObservedAtUtc is null?"":" · "+ObservedAtUtc.Value.ToLocalTime().ToString("MM-dd HH:mm:ss",CultureInfo.InvariantCulture)+" 확인";
            return "Desktop Commander · "+(ObservedAtUtc is not null&&!ObservationFresh?"마지막 확인 ":"")+connection+" · "+auth+" · "+quota+at+
                (ObservationFresh?" (연결/인증과 호출 가능량은 별도)":" · 현재 재확인 필요");
        }
    }
    public string RecoveryText=>SupervisorFresh?"Desktop Commander 로컬 자동 복구 감시: 최근 응답 확인됨 (클라우드 Online·잔여량을 뜻하지 않습니다)":"Desktop Commander 로컬 자동 복구 감시: 최근 응답 미확인";
}

public sealed class CommanderStatusService
{
    private readonly string _directory;
    public CommanderStatusService(string? directory=null)=>_directory=directory??ControlTowerSettings.DataDirectory;
    private JsonDocument? ReadDocument(string filename)
    {
        try
        {
            var path=Path.Combine(_directory,filename);
            for(var item=path;!string.IsNullOrEmpty(item);item=Path.GetDirectoryName(item))
                if((File.Exists(item)||Directory.Exists(item))&&(File.GetAttributes(item)&FileAttributes.ReparsePoint)!=0)return null;
            using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read|FileShare.Delete);
            if(stream.Length>64*1024)return null;
            return JsonDocument.Parse(stream,new JsonDocumentOptions{MaxDepth=8});
        }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException or JsonException){return null;}
    }
    private static bool? Bool(JsonElement row,string key)=>row.TryGetProperty(key,out var value)?value.ValueKind switch{JsonValueKind.True=>true,JsonValueKind.False=>false,_=>null}:null;
    private static DateTimeOffset? Timestamp(JsonElement row,string key)
        =>DateTimeOffset.TryParse(ProjectBridgeService.Text(row,key),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind,out var at)&&at.Offset==TimeSpan.Zero?at:null;
    private static bool Fresh(DateTimeOffset? at,DateTimeOffset now,TimeSpan maximumAge)=>at is not null&&at<=now.AddSeconds(30)&&now-at<=maximumAge;
    public CommanderStatusSnapshot Read(DateTimeOffset now)
    {
        var result=new CommanderStatusSnapshot();
        using(var doc=ReadDocument("remote-commander-observation.json"))
        {
            if(doc is not null&&doc.RootElement.ValueKind==JsonValueKind.Object)
            {
                var row=doc.RootElement;
                if(row.TryGetProperty("schemaVersion",out var schema)&&schema.ValueKind==JsonValueKind.Number&&schema.TryGetInt32(out var version)&&version==1&&ProjectBridgeService.Text(row,"source")=="desktop_commander_connector")
                {
                    var at=Timestamp(row,"observedAtUtc");
                    if(at is not null&&at<=now.AddSeconds(30))
                    {
                        double? remaining=null;
                        if(row.TryGetProperty("remainingPercent",out var quota)&&quota.ValueKind==JsonValueKind.Number&&quota.TryGetDouble(out var value)&&double.IsFinite(value)&&value is >=0 and <=100)remaining=value;
                        result=result with{ObservedAtUtc=at,ObservationFresh=Fresh(at,now,TimeSpan.FromMinutes(5)),Online=Bool(row,"online"),Authenticated=Bool(row,"authenticated"),RemainingPercent=remaining};
                    }
                }
            }
        }
        using(var doc=ReadDocument("remote-supervisor-status.json"))
        {
            if(doc is not null&&doc.RootElement.ValueKind==JsonValueKind.Object)
            {
                var row=doc.RootElement;
                result=result with{SupervisorFresh=Bool(row,"Enabled")==true&&Bool(row,"Running")==true&&
                    row.TryGetProperty("RootCount",out var roots)&&roots.ValueKind==JsonValueKind.Number&&roots.TryGetInt32(out var count)&&count>0&&Fresh(Timestamp(row,"UpdatedUtc"),now,TimeSpan.FromSeconds(30))};
            }
        }
        return result;
    }
}
