using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIControlTower.Models;

namespace AIControlTower.Services;

/// <summary>Reported batch progress, never overall project completion or process liveness.</summary>
public sealed record ProjectProgressSnapshot(string ProjectId,string ProjectName,DateTimeOffset ObservedAtUtc)
{
    public string ProjectRoot { get; init; } = "";
    public int? Completed { get; init; }
    public int? Total { get; init; }
    public int? Held { get; init; }
    public int? CurrentChapter { get; init; }
    public DateTimeOffset? SourceUpdatedAtUtc { get; init; }
    public DateTimeOffset? WorkerUpdatedAtUtc { get; init; }
    public string WorkerId { get; init; } = "";
    public string Stage { get; init; } = "unknown";
    public string ReasonCode { get; init; } = "source_unlinked";
    public bool IsKnown=>Completed.HasValue&&Total is >0;
    public bool IsFresh=>IsKnown&&SourceUpdatedAtUtc is {} at&&at<=ObservedAtUtc&&ObservedAtUtc-at<=TimeSpan.FromMinutes(2);
    public double? Percent=>IsKnown?Math.Round(Completed!.Value*100.0/Total!.Value,1):null;
    public double ProgressValue=>Percent??0;
    public string ScopeText=>IsKnown?"이번 제작 범위 · 완료 회차 기준":"제작 범위 진행 자료 미연결";
    public string OverallText=>"전체 프로젝트 완료율 미확인 · 작업 기록 완료와 구분";
    public string Headline=>Percent is {} p?(IsFresh?"최근 제작 기록 ":"마지막 기록 ")+p.ToString("0.#",CultureInfo.InvariantCulture)+"%":"진행률 미확인";
    public string CountsText=>IsKnown?$"{Completed}/{Total}회차 · 검토 보류 {Held}회차":"완료/전체 수량 미확인";
    public string ChapterText=>CurrentChapter is {} chapter?$"기록된 회차: {chapter}회차":"회차 미확인";
    public string StageText=>Stage switch {"generating"=>"음성 제작 기록","complete"=>"제작 종료 기록","paused"=>"일시정지 기록","error" or "failed"=>"오류 기록","preparing"=>"준비 기록",_=>"단계 미확인"};
    public string FreshnessText=>!IsKnown?"연결된 상태 자료를 확인해야 합니다.":IsFresh?"최근 원본 상태 기록 · 현재 실행 생존은 별도 확인":"오래된 원본 기록 · 현재 실행 확인 필요";
    public string SourceTimeText=>SourceUpdatedAtUtc is {} at?"원본 갱신 "+at.ToLocalTime().ToString("MM-dd HH:mm:ss zzz",CultureInfo.InvariantCulture)+" · UTC "+at.ToString("O",CultureInfo.InvariantCulture):"원본 갱신 시각 미확인";
    public string ObservationTimeText=>"조회 "+ObservedAtUtc.ToLocalTime().ToString("MM-dd HH:mm:ss zzz",CultureInfo.InvariantCulture)+" · 조회 시각은 제작 갱신이 아닙니다.";
    public string WorkerText=>WorkerUpdatedAtUtc is {} at?$"발화 진행 기록 {WorkerId} · {at.ToLocalTime():MM-dd HH:mm:ss} · 현재 실행 확인과 별개":"회차 내부 처리량·남은 시간 미확인";
}

/// <summary>Only a registered project's canonical metadata. No writes, shell, HTTP, audio or producer calls.</summary>
public sealed class ProjectProgressService
{
    public static ProjectProgressSnapshot Unknown(ProjectItem? project,DateTimeOffset now,string reason="source_unlinked")
        =>new(project?.Id??"",WorkDashboardService.SafeText(project?.DisplayName??"프로젝트를 선택하세요"),now.ToUniversalTime()){ProjectRoot=project?.Path??"",ReasonCode=reason};
    public ProjectProgressSnapshot Read(ProjectItem project,CatalogDefinition catalog,DateTimeOffset now)
    {
        now=now.ToUniversalTime();var empty=Unknown(project,now);
        try
        {
            // Names, aliases, discovered folders and completed task counts cannot register a producer.
            if(project.Id!="audiobook"||catalog.SchemaVersion!=1)return empty;
            var root=DirectoryPath(project.Path);
            if(catalog.Projects.Take(200).Count(p=>p.Id==project.Id&&DirectoryPath(p.Path).Equals(root,StringComparison.OrdinalIgnoreCase))!=1)return empty;
            var pointerPath=Inside(root,"output/production_active.json");
            using var pointer=Json(pointerPath,8192);var active=pointer.RootElement;Unique(active);
            var edition=Text(active,"edition");var relative=Text(active,"run_relative");
            if(!Regex.IsMatch(edition,@"\Aproduction_[A-Za-z0-9_-]{1,100}\z",RegexOptions.CultureInvariant)||relative!="output/"+edition)throw new InvalidDataException();
            var run=Inside(root,relative);
            using var document=Json(Inside(root,relative+"/status.json"),1024*1024);var state=document.RootElement;Unique(state);
            if(Text(state,"edition")!=edition)throw new InvalidDataException();
            var total=Number(state,"total",1,10000);var completed=Number(state,"completed",0,total);
            var sourceTime=Epoch(state,"updated_at",now);
            if(!state.TryGetProperty("chapters",out var chapters)||chapters.ValueKind!=JsonValueKind.Object)throw new InvalidDataException();
            Unique(chapters);var ready=0;var held=0;var count=0;
            foreach(var chapter in chapters.EnumerateObject())
            {
                if(++count>total||count>10000)throw new InvalidDataException();Unique(chapter.Value);
                var status=Text(chapter.Value,"status");if(status=="ready")ready++;
                if(status is "needs_review" or "failed" or "blocked" or "quality_hold")held++;
            }
            if(ready!=completed)throw new InvalidDataException();
            int? current=null;if(state.TryGetProperty("current",out var c)&&c.ValueKind!=JsonValueKind.Null)current=Number(state,"current",1,10000000);
            var result=empty with {Completed=completed,Total=total,Held=held,CurrentChapter=current,SourceUpdatedAtUtc=sourceTime,Stage=Text(state,"stage"),ReasonCode="batch_record"};
            try
            {
                using var progress=Json(Inside(root,relative+"/worker_progress.json"),65536);var worker=progress.RootElement;Unique(worker);
                var at=Epoch(worker,"updated",now);var id=Text(worker,"id");
                if(current.HasValue&&Number(worker,"chapter",1,10000000)==current&&at>=sourceTime&&Regex.IsMatch(id,@"\A[0-9]{1,7}-[A-Za-z0-9_-]{1,40}\z",RegexOptions.CultureInvariant)&&id.StartsWith(current.Value.ToString(CultureInfo.InvariantCulture)+"-",StringComparison.Ordinal))
                    result=result with {WorkerId=id,WorkerUpdatedAtUtc=at};
            }
            catch(Exception ex) when(ReadError(ex)) { /* Optional worker metadata never changes batch completion. */ }
            // Do not combine files from different editions when the producer atomically changes its pointer.
            using var finalPointer=Json(pointerPath,8192);Unique(finalPointer.RootElement);
            if(Text(finalPointer.RootElement,"edition")!=edition||Text(finalPointer.RootElement,"run_relative")!=relative)throw new InvalidDataException();
            _=run;return result;
        }
        catch(Exception ex) when(ReadError(ex)){return empty with {ReasonCode="source_unavailable"};}
    }
    private static bool ReadError(Exception ex)=>ex is IOException or InvalidDataException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException or NotSupportedException or OverflowException;
    private static string DirectoryPath(string path)
    {
        if(!Path.IsPathFullyQualified(path)||path.StartsWith(@"\\",StringComparison.Ordinal))throw new InvalidDataException();
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
    }
    private static string Inside(string root,string relative)
    {
        if(Path.IsPathRooted(relative))throw new InvalidDataException();
        var path=Path.GetFullPath(Path.Combine(root,relative));
        if(!path.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException();
        NoReparse(path);return path;
    }
    private static void NoReparse(string path)
    {
        for(var p=path;!string.IsNullOrEmpty(p);p=Path.GetDirectoryName(p))
            if((File.Exists(p)||Directory.Exists(p))&&(File.GetAttributes(p)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException();
    }
    private static JsonDocument Json(string path,int maximum)
    {
        NoReparse(path);
        // Short independent reads close handles before producers replace files. No cache/metadata writes.
        using var input=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
        if(input.Length>maximum)throw new InvalidDataException();using var bytes=new MemoryStream();var buffer=new byte[4096];int length;
        while((length=input.Read(buffer))>0){if(bytes.Length+length>maximum)throw new InvalidDataException();bytes.Write(buffer,0,length);}
        return JsonDocument.Parse(bytes.ToArray(),new JsonDocumentOptions{MaxDepth=16});
    }
    private static void Unique(JsonElement row)
    {
        if(row.ValueKind!=JsonValueKind.Object)throw new InvalidDataException();var names=new HashSet<string>(StringComparer.Ordinal);
        foreach(var property in row.EnumerateObject())if(!names.Add(property.Name))throw new InvalidDataException();
    }
    private static string Text(JsonElement row,string key)
        =>row.TryGetProperty(key,out var value)&&value.ValueKind==JsonValueKind.String?value.GetString()??"":throw new InvalidDataException();
    private static int Number(JsonElement row,string key,int minimum,int maximum)
        =>row.TryGetProperty(key,out var value)&&value.ValueKind==JsonValueKind.Number&&value.TryGetInt32(out var count)&&count>=minimum&&count<=maximum?count:throw new InvalidDataException();
    private static DateTimeOffset Epoch(JsonElement row,string key,DateTimeOffset now)
    {
        if(!row.TryGetProperty(key,out var value)||value.ValueKind!=JsonValueKind.Number||!value.TryGetDouble(out var seconds)||!double.IsFinite(seconds)||seconds<0||seconds>253402300799)throw new InvalidDataException();
        var at=DateTimeOffset.FromUnixTimeMilliseconds(checked((long)(seconds*1000)));
        if(at>now.AddMinutes(1))throw new InvalidDataException();return at;
    }
}
