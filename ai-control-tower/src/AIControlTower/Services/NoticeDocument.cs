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
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text",typeof(string),typeof(NoticeDocument),new PropertyMetadata("",Changed));
    public static void SetText(DependencyObject target,string value)=>target.SetValue(TextProperty,value);
    public static string GetText(DependencyObject target)=>(string)target.GetValue(TextProperty);
    private static Brush Brush(string key)=>(Brush)Application.Current.FindResource(key);
    private static void Changed(DependencyObject target,DependencyPropertyChangedEventArgs e)
    {
        if(target is not RichTextBox reader)return;
        var document=new FlowDocument { FontFamily=new FontFamily("Segoe UI, Malgun Gothic"),FontSize=14,Foreground=Brush("TextBrush"),PagePadding=new Thickness(0),ColumnWidth=double.PositiveInfinity,LineHeight=23 };
        var lines=(e.NewValue as string??"").Replace("\r","").Split('\n');
        for(var i=0;i<lines.Length;i++)
        {
            var line=lines[i].Trim();
            if(line.Length==0)continue;
            if(i==0 && line.StartsWith("# "))continue;
            if(i<5 && line.StartsWith("버전 "))continue;
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
                        AddInline(paragraph,col<rows[row].Length?rows[row][col]:"");
                        tr.Cells.Add(new TableCell(paragraph) { Padding=new Thickness(10,8,10,8),BorderBrush=Brush("LineBrush"),BorderThickness=new Thickness(0,0,0,1),Background=row==0?Brush("RaisedBrush"):Brush("SurfaceBrush") });
                    }
                }
                document.Blocks.Add(table);continue;
            }
            var heading=line.StartsWith("##");
            var paragraphBlock=new Paragraph { Margin=new Thickness(0,heading?15:0,0,heading?10:14),FontSize=heading?16:14,FontWeight=heading?FontWeights.SemiBold:FontWeights.Normal };
            if(heading)line=line.TrimStart('#').Trim();
            if(line.StartsWith("- "))line="— "+line[2..];
            AddInline(paragraphBlock,line);document.Blocks.Add(paragraphBlock);
        }
        reader.Document=document;
    }
    private static void AddInline(Paragraph paragraph,string text)
    {
        var offset=0;
        foreach(Match match in Regex.Matches(text,@"\*\*([^*]+)\*\*|`([^`]+)`|\[([^\]]+)\]\(([^)]+)\)"))
        {
            paragraph.Inlines.Add(new Run(text[offset..match.Index]));
            if(match.Groups[1].Success)paragraph.Inlines.Add(new Run(match.Groups[1].Value) { FontWeight=FontWeights.SemiBold });
            else if(match.Groups[2].Success)paragraph.Inlines.Add(new Run(match.Groups[2].Value) { Foreground=Brush("AccentBrush") });
            else if(Uri.TryCreate(match.Groups[4].Value,UriKind.Absolute,out var uri) && uri.Scheme=="https")
            {
                var link=new Hyperlink(new Run(match.Groups[3].Value)) { NavigateUri=uri,Foreground=Brush("AccentBrush") };
                link.RequestNavigate+=(_,args)=> { Process.Start(new ProcessStartInfo(args.Uri.AbsoluteUri) { UseShellExecute=true })?.Dispose();args.Handled=true; };
                paragraph.Inlines.Add(link);
            }
            else paragraph.Inlines.Add(new Run(match.Groups[3].Value));
            offset=match.Index+match.Length;
        }
        paragraph.Inlines.Add(new Run(text[offset..]));
    }
}
