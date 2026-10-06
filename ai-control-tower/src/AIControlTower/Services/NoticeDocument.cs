using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AIControlTower.Services;

/// <summary>Small native reader for the controlled announcement Markdown; no HTML/web runtime.</summary>
public static class NoticeDocument
{
    public static readonly DependencyProperty BaseDirectoryProperty = DependencyProperty.RegisterAttached("BaseDirectory", typeof(string), typeof(NoticeDocument), new PropertyMetadata("", Changed));
    public static void SetBaseDirectory(DependencyObject target, string value) => target.SetValue(BaseDirectoryProperty, value);
    public static string GetBaseDirectory(DependencyObject target) => (string)target.GetValue(BaseDirectoryProperty);
    public static readonly DependencyProperty HideMetadataProperty = DependencyProperty.RegisterAttached("HideMetadata",typeof(bool),typeof(NoticeDocument),new PropertyMetadata(true));
    public static void SetHideMetadata(DependencyObject target,bool value)=>target.SetValue(HideMetadataProperty,value);
    public static bool GetHideMetadata(DependencyObject target)=>(bool)target.GetValue(HideMetadataProperty);
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text",typeof(string),typeof(NoticeDocument),new PropertyMetadata("",Changed));
    public static void SetText(DependencyObject target,string value)=>target.SetValue(TextProperty,value);
    public static string GetText(DependencyObject target)=>(string)target.GetValue(TextProperty);
    private static void Changed(DependencyObject target,DependencyPropertyChangedEventArgs e)
    {
        if(target is not RichTextBox reader)return;
        var document=new FlowDocument { FontFamily=new FontFamily("Segoe UI, Malgun Gothic"),FontSize=14,PagePadding=new Thickness(0),ColumnWidth=double.PositiveInfinity,LineHeight=23 };
        document.SetResourceReference(FlowDocument.ForegroundProperty,"TextBrush");
        var lines=Regex.Replace(GetText(reader)??"", @"(?s)<!--.*?-->", "").Replace("\r","").Split('\n');
        var directory = GetBaseDirectory(reader);
        for(var i=0;i<lines.Length;i++)
        {
            var line=lines[i].Trim();
            if(line.Length==0)continue;
            if(GetHideMetadata(reader) && i==0 && line.StartsWith("# "))continue;
            if(GetHideMetadata(reader) && i<5 && line.StartsWith("버전 "))continue;
            if(line.StartsWith("```"))
            {
                var code = new System.Text.StringBuilder();
                i++;
                while(i<lines.Length && !lines[i].TrimStart().StartsWith("```")) { code.AppendLine(lines[i]); i++; }
                var block = new Paragraph(new Run(code.ToString())) { FontFamily=new FontFamily("Consolas, Malgun Gothic"),FontSize=12,Padding=new Thickness(10),Margin=new Thickness(0,8,0,14) };
                block.SetResourceReference(Paragraph.BackgroundProperty,"RaisedBrush");
                document.Blocks.Add(block); continue;
            }
            var imageMatch = Regex.Match(line, @"^!\[([^\]]*)\]\(([^)]+)\)$");
            if (imageMatch.Success && ProjectGuideService.ResolveResource(directory, imageMatch.Groups[2].Value, imageOnly: true) is { } imagePath)
            {
                try
                {
                    if (new FileInfo(imagePath).Length > 8 * 1024 * 1024) throw new InvalidDataException("이미지 크기 초과");
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit(); bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 1200; bitmap.UriSource = new Uri(imagePath); bitmap.EndInit(); bitmap.Freeze();
                    var picture = new Image { Source = bitmap, MaxWidth = 900, MaxHeight = 600, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Left };
                    System.Windows.Automation.AutomationProperties.SetName(picture, imageMatch.Groups[1].Value);
                    document.Blocks.Add(new BlockUIContainer(picture) { Margin = new Thickness(0, 8, 0, 16) });
                    continue;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or System.IO.FileFormatException or ArgumentException) { }
            }
            if(line.StartsWith('|'))
            {
                var rows=new System.Collections.Generic.List<string[]>();
                while(i<lines.Length && lines[i].Trim().StartsWith('|'))
                {
                    var cells=lines[i].Trim().Trim('|').Split('|').Select(c=>c.Trim()).ToArray();
                    if(!cells.All(c=>Regex.IsMatch(c,"^:?-+:?$")))rows.Add(cells);
                    i++;
                }
                i--;
                if(rows.Count==0)continue;
                var table=new Table { CellSpacing=0,Margin=new Thickness(0,8,0,18) };
                var columnCount=rows.Max(r=>r.Length);
                for(var col=0;col<columnCount;col++)table.Columns.Add(new TableColumn { Width=new GridLength(col==0?1:3,GridUnitType.Star) });
                var group=new TableRowGroup();table.RowGroups.Add(group);
                for(var row=0;row<rows.Count;row++)
                {
                    var tr=new TableRow();group.Rows.Add(tr);
                    for(var col=0;col<columnCount;col++)
                    {
                        var paragraph=new Paragraph { Margin=new Thickness(0),FontSize=13,FontWeight=row==0?FontWeights.SemiBold:FontWeights.Normal };
                        AddInline(paragraph,col<rows[row].Length?rows[row][col]:"",directory);
                        var cell=new TableCell(paragraph) { Padding=new Thickness(10,8,10,8),BorderThickness=new Thickness(0,0,0,1) };
                        cell.SetResourceReference(TableCell.BorderBrushProperty,"LineBrush");
                        cell.SetResourceReference(TableCell.BackgroundProperty,row==0?"RaisedBrush":"SurfaceBrush");
                        tr.Cells.Add(cell);
                    }
                }
                document.Blocks.Add(table);continue;
            }
            var heading=line.StartsWith("#");
            var paragraphBlock=new Paragraph { Margin=new Thickness(0,heading?15:0,0,heading?10:14),FontSize=line.StartsWith("# ")?24:heading?17:14,FontWeight=heading?FontWeights.SemiBold:FontWeights.Normal };
            if(heading)line=line.TrimStart('#').Trim();
            if(line.StartsWith("- "))line="— "+line[2..];
            AddInline(paragraphBlock,line,directory);document.Blocks.Add(paragraphBlock);
        }
        reader.Document=document;
    }
    private static void AddInline(Paragraph paragraph,string text,string directory)
    {
        var offset=0;
        foreach(Match match in Regex.Matches(text,@"\*\*([^*]+)\*\*|`([^`]+)`|\[([^\]]+)\]\(([^)]+)\)"))
        {
            paragraph.Inlines.Add(new Run(text[offset..match.Index]));
            if(match.Groups[1].Success)paragraph.Inlines.Add(new Run(match.Groups[1].Value) { FontWeight=FontWeights.SemiBold });
            else if(match.Groups[2].Success)
            {
                var run=new Run(match.Groups[2].Value);
                run.SetResourceReference(Run.ForegroundProperty,"AccentBrush");
                paragraph.Inlines.Add(run);
            }
            else if(Uri.TryCreate(match.Groups[4].Value,UriKind.Absolute,out var uri) && uri.Scheme=="https")
            {
                var link=new Hyperlink(new Run(match.Groups[3].Value)) { NavigateUri=uri };
                link.SetResourceReference(Hyperlink.ForegroundProperty,"AccentBrush");
                link.RequestNavigate+=(_,args)=> { OpenResource(args.Uri.AbsoluteUri);args.Handled=true; };
                paragraph.Inlines.Add(link);
            }
            else if (ProjectGuideService.ResolveResource(directory, match.Groups[4].Value) is { } resource)
            {
                var link = new Hyperlink(new Run(match.Groups[3].Value));
                link.SetResourceReference(Hyperlink.ForegroundProperty, "AccentBrush");
                link.Click += (_, _) => OpenResource(resource);
                paragraph.Inlines.Add(link);
            }
            else paragraph.Inlines.Add(new Run(match.Groups[3].Value + (directory.Length > 0 ? " (미확인 위치)" : "")));
            offset=match.Index+match.Length;
        }
        paragraph.Inlines.Add(new Run(text[offset..]));
    }
    private static void OpenResource(string target)
    {
        try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true })?.Dispose(); }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or IOException)
        { MessageBox.Show("자료를 열지 못했습니다.\n" + ProcessRunner.Sanitize(ex.Message), "자료 열기"); }
    }

}
