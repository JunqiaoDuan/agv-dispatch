using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Shared.DTOs.Stations;
using AgvDispatch.Shared.DTOs.Tasks;
using AgvDispatch.Shared.Enums;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 上料任务创建对话框ViewModel
/// </summary>
public partial class CallForLoadingDialogViewModel : ObservableObject
{
    private readonly IAgvApiService _agvApiService;

    [ObservableProperty]
    private ObservableCollection<StationListItemDto> _stations = new();

    [ObservableProperty]
    private StationListItemDto? _selectedStation;

    [ObservableProperty]
    private ObservableCollection<AgvRecommendationDto> _recommendations = new();

    [ObservableProperty]
    private AgvRecommendationDto? _selectedRecommendation;

    [ObservableProperty]
    private bool _isLoadingRecommendations;

    [ObservableProperty]
    private bool _isCreatingTask;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _showAllRecommendations;

    public CallForLoadingDialogViewModel(IAgvApiService agvApiService)
    {
        _agvApiService = agvApiService;
    }

    /// <summary>
    /// 初始化对话框
    /// </summary>
    public async Task InitializeAsync(List<StationListItemDto> stations)
    {
        // 筛选上料站点
        var pickupStations = stations.Where(s => s.StationType == StationType.Pickup).ToList();
        Stations = new ObservableCollection<StationListItemDto>(pickupStations);

        // 加载推荐小车
        await LoadRecommendationsAsync();
    }

    /// <summary>
    /// 加载推荐小车
    /// </summary>
    [RelayCommand]
    private async Task LoadRecommendationsAsync()
    {
        IsLoadingRecommendations = true;
        ErrorMessage = null;
        Recommendations.Clear();
        SelectedRecommendation = null;

        try
        {
            var request = new GetRecommendationsRequestDto
            {
                TaskType = TaskJobType.CallForLoading
            };

            var recommendations = await _agvApiService.GetRecommendationsAsync(request);
            Recommendations = new ObservableCollection<AgvRecommendationDto>(recommendations);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"获取推荐失败: {ex.Message}";
        }
        finally
        {
            IsLoadingRecommendations = false;
        }
    }

    /// <summary>
    /// 切换显示所有推荐
    /// </summary>
    [RelayCommand]
    private void ToggleShowAll()
    {
        ShowAllRecommendations = !ShowAllRecommendations;
    }

    /// <summary>
    /// 选择推荐小车
    /// </summary>
    [RelayCommand]
    public void SelectRecommendation(AgvRecommendationDto recommendation)
    {
        SelectedRecommendation = recommendation;
    }

    /// <summary>
    /// 确认创建任务
    /// </summary>
    [RelayCommand]
    public async Task<bool> ConfirmAsync()
    {
        if (SelectedStation == null || SelectedRecommendation == null)
        {
            ErrorMessage = "请选择站点和小车";
            return false;
        }

        IsCreatingTask = true;
        ErrorMessage = null;

        try
        {
            var request = new CreateTaskWithAgvRequestDto
            {
                TaskType = TaskJobType.CallForLoading,
                TargetStationCode = SelectedStation.StationCode,
                SelectedAgvCode = SelectedRecommendation.AgvCode
            };

            var response = await _agvApiService.CreateTaskAsync(request);

            if (response != null)
            {
                return true;
            }
            else
            {
                ErrorMessage = "创建任务失败";
                return false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"创建任务失败: {ex.Message}";
            return false;
        }
        finally
        {
            IsCreatingTask = false;
        }
    }

    /// <summary>
    /// 判断是否应该显示该推荐
    /// </summary>
    public bool ShouldShowRecommendation(int index, bool isAvailable)
    {
        if (ShowAllRecommendations)
            return true;

        var hasAvailable = Recommendations.Any(x => x.IsAvailable);

        if (!hasAvailable)
            return false;

        var firstAvailableIndex = Recommendations.ToList().FindIndex(x => x.IsAvailable);
        return index == firstAvailableIndex;
    }
}
