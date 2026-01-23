using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Shared.DTOs.Agvs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 小车列表ViewModel
/// </summary>
public partial class AgvListViewModel : ObservableObject
{
    private readonly IAgvApiService _agvApiService;

    [ObservableProperty]
    private ObservableCollection<AgvListItemDto> _agvList = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private int _totalAgvs;

    [ObservableProperty]
    private int _onlineAgvs;

    [ObservableProperty]
    private int _runningAgvs;

    [ObservableProperty]
    private int _errorAgvs;

    public AgvListViewModel(IAgvApiService agvApiService)
    {
        _agvApiService = agvApiService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var agvs = await _agvApiService.GetAllAgvsAsync();
            AgvList = new ObservableCollection<AgvListItemDto>(agvs);

            // 计算统计数据
            TotalAgvs = agvs.Count;
            OnlineAgvs = agvs.Count(x => x.AgvStatus == Shared.Enums.AgvStatus.Online);
            RunningAgvs = 0; // 不再统计运行中的 AGV（需要查询任务表）
            ErrorAgvs = 0;   // 不再统计故障 AGV（需要查询异常表）
        }
        catch (Exception ex)
        {
            ErrorMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OnNavigatedTo()
    {
        // 页面导航到时自动加载数据
        _ = LoadDataAsync();
    }
}
