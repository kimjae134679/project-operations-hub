using AIControlTower.Models;

namespace AIControlTower.ViewModels;

public sealed class ToolStatusViewModel : ObservableObject
{
    private ToolStatus _status;
    public ToolStatusViewModel(ToolStatus status) => _status = status;
    public string DisplayName => _status.DisplayName;
    public string State => _status.Kind.ToString();
    public string Detail => _status.Detail;
    public string CheckedAt => _status.CheckedAt.ToLocalTime().ToString("HH:mm:ss");
    public StatusKind Kind => _status.Kind;
    public void Update(ToolStatus status) { _status = status; OnPropertyChanged(nameof(State)); OnPropertyChanged(nameof(Detail)); OnPropertyChanged(nameof(CheckedAt)); OnPropertyChanged(nameof(Kind)); }
}
