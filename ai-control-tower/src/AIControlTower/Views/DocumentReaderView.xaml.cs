using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Views;

public partial class DocumentReaderView : UserControl
{
    private DocumentReaderViewModel? _model;
    private int _hit=-1;
    private string? _renderedText;
    public DocumentReaderView()
    {
        InitializeComponent();
        DataContextChanged+=(_,_)=>Attach(DataContext as DocumentReaderViewModel);
        Unloaded+=(_,_)=>Attach(null);
        Loaded+=(_,_)=>Attach(DataContext as DocumentReaderViewModel);
    }
    private void Attach(DocumentReaderViewModel? model)
    {
        if(_model is not null) { CaptureView();_model.PropertyChanged-=Changed;_model.Opening-=CaptureView;_model.StateRestored-=RestoreView; }
        _model=model;
        if(_model is not null) { _model.PropertyChanged+=Changed;_model.Opening+=CaptureView;_model.StateRestored+=RestoreView;Render();RestoreView(null,EventArgs.Empty); }
    }
    private void Changed(object? sender,PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(DocumentReaderViewModel.RawText) or nameof(DocumentReaderViewModel.Format)) Render();
        if(e.PropertyName==nameof(DocumentReaderViewModel.Matches)) { _hit=-1;if(_model is not null)_model.SearchHitIndex=-1; }
    }
    private void Render()
    {
        var text=_model?.RawText??"";
        if(text==_renderedText)return;
        _renderedText=text;
        FormattedBody.Document=DocumentReaderService.RenderMarkdown(text);
    }
    private void CaptureView(object? sender,EventArgs e)=>CaptureView();
    private void CaptureView()
    {
        if(_model is null || _model.IsLoading)return;
        _model.RawScrollOffset=RawBody.VerticalOffset;_model.FormattedScrollOffset=FormattedBody.VerticalOffset;
        _model.SelectionStart=RawBody.SelectionStart;_model.SelectionLength=RawBody.SelectionLength;_model.SearchHitIndex=_hit;
    }
    private void RestoreView(object? sender,EventArgs e)
    {
        var model=_model;if(model is null)return;var path=model.FilePath;
        Dispatcher.BeginInvoke(()=>
        {
            if(_model!=model || model.FilePath!=path)return;
            RawBody.Select(Math.Min(model.SelectionStart,RawBody.Text.Length),Math.Min(model.SelectionLength,Math.Max(0,RawBody.Text.Length-model.SelectionStart)));
            RawBody.ScrollToVerticalOffset(model.RawScrollOffset);FormattedBody.ScrollToVerticalOffset(model.FormattedScrollOffset);_hit=model.SearchHitIndex;
        },System.Windows.Threading.DispatcherPriority.Loaded);
    }
    private async void Back_Click(object sender,RoutedEventArgs e) { if(_model is not null)await _model.GoBackAsync(); }
    private async void Forward_Click(object sender,RoutedEventArgs e) { if(_model is not null)await _model.GoForwardAsync(); }
    private void Return_Click(object sender,RoutedEventArgs e)=>_model?.ReturnToOrigin();
    private void Previous_Click(object sender,RoutedEventArgs e)=>SelectHit(-1);
    private void Next_Click(object sender,RoutedEventArgs e)=>SelectHit(1);
    private void SelectHit(int direction)
    {
        if(_model is null || _model.MatchCount==0) return;
        _hit=_hit<0?(direction>0?0:_model.MatchCount-1):(_hit+direction+_model.MatchCount)%_model.MatchCount;
        _model.SearchHitIndex=_hit;
        _model.ShowRaw=true; var hit=_model.Matches[_hit]; RawBody.Select(hit.Offset,hit.Length);
        RawBody.ScrollToLine(Math.Max(0,RawBody.GetLineIndexFromCharacterIndex(hit.Offset))); RawBody.Focus();
    }
}
