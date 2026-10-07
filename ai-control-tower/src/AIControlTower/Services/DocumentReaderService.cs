using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using AIControlTower.Models;

namespace AIControlTower.Services;

/// <summary>Reads one explicitly selected document within a caller-verified root. No shell/web execution.</summary>
public sealed class DocumentReaderService
{
    public const int MaximumBytes = 512 * 1024;
    public const int MaximumCharacters = 512 * 1024;
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".md", ".txt", ".json", ".log" };
    private static readonly Regex SensitiveName = new(@"(?:^|[._\-])(config|settings|credential[s]?|secret[s]?|cookie[s]?|token[s]?|private[._\-]?key)(?:$|[._\-])|^\.env(?:$|\.)|^id_(rsa|ed25519)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex SecretAssignment = new(@"(?im)[""']?(?:api[_\-]?key|access[_\-]?token|refresh[_\-]?token|password|passwd|authorization|client[_\-]?secret)[""']?\s*[:=]\s*[^\r\n]{4,}|-----BEGIN [A-Z ]*PRIVATE KEY-----|\b(?:sk-[a-zA-Z0-9_-]{16,}|gh[pousr]_[a-zA-Z0-9]{20,})", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    public async Task<ReaderDocument> ReadAsync(string approvedRoot, string filePath, CancellationToken cancellationToken = default)
    {
        var title = "문서";
        try
        {
            title=Path.GetFileName(filePath ?? "");
            var path = ValidatePath(approvedRoot, filePath ?? "");
            if (!Extensions.Contains(Path.GetExtension(path)) || SensitiveName.IsMatch(Path.GetFileName(path)))
                return Held(filePath, title, "설정·인증 자료 또는 실행 형식은 내부 문서 읽기 대상이 아닙니다.");
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.Asynchronous | FileOptions.SequentialScan);
            if (stream.Length > MaximumBytes) return Held(path,title,"문서가 512 KiB 읽기 한도를 넘습니다. 범위를 나눈 자료를 선택하세요.");
            using var reader = new StreamReader(stream,new UTF8Encoding(false,true),true,8192,leaveOpen:true);
            var text = new StringBuilder(); var buffer = new char[4096];
            while (true)
            {
                var count=await reader.ReadAsync(buffer.AsMemory(),cancellationToken); if(count==0) break;
                if(text.Length+count>MaximumCharacters) return Held(path,title,"문서 읽기 길이 한도를 넘습니다.");
                text.Append(buffer,0,count);
            }
            ValidatePath(approvedRoot,path); // Reject changed links/ancestors after the bounded read as well.
            var raw=text.ToString();
            if(raw.Contains('\0')) return Held(path,title,"바이너리 자료는 본문으로 표시하지 않습니다.");
            if(CommunicationService.ContainsSensitiveText(raw) || SecretAssignment.IsMatch(raw))
                return Held(path,title,"민감값이 의심되어 본문 표시를 보류했습니다. 정제된 안내·기록을 선택하세요.");
            // Preserve document lines (the process-log sanitizer truncates them). Strip terminal controls,
            // never render HTML or interpret ANSI. Secret detection remains defense in depth, not a guarantee.
            raw=Regex.Replace(raw,"\u001b\\[[0-?]*[ -/]*[@-~]","",RegexOptions.None,TimeSpan.FromMilliseconds(100));
            raw=new string(raw.Where(c=>!char.IsControl(c) || c is '\r' or '\n' or '\t').ToArray());
            return new(path,title,raw,Path.GetExtension(path).Equals(".md",StringComparison.OrdinalIgnoreCase)?"markdown":"text","");
        }
        catch(OperationCanceledException) { throw; }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or RegexMatchTimeoutException or DecoderFallbackException or System.Security.SecurityException)
        { return Held(filePath,title,e is FileNotFoundException or DirectoryNotFoundException ? "현재 파일을 찾을 수 없습니다. 안내 경로를 확인하세요." : "안전한 문서 읽기를 완료하지 못했습니다. 확인된 루트·파일 권한·인코딩을 확인하세요."); }
    }
    private static ReaderDocument Held(string? path,string title,string error) => new(path??"",title,"","text",error);
    public static string ValidatePath(string approvedRoot,string filePath)
    {
        if(!Path.IsPathFullyQualified(approvedRoot) || !Path.IsPathFullyQualified(filePath)) throw new ArgumentException("Absolute paths required.");
        var root=Path.TrimEndingDirectorySeparator(Path.GetFullPath(approvedRoot)); var path=Path.GetFullPath(filePath);
        if(root.Equals(Path.TrimEndingDirectorySeparator(Path.GetPathRoot(root)!),StringComparison.OrdinalIgnoreCase) || !Directory.Exists(root)) throw new ArgumentException("Explicit project root required.");
        var relative=Path.GetRelativePath(root,path);
        if(Path.IsPathRooted(relative) || relative==".." || relative.StartsWith(".."+Path.DirectorySeparatorChar) || relative.Contains(':') || relative.Split(Path.DirectorySeparatorChar).Any(p=>p.Equals(".git",StringComparison.OrdinalIgnoreCase) || p.Equals(".ssh",StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("Outside approved document root.");
        for(FileSystemInfo? current=new FileInfo(path);current is not null;current=current is FileInfo f?f.Directory:((DirectoryInfo)current).Parent)
            if((current.Attributes&FileAttributes.ReparsePoint)!=0) throw new IOException("Linked document paths are not supported.");
        return path;
    }

    public static FlowDocument RenderMarkdown(string text,double fontSize=16)
    {
        var doc=new FlowDocument { FontFamily=new FontFamily("Segoe UI, Malgun Gothic"),FontSize=fontSize,PagePadding=new Thickness(12),ColumnWidth=double.PositiveInfinity,LineHeight=fontSize*1.65 };
        doc.SetResourceReference(FlowDocument.ForegroundProperty,"TextBrush");
        var available=text[..Math.Min(text.Length,MaximumCharacters)].Replace("\r","").Split('\n');
        var previewLimited=text.Length>MaximumCharacters || available.Length>2000 || available.Any(l=>l.Length>4096);
        var lines=available.Take(2000).Select(l=>l[..Math.Min(l.Length,4096)]).ToArray();
        for(var i=0;i<lines.Length;i++)
        {
            var line=lines[i];
            if(line.TrimStart().StartsWith("```"))
            {
                var code=new StringBuilder(); while(++i<lines.Length && !lines[i].TrimStart().StartsWith("```")) code.AppendLine(lines[i]);
                var block=new Paragraph(new Run(code.ToString())) { FontFamily=new FontFamily("Consolas, Malgun Gothic"),FontSize=14,Padding=new Thickness(10),Margin=new Thickness(0,8,0,14) };
                block.SetResourceReference(Paragraph.BackgroundProperty,"RaisedBrush"); doc.Blocks.Add(block); continue;
            }
            if(line.Length==0) { doc.Blocks.Add(new Paragraph { Margin=new Thickness(0,0,0,7) }); continue; }
            var heading=Regex.Match(line,@"^(#{1,6})\s+(.+)$",RegexOptions.None,TimeSpan.FromMilliseconds(100));
            if(heading.Success) line=heading.Groups[2].Value;
            if(line.TrimStart().StartsWith("- ")) line="• "+line.TrimStart()[2..];
            // Links/images/HTML remain inert text. There is intentionally no URI, image loader, or RequestNavigate handler.
            var paragraph=new Paragraph { Margin=new Thickness(0,heading.Success?12:0,0,8),FontSize=heading.Success?fontSize+Math.Max(1,7-heading.Groups[1].Length):fontSize,FontWeight=heading.Success?FontWeights.SemiBold:FontWeights.Normal };
            var offset=0;
            foreach(Match match in Regex.Matches(line,@"\*\*([^*]+)\*\*|`([^`]+)`",RegexOptions.None,TimeSpan.FromMilliseconds(100)))
            {
                paragraph.Inlines.Add(new Run(line[offset..match.Index]));
                paragraph.Inlines.Add(match.Groups[1].Success?new Bold(new Run(match.Groups[1].Value)):new Run(match.Groups[2].Value) { FontFamily=new FontFamily("Consolas, Malgun Gothic") });
                offset=match.Index+match.Length;
            }
            paragraph.Inlines.Add(new Run(line[offset..])); doc.Blocks.Add(paragraph);
        }
        if(previewLimited) doc.Blocks.Add(new Paragraph(new Run("읽기 미리보기 길이를 제한했습니다. 전체 확인·검색에는 원문 보기를 사용하세요.")) { Margin=new Thickness(0,12,0,0) });
        return doc;
    }
}
