using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed class DocumentReaderViewModel : ObservableObject
{
    private readonly Func<string,string,CancellationToken,Task<ReaderDocument>> _read;
    private int _generation;
    private string _title="문서 읽기",_filePath="",_rawText="",_format="text",_error="안내·텍스트 기록을 선택하면 이 화면에서 읽습니다.",_searchText="";
    private bool _wrapText=true,_showRaw,_isLoading;
    private IReadOnlyList<DocumentSearchHit> _matches=[];
    public DocumentReaderViewModel() : this(new DocumentReaderService().ReadAsync) { }
    public DocumentReaderViewModel(Func<string,string,CancellationToken,Task<ReaderDocument>> read) => _read=read;
    public event EventHandler? Opened;
    public string Title {get=>_title;private set=>SetProperty(ref _title,value);}
    public string FilePath {get=>_filePath;private set=>SetProperty(ref _filePath,value);}
    public string RawText {get=>_rawText;private set=>SetProperty(ref _rawText,value);}
    public string Format {get=>_format;private set=>SetProperty(ref _format,value);}
    public string Error {get=>_error;private set=>SetProperty(ref _error,value);}
    public bool IsLoading {get=>_isLoading;private set=>SetProperty(ref _isLoading,value);}
    public bool WrapText {get=>_wrapText;set=>SetProperty(ref _wrapText,value);}
    public bool ShowRaw {get=>_showRaw;set=>SetProperty(ref _showRaw,value);}
    public IReadOnlyList<DocumentSearchHit> Matches=>_matches;
    public int MatchCount=>_matches.Count;
    public string SearchSummary=>SearchText.Length==0?"선택 복사 가능 · 링크와 코드는 실행하지 않습니다.":MatchCount==1000?"검색 결과 최대 1,000개 표시":$"검색 결과 {MatchCount}개";
    public string SearchText {get=>_searchText;set { if(SetProperty(ref _searchText,value)) UpdateMatches(); }}
    public async Task OpenAsync(string approvedRoot,string filePath,string? title=null)
    {
        var current=Interlocked.Increment(ref _generation); FilePath=filePath; Title=title??Path.GetFileName(filePath); RawText=""; Error=""; IsLoading=true; SearchText=""; UpdateMatches(); Opened?.Invoke(this,EventArgs.Empty);
        try
        {
            var document=await _read(approvedRoot,filePath,CancellationToken.None);
            if(current!=_generation) return;
            RawText=document.RawText; Format=document.Format; Error=document.Error; ShowRaw=Format!="markdown"; UpdateMatches();
        }
        finally { if(current==_generation) IsLoading=false; }
    }
    private void UpdateMatches()
    {
        var hits=new List<DocumentSearchHit>();
        if(SearchText.Length>0)
        {
            var offset=0;var line=1;var scanned=0;
            while(hits.Count<1000 && offset<=RawText.Length-SearchText.Length)
            {
                var found=RawText.IndexOf(SearchText,offset,StringComparison.OrdinalIgnoreCase); if(found<0) break;
                for(var i=scanned;i<found;i++) if(RawText[i]=='\n') line++;
                var start=RawText.LastIndexOf('\n',Math.Max(0,found-1)); start=start<0?0:start+1;
                var end=RawText.IndexOf('\n',found); if(end<0) end=RawText.Length;
                hits.Add(new(found,SearchText.Length,line,RawText[start..Math.Min(end,start+180)])); offset=found+SearchText.Length; scanned=found;
            }
        }
        _matches=hits; OnPropertyChanged(nameof(Matches)); OnPropertyChanged(nameof(MatchCount)); OnPropertyChanged(nameof(SearchSummary));
    }
}
