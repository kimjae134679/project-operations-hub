using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AIControlTower.Services;

/// <summary>Small native reader for the controlled announcement Markdown; no HTML/web runtime.</summary>
public static class NoticeDocument
{
    public static readonly DependencyProperty BaseDirectoryProperty=DependencyProperty.RegisterAttached("BaseDirectory",typeof(string),typeof(NoticeDocument),new PropertyMetadata(""));
    public static void SetBaseDirectory(DependencyObject target,string value)=>target.SetValue(BaseDirectoryProperty,value);
    public static string GetBaseDirectory(DependencyObject target)=>(string)target.GetValue(BaseDirectoryProperty);
    public static readonly DependencyProperty HideMetadataProperty = DependencyProperty.RegisterAttached("HideMetadata",typeof(bool),typeof(NoticeDocument),new PropertyMetadata(true));
    public static void SetHideMetadata(DependencyObject target,bool value)=>target.SetValue(HideMetadataProperty,value);
    public static bool GetHideMetadata(DependencyObject target)=>(bool)target.GetValue(HideMetadataProperty);
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text",typeof(string),typeof(NoticeDocument),new PropertyMetadata("",Changed));
    public static void SetText(DependencyObject target,string value)=>target.SetValue(TextProperty,value);
    public static string GetText(DependencyObject target)=>(string)target.GetValue(TextProperty);
    public static readonly DependencyProperty AutoHeightProperty = DependencyProperty.RegisterAttached("AutoHeight",typeof(bool),typeof(NoticeDocument),new PropertyMetadata(false,AutoHeightChanged));
    public static void SetAutoHeight(DependencyObject target,bool value)=>target.SetValue(AutoHeightProperty,value);
    public static bool GetAutoHeight(DependencyObject target)=>(bool)target.GetValue(AutoHeightProperty);
    private sealed class HeightState { public bool Queued; }
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<RichTextBox,HeightState> Heights = new();
    private static void AutoHeightChanged(DependencyObject target,DependencyPropertyChangedEventArgs e)
    {
        if(target is not RichTextBox reader)return;
        if((bool)e.NewValue)
        {
            reader.Loaded+=ReaderLoaded;
            reader.SizeChanged+=ReaderSizeChanged;
            QueueHeight(reader);
        }
        else
        {
            reader.Loaded-=ReaderLoaded;
            reader.SizeChanged-=ReaderSizeChanged;
            reader.ClearValue(FrameworkElement.HeightProperty);
        }
    }
    private static void ReaderLoaded(object sender,RoutedEventArgs e)=>QueueHeight((RichTextBox)sender);
    private static void ReaderSizeChanged(object sender,SizeChangedEventArgs e)
    {
        // Height changes are our own result. Only width changes require another reflow.
        if(e.WidthChanged)QueueHeight((RichTextBox)sender);
    }
    private static void QueueHeight(RichTextBox reader)
    {
        if(!GetAutoHeight(reader))return;
        var state=Heights.GetValue(reader,_=>new HeightState());
        if(state.Queued)return;
        state.Queued=true;
        reader.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,new Action(()=>
        {
            state.Queued=false;
            if(!GetAutoHeight(reader) || !reader.IsLoaded || reader.ActualWidth<=0)return;
            // RichTextBox's first desired size can be one line when its document was assigned
            // while the tab was hidden. Measure at the final width before using text geometry.
            reader.SetCurrentValue(FrameworkElement.HeightProperty,double.NaN);
            reader.Measure(new Size(reader.ActualWidth,double.PositiveInfinity));
            reader.UpdateLayout();
            reader.ScrollToVerticalOffset(0);
            var end=reader.Document.ContentEnd.GetCharacterRect(LogicalDirection.Backward);
            var lastMargin=reader.Document.Blocks.LastBlock?.Margin.Bottom ?? 0;
            var bottom=end.IsEmpty ? 0 : end.Bottom+lastMargin+reader.Padding.Bottom+reader.BorderThickness.Bottom;
            var height=Math.Ceiling(Math.Max(reader.DesiredSize.Height,Math.Max(reader.ExtentHeight,bottom)));
            if(double.IsFinite(height) && height>0)reader.SetCurrentValue(FrameworkElement.HeightProperty,height);
        }));
    }
    private static void Changed(DependencyObject target,DependencyPropertyChangedEventArgs e)
    {
        if(target is not RichTextBox reader)return;
        var document=new FlowDocument { FontFamily=new FontFamily("Segoe UI, Malgun Gothic"),FontSize=reader.FontSize,PagePadding=new Thickness(0),ColumnWidth=double.PositiveInfinity,LineHeight=reader.FontSize*1.75 };
        document.SetResourceReference(FlowDocument.ForegroundProperty,"TextBrush");
        var lines=Regex.Replace(e.NewValue as string??"", @"(?s)<!--.*?-->", "").Replace("\r","").Split('\n');
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
            var picture=Regex.Match(line,@"^!\[([^\]]*)\]\(([^)]+)\)$");
            if(picture.Success && GetBaseDirectory(reader).Length>0)
            {
                try
                {
                    var source=Path.GetFullPath(picture.Groups[2].Value,GetBaseDirectory(reader));
                    if(File.Exists(source) && new FileInfo(source).Length<16*1024*1024)
                    {
                        var bitmap=new BitmapImage();
                        bitmap.BeginInit(); bitmap.CacheOption=BitmapCacheOption.OnLoad; bitmap.DecodePixelWidth=800;
                        bitmap.UriSource=new Uri(source,UriKind.Absolute); bitmap.EndInit(); bitmap.Freeze();
                        var image=new Image { Source=bitmap,MaxWidth=600,Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Left };
                        document.Blocks.Add(new BlockUIContainer(image) { Margin=new Thickness(0,12,0,12) });
                        if(picture.Groups[1].Value.Length>0) document.Blocks.Add(new Paragraph(new Run(picture.Groups[1].Value)));
                    }
                    else document.Blocks.Add(new Paragraph(new Run("이미지 위치 확인 필요 · "+picture.Groups[1].Value)));
                }
                catch(Exception ex) when(ex is IOException or ArgumentException or NotSupportedException or UnauthorizedAccessException or FormatException)
                { document.Blocks.Add(new Paragraph(new Run("이미지를 표시하지 못했습니다 · "+picture.Groups[1].Value))); }
                continue;
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
                        var paragraph=new Paragraph { Margin=new Thickness(0),FontSize=Math.Max(13,reader.FontSize-1),FontWeight=row==0?FontWeights.SemiBold:FontWeights.Normal };
                        AddInline(paragraph,col<rows[row].Length?rows[row][col]:"",GetBaseDirectory(reader));
                        var cell=new TableCell(paragraph) { Padding=new Thickness(10,8,10,8),BorderThickness=new Thickness(0,0,0,1) };
                        cell.SetResourceReference(TableCell.BorderBrushProperty,"LineBrush");
                        cell.SetResourceReference(TableCell.BackgroundProperty,row==0?"RaisedBrush":"SurfaceBrush");
                        tr.Cells.Add(cell);
                    }
                }
                document.Blocks.Add(table);continue;
            }
            var heading=line.StartsWith("#");
            var paragraphBlock=new Paragraph { Margin=new Thickness(0,heading?15:0,0,heading?10:14),FontSize=heading?reader.FontSize+3:reader.FontSize,FontWeight=heading?FontWeights.SemiBold:FontWeights.Normal };
            if(heading)line=line.TrimStart('#').Trim();
            if(line.StartsWith("- "))line="— "+line[2..];
            AddInline(paragraphBlock,line,GetBaseDirectory(reader));document.Blocks.Add(paragraphBlock);
        }
        reader.Document=document;
        QueueHeight(reader);
    }
    private static bool TryLink(string target,string baseDirectory,out Uri uri)
    {
        uri=null!;
        if(Uri.TryCreate(target,UriKind.Absolute,out var absolute) && (absolute.Scheme=="https" || absolute.Scheme=="http" && absolute.IsLoopback))
        { uri=absolute; return true; }
        if(baseDirectory.Length==0) return false;
        try
        {
            var path=target.StartsWith("file:",StringComparison.OrdinalIgnoreCase) ? new Uri(target).LocalPath : Path.GetFullPath(target,baseDirectory);
            if(!File.Exists(path) && !Directory.Exists(path)) return false;
            uri=new Uri(path,UriKind.Absolute); return uri.IsFile;
        }
        catch(Exception ex) when(ex is ArgumentException or NotSupportedException or UriFormatException) { return false; }
    }
    private static void AddInline(Paragraph paragraph,string text,string baseDirectory)
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
            else if(TryLink(match.Groups[4].Value,baseDirectory,out var uri))
            {
                var link=new Hyperlink(new Run(match.Groups[3].Value)) { NavigateUri=uri };
                link.SetResourceReference(Hyperlink.ForegroundProperty,"AccentBrush");
                link.RequestNavigate+=(_,args)=>
                {
                    try
                    {
                        if(args.Uri.IsFile)
                        {
                            var path=args.Uri.LocalPath;
                            if(Directory.Exists(path)) Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute=true,Arguments=string.Concat((char)34,path,(char)34) })?.Dispose();
                            else if(File.Exists(path))
                            {
                                var extension=Path.GetExtension(path).ToLowerInvariant();
                                if(new[]{".exe",".html",".htm",".mp3",".wav",".flac",".m4a",".ogg",".mp4",".png",".jpg",".jpeg",".pdf",".txt"}.Contains(extension))
                                    Process.Start(new ProcessStartInfo(path) { UseShellExecute=true })?.Dispose();
                                else Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute=true,Arguments=string.Concat("/select,",(char)34,path,(char)34) })?.Dispose();
                            }
                        }
                        else Process.Start(new ProcessStartInfo(args.Uri.AbsoluteUri) { UseShellExecute=true })?.Dispose();
                    }
                    catch(Exception ex) when(ex is System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException) { }
                    args.Handled=true;
                };
                paragraph.Inlines.Add(link);
            }
            else paragraph.Inlines.Add(new Run(match.Groups[3].Value));
            offset=match.Index+match.Length;
        }
        paragraph.Inlines.Add(new Run(text[offset..]));
    }
}
