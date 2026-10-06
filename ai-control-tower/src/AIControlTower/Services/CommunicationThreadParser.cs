using System.Text.RegularExpressions;
using System.Text.Json;

namespace AIControlTower.Services;

public sealed record CommunicationEntry(string Identity, string Title, string Author, string TimeDisplay,
    string Body, string ContentHash, bool IsComment = false)
{
    public string KindLabel => IsComment ? "댓글" : "게시글";
}

/// <summary>Splits only explicit dated/attributed entries, never ordinary article headings.</summary>
public static class CommunicationThreadParser
{
    private static readonly Regex Heading = new(@"(?m)^#{2,3}\s+(?<heading>[^\r\n]+)$", RegexOptions.CultureInvariant);
    private static readonly Regex Date = new(@"\b\d{4}[-/.]\d{2}[-/.]\d{2}(?:[ T]\d{2}:\d{2}(?::\d{2})?(?:[+-]\d{2}:\d{2}|Z)?)?", RegexOptions.CultureInvariant);
    private static readonly Regex Author = new(@"(?im)^\s*(?:[-*]\s*)?(?:\*\*)?(?:작성자|담당|author|actor(?:Id)?)(?:\*\*)?\s*[:：]\s*(?<author>[^\r\n]+)$", RegexOptions.CultureInvariant);
    public static IReadOnlyList<CommunicationEntry> Parse(string body, string identity, string title, string fallbackAuthor = "", string fallbackTime = "")
    {
        var fenceLines = new HashSet<int>();
        var position=0;
        char fence='\0';
        foreach(var line in body.Split('\n'))
        {
            var trimmed=line.TrimStart();
            if(trimmed.StartsWith("```") || trimmed.StartsWith("~~~"))
            {
                if(fence=='\0') fence=trimmed[0];
                else if(fence==trimmed[0]) fence='\0';
                fenceLines.Add(position);
            }
            else if(fence!='\0') fenceLines.Add(position);
            position+=line.Length+1;
        }
        var headings = Heading.Matches(body).Cast<Match>().Where(m => !fenceLines.Contains(m.Index) && Date.Match(m.Groups["heading"].Value) is { Success:true, Index:0 }).ToArray();
        if (headings.Length == 0)
        {
            var author=Author.Match(body).Groups["author"].Value.Trim().Trim('*','`').Split(" · ")[0];
            if(body.TrimStart().StartsWith('{'))
            {
                try
                {
                    using var json=JsonDocument.Parse(body);
                    foreach(var key in new[]{"actorId","author","actor"}) if(json.RootElement.TryGetProperty(key,out var value) && value.ValueKind==JsonValueKind.String) { author=value.GetString()??""; break; }
                    if(json.RootElement.TryGetProperty("updatedAt",out var updated) && updated.ValueKind==JsonValueKind.String) fallbackTime=updated.GetString()??fallbackTime;
                }
                catch(JsonException) { }
            }
            if(author.Length==0) author=fallbackAuthor;
            return [new(identity+"/post",title,author.Length==0 ? "작성자 정보 없음" : author,fallbackTime,CommunicationArticle.Read(body,null),CommunicationService.ContentHash(body))];
        }
        var entries = new List<CommunicationEntry>();
        var occurrences = new Dictionary<string,int>(StringComparer.Ordinal);
        for (var i = 0; i < headings.Length; i++)
        {
            var match = headings[i];
            var heading = match.Groups["heading"].Value.Trim();
            var text = body[match.Index..(i + 1 < headings.Length ? headings[i + 1].Index : body.Length)].Trim();
            var date = Date.Match(heading).Value + (heading.Contains("KST",StringComparison.Ordinal) ? " KST" : "");
            var parts = heading.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var author = Author.Match(text).Groups["author"].Value.Trim().Trim('*', '`').Split(" · ")[0];
            if (author.Length == 0 && parts.Length >= 2) author = parts.FirstOrDefault(p => !Date.IsMatch(p)) ?? "";
            if (author.Length == 0) author = fallbackAuthor;
            var entryTitle = parts.Length >= 3 ? string.Join(" · ", parts.Skip(2)) : Regex.Replace(heading, @"^\d{4}[-/.]\d{2}[-/.]\d{2}(?:[ T]\d{2}:\d{2}(?::\d{2})?)?\s*(?:KST|UTC|Z)?\s*[—–·|-]?\s*", "");
            var key = CommunicationService.ContentHash(heading);
            var count = occurrences.GetValueOrDefault(key);
            occurrences[key] = count + 1;
            // Revisions of the same file keep an entry identity; only changed entry bytes become unread.
            var comment = Regex.IsMatch(heading, @"(?:^|\s|\|)(?:댓글|reply|comment)(?:\s|\||:|$)", RegexOptions.IgnoreCase);
            entries.Add(Create(identity + "/" + key + "/" + count, entryTitle, author, date, text, comment));
        }
        return entries;
    }
    private static CommunicationEntry Create(string id,string title,string author,string time,string body,bool comment) =>
        new(id,title,author.Length == 0 ? "작성자 정보 없음" : author,time,body,CommunicationService.ContentHash(body),comment);
}
