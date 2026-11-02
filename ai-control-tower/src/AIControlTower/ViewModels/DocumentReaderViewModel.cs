using AIControlTower.Models;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed class DocumentReaderViewModel : ObservableObject
{
    private readonly Func<string,string,CancellationToken,Task<ReaderDocument>> _read;
    private int _generation;
    private readonly object _ownedAdmission=new();
    private int _ownedReads;
    private bool _ownedFrozen;
    public bool OwnedReadsIdle {get {lock(_ownedAdmission)return _ownedReads==0;}}
    public void FreezeOwnedAdmission(bool frozen){lock(_ownedAdmission)_ownedFrozen=frozen;}
    private async Task<ReaderDocument> ReadOwnedAsync(string root,string path)
    {
        lock(_ownedAdmission){if(_ownedFrozen)throw new InvalidOperationException("안전 종료 확인 중 · 새 문서 조회 보류");_ownedReads++;}
        try{return await _read(root,path,CancellationToken.None);}
        finally{lock(_ownedAdmission)_ownedReads--;}
    }
    private string _title="문서 읽기",_filePath="",_rawText="",_format="text",_error="안내·텍스트 기록을 선택하면 이 화면에서 읽습니다.",_searchText="";
    private bool _wrapText=true,_showRaw,_isLoading;
    private IReadOnlyList<DocumentSearchHit> _matches=[];
    private readonly List<DocumentState> _history=[];
    private int _historyIndex=-1;
    private double _rawScrollOffset,_formattedScrollOffset;
    private int _selectionStart,_selectionLength,_searchHitIndex=-1;
    private bool _canReturnToOrigin;
    private sealed record DocumentState(string Root,string Path,string Title,string Search="",bool Raw=false,bool Wrap=true,
        double RawOffset=0,double FormattedOffset=0,int Selection=0,int Length=0,int Hit=-1);
    public DocumentReaderViewModel() : this(new DocumentReaderService().ReadAsync) { }
    public DocumentReaderViewModel(Func<string,string,CancellationToken,Task<ReaderDocument>> read) => _read=read;
    public event EventHandler? Opened;
    public event EventHandler? Opening;
    public event EventHandler? StateRestored;
    public event EventHandler? ReturnRequested;
    public bool CanGoBack=>_historyIndex>0;
    public bool CanGoForward=>_historyIndex>=0 && _historyIndex<_history.Count-1;
    public int HistoryCount=>_history.Count;
    public bool CanReturnToOrigin {get=>_canReturnToOrigin;set=>SetProperty(ref _canReturnToOrigin,value);}
    public double RawScrollOffset {get=>_rawScrollOffset;set=>_rawScrollOffset=double.IsFinite(value)?Math.Max(0,value):0;}
    public double FormattedScrollOffset {get=>_formattedScrollOffset;set=>_formattedScrollOffset=double.IsFinite(value)?Math.Max(0,value):0;}
    public int SelectionStart {get=>_selectionStart;set=>_selectionStart=Math.Max(0,value);}
    public int SelectionLength {get=>_selectionLength;set=>_selectionLength=Math.Max(0,value);}
    public int SearchHitIndex {get=>_searchHitIndex;set=>_searchHitIndex=value;}
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
        lock(_ownedAdmission)if(_ownedFrozen)return;
        Opening?.Invoke(this,EventArgs.Empty); SaveCurrent();
        if(_historyIndex+1<_history.Count)_history.RemoveRange(_historyIndex+1,_history.Count-_historyIndex-1);
        var state=new DocumentState(approvedRoot,filePath,title??Path.GetFileName(filePath));_history.Add(state);
        if(_history.Count>24)_history.RemoveAt(0);
        _historyIndex=_history.Count-1; NotifyHistory();await LoadAsync(state,false);
    }
    public Task GoBackAsync()=>NavigateAsync(-1);
    public Task GoForwardAsync()=>NavigateAsync(1);
    public void ReturnToOrigin() { if(CanReturnToOrigin)ReturnRequested?.Invoke(this,EventArgs.Empty); }
    private async Task NavigateAsync(int direction)
    {
        lock(_ownedAdmission)if(_ownedFrozen)return;
        var next=_historyIndex+direction;if(next<0 || next>=_history.Count)return;
        Opening?.Invoke(this,EventArgs.Empty);SaveCurrent();_historyIndex=next;NotifyHistory();await LoadAsync(_history[next],true);
    }
    private void SaveCurrent()
    {
        if(_historyIndex<0)return;
        _history[_historyIndex]=_history[_historyIndex] with { Search=SearchText,Raw=ShowRaw,Wrap=WrapText,
            RawOffset=RawScrollOffset,FormattedOffset=FormattedScrollOffset,Selection=SelectionStart,Length=SelectionLength,Hit=SearchHitIndex };
    }
    private void NotifyHistory() { OnPropertyChanged(nameof(CanGoBack));OnPropertyChanged(nameof(CanGoForward));OnPropertyChanged(nameof(HistoryCount)); }
    private async Task LoadAsync(DocumentState state,bool restore)
    {
        var current=Interlocked.Increment(ref _generation); FilePath=state.Path; Title=state.Title; RawText=""; Error=""; IsLoading=true;
        SearchText="";RawScrollOffset=0;FormattedScrollOffset=0;SelectionStart=0;SelectionLength=0;SearchHitIndex=-1;
        UpdateMatches(); Opened?.Invoke(this,EventArgs.Empty);
        try
        {
            // History stores metadata only. Re-read through the same approved-root/security reader.
            var document=await ReadOwnedAsync(state.Root,state.Path);
            if(current!=_generation) return;
            RawText=document.RawText; Format=document.Format; Error=document.Error; ShowRaw=Format!="markdown"; UpdateMatches();
            if(restore && document.Success)
            {
                SearchText=state.Search;ShowRaw=state.Raw;WrapText=state.Wrap;RawScrollOffset=state.RawOffset;FormattedScrollOffset=state.FormattedOffset;
                SelectionStart=Math.Min(state.Selection,RawText.Length);SelectionLength=Math.Min(state.Length,RawText.Length-SelectionStart);SearchHitIndex=state.Hit;
            }
            StateRestored?.Invoke(this,EventArgs.Empty);
        }
        catch(Exception) { if(current==_generation){Error="문서를 읽을 수 없습니다. 확인된 위치와 접근 조건을 확인하세요.";RawText="";} }
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
