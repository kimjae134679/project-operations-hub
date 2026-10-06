using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIControlTower.Services;

/// <summary>Human projection of legacy JSON; source bytes remain unchanged for AI readers.</summary>
public static class CommunicationArticle
{
    private static readonly HashSet<string> Metadata = new(StringComparer.OrdinalIgnoreCase)
    { "schemaVersion","recordType","contentSha256","recordId","projectId","sessionId","source","centralPath","collectedAt" };
    private static readonly Dictionary<string,string> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["title"]="제목", ["summary"]="요약", ["details"]="자세한 내용", ["request"]="받은 요청",
        ["response"]="답변", ["reply"]="답변", ["workDone"]="한 일", ["nextActions"]="다음 할 일",
        ["nextSteps"]="다음 할 일", ["blockers"]="막힌 이유", ["verification"]="확인 결과",
        ["evidence"]="근거", ["status"]="자료에 적힌 상태", ["message"]="내용", ["note"]="메모",
        ["notes"]="메모", ["result"]="결과", ["results"]="결과", ["outputs"]="생성 결과",
        ["files"]="파일", ["paths"]="위치", ["path"]="위치", ["author"]="작성자", ["actorId"]="작성자",
        ["updatedAt"]="기록 시각", ["receivedAt"]="요청 시각", ["checkedAt"]="확인 시각",
        ["name"]="이름", ["actions"]="작업", ["changes"]="바뀐 점", ["completed"]="완료 내역",
        ["remaining"]="남은 일", ["handoff"]="인수인계", ["items"]="항목", ["tasks"]="작업",
        ["health"]="상태", ["warnings"]="주의", ["errors"]="문제", ["description"]="설명"
    };
    public static string Read(string body, TaskExchangeView? exchange)
    {
        if (exchange is not null)
        {
            var article = exchange.Markdown.Split("## 기록 출처",StringSplitOptions.None)[0];
            return Regex.Replace(article,@"(?m)^\s*근거:.*(?:\r?\n|$)","");
        }
        if (!body.TrimStart().StartsWith('{') && !body.TrimStart().StartsWith('[')) return body;
        try
        {
            using var doc=JsonDocument.Parse(body,new JsonDocumentOptions { MaxDepth=32 });
            var output=new StringBuilder();
            var budget=250;
            Append(output,doc.RootElement,0,ref budget);
            if(budget<=0) output.AppendLine("\n나머지 항목은 아래 원본에서 확인할 수 있습니다.");
            return output.Length==0 ? "본문으로 표시할 내용이 없습니다. 아래 원본에서 전체 기록을 확인하세요." : output.ToString();
        }
        catch(JsonException) { return "본문 형식을 읽지 못했습니다. 아래 원본에서 전체 기록을 확인하세요.\n\n\u0060\u0060\u0060json\n"+body+"\n\u0060\u0060\u0060"; }
    }
    private static void Append(StringBuilder output,JsonElement value,int depth,ref int budget)
    {
        if(budget<=0 || depth>16) return;
        if(value.ValueKind==JsonValueKind.Object)
        {
            foreach(var field in value.EnumerateObject())
            {
                if(Metadata.Contains(field.Name) || budget--<=0) continue;
                var name=Labels.GetValueOrDefault(field.Name,Regex.Replace(field.Name,@"[_-]"," "));
                if(field.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                {
                    if(field.Value.ValueKind==JsonValueKind.Array && field.Value.GetArrayLength()==0) continue;
                    output.AppendLine().AppendLine("## "+name);
                    Append(output,field.Value,depth+1,ref budget);
                }
                else
                {
                    var text=Scalar(field.Value);
                    if(text.Length>0) output.AppendLine("**"+name+"** · "+text).AppendLine();
                }
            }
        }
        else if(value.ValueKind==JsonValueKind.Array)
        {
            foreach(var item in value.EnumerateArray())
            {
                if(budget--<=0) break;
                if(item.ValueKind is JsonValueKind.Object or JsonValueKind.Array) { Append(output,item,depth+1,ref budget); output.AppendLine(); }
                else output.AppendLine("- "+Scalar(item));
            }
        }
        else output.AppendLine(Scalar(value));
    }
    private static string Scalar(JsonElement value) => value.ValueKind switch
    { JsonValueKind.String=>value.GetString()??"", JsonValueKind.Null=>"",JsonValueKind.True=>"예",JsonValueKind.False=>"아니요",_=>value.ToString() };
}
