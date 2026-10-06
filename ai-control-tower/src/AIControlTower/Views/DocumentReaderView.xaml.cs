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
    public DocumentReaderView()
    {
        InitializeComponent();
        DataContextChanged+=(_,_)=>Attach(DataContext as DocumentReaderViewModel);
        Unloaded+=(_,_)=>Attach(null);
        Loaded+=(_,_)=>Attach(DataContext as DocumentReaderViewModel);
    }
    private void Attach(DocumentReaderViewModel? model)
    {
        if(_model is not null) _model.PropertyChanged-=Changed;
        _model=model;
        if(_model is not null) _model.PropertyChanged+=Changed;
        Render();
    }
    private void Changed(object? sender,PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(DocumentReaderViewModel.RawText) or nameof(DocumentReaderViewModel.Format)) Render();
        if(e.PropertyName==nameof(DocumentReaderViewModel.Matches)) _hit=-1;
    }
    private void Render()
    {
        var text=_model?.RawText??"";
        FormattedBody.Document=DocumentReaderService.RenderMarkdown(text);
    }
    private void Previous_Click(object sender,RoutedEventArgs e)=>SelectHit(-1);
    private void Next_Click(object sender,RoutedEventArgs e)=>SelectHit(1);
    private void SelectHit(int direction)
    {
        if(_model is null || _model.MatchCount==0) return;
        _hit=_hit<0?(direction>0?0:_model.MatchCount-1):(_hit+direction+_model.MatchCount)%_model.MatchCount;
        _model.ShowRaw=true; var hit=_model.Matches[_hit]; RawBody.Select(hit.Offset,hit.Length);
        RawBody.ScrollToLine(Math.Max(0,RawBody.GetLineIndexFromCharacterIndex(hit.Offset))); RawBody.Focus();
    }
}
