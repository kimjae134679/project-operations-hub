using System.Text.RegularExpressions;
using System.Text.Json;

namespace AIControlTower.Services;

public sealed record CommunicationEntry(string Identity, string Title, string Author, string TimeDisplay,
    string Body, string ContentHash, bool IsComment = false)
{
    public string RecordKind { get; init; } = "post";
    public string SourceName { get; init; } = "";
    public string SourcePath { get; init; } = "";
    public string KindLabel => IsComment ? "댓글" : RecordKind switch { "guide" => "주제 안내", "record" => "답변·기록", "unknown" => "종류 미확인", _ => "게시글" };
}

/// <summary>Read-only projection of explicit dated posts, merged original records and standalone sources.</summary>
public static class CommunicationThreadParser
{
    private static readonly Regex Heading = new(@"(?m)^(?<level>#{2,3})\s+(?<heading>[^\r\n]+)$", RegexOptions.CultureInvariant);
    private static readonly Regex Original = new(@"^원본 기록:\s*(?<source>.+)$", RegexOptions.CultureInvariant);
    private static readonly Regex Date = new(@"\b\d{4}[-/.]\d{2}[-/.]\d{2}(?:[ T]\d{2}:\d{2}(?::\d{2})?(?:[+-]\d{2}:\d{2}|Z)?)?", RegexOptions.CultureInvariant);
    private static readonly Regex Author = new(@"(?im)^\s*(?:[-*]\s*)?(?:\*\*)?(?:작성자|담당|author|actor(?:Id)?)(?:\*\*)?\s*[:：]\s*(?<author>[^\r\n]+)$", RegexOptions.CultureInvariant);
    private static readonly Regex MetadataDate = new(@"(?im)^\s*(?:[-*]\s*)?(?:Date|Updated|작성 시각|작성일|시각)\s*[:：]\s*(?<time>[^\r\n]+)$", RegexOptions.CultureInvariant);
    private static readonly Regex Comment = new(@"(?:^|\s|\|)(?:댓글|reply|comment)(?:\s|\||:|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private sealed record Boundary(Match Match,string Source,bool Archived);

    public static IReadOnlyList<CommunicationEntry> Parse(string body, string identity, string title, string fallbackAuthor = "", string fallbackTime = "",
        string sourceName = "", string sourcePath = "", bool standaloneRecord = false)
    {
        var fenceLines = new HashSet<int>();
        var position=0; char fence='\0';
        foreach(var line in body.Split('\n'))
        {
            var trimmed=line.TrimStart();
            if(trimmed.StartsWith("```") || trimmed.StartsWith("~~~"))
            {
                if(fence=='\0') fence=trimmed[0]; else if(fence==trimmed[0]) fence='\0';
                fenceLines.Add(position);
            }
            else if(fence!='\0') fenceLines.Add(position);
            position+=line.Length+1;
        }
        var boundaries=new List<Boundary>();
        var archived=false;
        foreach(var match in Heading.Matches(body).Cast<Match>().Where(m=>!fenceLines.Contains(m.Index)))
        {
            var heading=match.Groups["heading"].Value;
            var original=Original.Match(heading);
            if(match.Groups["level"].Length==2 && original.Success)
            {
                archived=true; boundaries.Add(new(match,original.Groups["source"].Value.Trim(),true));
            }
            else if(Date.Match(heading) is {Success:true,Index:0} && (!archived || match.Groups["level"].Length==2))
            {
                archived=false; boundaries.Add(new(match,sourceName,false));
            }
        }
        if(boundaries.Count==0)
        {
            var author=ReadAuthor(body); var time=fallbackTime;
            if(body.TrimStart().StartsWith('{'))
            {
                try
                {
                    using var json=JsonDocument.Parse(body);
                    foreach(var key in new[]{"actorId","author","actor"}) if(json.RootElement.TryGetProperty(key,out var value) && value.ValueKind==JsonValueKind.String) {author=value.GetString()??"";break;}
                    if(json.RootElement.TryGetProperty("updatedAt",out var updated) && updated.ValueKind==JsonValueKind.String)time=updated.GetString()??time;
                }
                catch(JsonException) { }
            }
            if(author.Length==0)author=standaloneRecord?"작성자 미확인":fallbackAuthor;
            if(standaloneRecord)time=ReadTime(body);
            return [new(identity+"/post",title,author.Length==0?"작성자 정보 없음":author,time,CommunicationArticle.Read(body,null),CommunicationService.ContentHash(body))
                {SourceName=sourceName,SourcePath=sourcePath,RecordKind=standaloneRecord?IsGuide(sourceName)?"guide":"record":"post"}];
        }
        var entries=new List<CommunicationEntry>();
        var occurrences=new Dictionary<string,int>(StringComparer.Ordinal);
        var preamble=body[..boundaries[0].Match.Index].Trim();
        // A title/transport banner alone is not another post. Actual introductory content remains readable.
        if(preamble.Split('\n').Any(l=>!string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith('#') && !l.TrimStart().StartsWith('>') && l.Trim()!="---"))
            entries.Add(new(identity+"/introduction",title+" · 주제 안내","작성자 미확인","시각 미확인",preamble,CommunicationService.ContentHash(preamble)) {SourceName=sourceName,SourcePath=sourcePath,RecordKind="guide"});
        for(var i=0;i<boundaries.Count;i++)
        {
            var boundary=boundaries[i];var match=boundary.Match;
            var heading=match.Groups["heading"].Value.Trim();
            var text=body[match.Index..(i+1<boundaries.Count?boundaries[i+1].Match.Index:body.Length)].Trim();
            var author=ReadAuthor(text);var date="";var entryTitle="";var kind="post";
            var comment=false;
            if(boundary.Archived)
            {
                var sourceTitle=text.Split('\n').Skip(1).FirstOrDefault(l=>l.StartsWith("### "))?.TrimStart('#',' ')??boundary.Source;
                entryTitle=sourceTitle;
                // Metadata is evidence. A filename containing 'sol' is not evidence of who wrote this record.
                if(author.Length==0)author="작성자 미확인";
                date=ReadTime(text);
                kind=IsGuide(boundary.Source)?"guide":"record";
                comment=Comment.IsMatch(sourceTitle);
            }
            else
            {
                date=Date.Match(heading).Value+(heading.Contains("KST",StringComparison.Ordinal)?" KST":"");
                var parts=heading.Split('|',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries);
                if(author.Length==0 && parts.Length>=2)author=parts.FirstOrDefault(p=>!Date.IsMatch(p))??"";
                if(author.Length==0)author=fallbackAuthor;
                entryTitle=parts.Length>=3?string.Join(" · ",parts.Skip(2)):Regex.Replace(heading,@"^\d{4}[-/.]\d{2}[-/.]\d{2}(?:[ T]\d{2}:\d{2}(?::\d{2})?)?\s*(?:KST|UTC|Z)?\s*[—–·|-]?\s*","");
                comment=Comment.IsMatch(heading);
            }
            // Stable selection identity is source/heading based; changed bytes independently become unread.
            var key=CommunicationService.ContentHash(boundary.Archived?"original/"+boundary.Source:heading);
            var count=occurrences.GetValueOrDefault(key);occurrences[key]=count+1;
            var path=boundary.Archived?sourcePath+"#original="+Uri.EscapeDataString(boundary.Source)+"&occurrence="+count:sourcePath;
            entries.Add(new(identity+"/"+key+"/"+count,entryTitle,author.Length==0?"작성자 정보 없음":author,date,text,CommunicationService.ContentHash(text),comment)
                {SourceName=boundary.Source,SourcePath=path,RecordKind=kind});
        }
        return entries;
    }
    private static string ReadAuthor(string text)=>Author.Match(text).Groups["author"].Value.Trim().Trim('*','`').Split(" · ")[0];
    private static string ReadTime(string text)
    {
        var metadata=MetadataDate.Match(text).Groups["time"].Value.Trim();
        if(metadata.Length>0 && Date.IsMatch(metadata))return metadata;
        var standalone=text.Split('\n').Select(l=>l.Trim()).FirstOrDefault(l=>Date.Match(l) is {Success:true,Index:0});
        return standalone??"시각 미확인";
    }
    private static bool IsGuide(string source)=>source.Replace('\\','/').Split('/').Last().Equals("README.md",StringComparison.OrdinalIgnoreCase);
}
