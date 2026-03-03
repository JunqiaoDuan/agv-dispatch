using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Shared.DTOs.Stations;
using AgvDispatch.Shared.DTOs.Tasks;
using AgvDispatch.Shared.Enums;
using System.Collections.ObjectModel;
using System.Linq;
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
    private ObservableCollection<SelectableAgvRecommendation> _recommendations = new();

    [ObservableProperty]
    private ObservableCollection<SelectableAgvRecommendation> _filteredRecommendations = new();

    [ObservableProperty]
    private SelectableAgvRecommendation? _selectedRecommendation;

    [ObservableProperty]
    private bool _isLoadingRecommendations;

    [ObservableProperty]
    private bool _isCreatingTask;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _showAllRecommendations;

    [ObservableProperty]
    private int _availableCount;

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
        FilteredRecommendations.Clear();
        SelectedRecommendation = null;
        AvailableCount = 0;

        try
        {
            var request = new GetRecommendationsRequestDto
            {
                TaskType = TaskJobType.CallForLoading
            };

            var recommendations = await _agvApiService.GetRecommendationsAsync(request);
            var selectableRecommendations = recommendations.Select(r => new SelectableAgvRecommendation(r)).ToList();
            Recommendations = new ObservableCollection<SelectableAgvRecommendation>(selectableRecommendations);

            // 计算可用数量
            AvailableCount = Recommendations.Count(x => x.Dto.IsAvailable);

            // 更新过滤后的列表
            UpdateFilteredRecommendations();
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
    public void ToggleShowAll()
    {
        ShowAllRecommendations = !ShowAllRecommendations;
        UpdateFilteredRecommendations();
    }

    /// <summary>
    /// 更新过滤后的推荐列表
    /// </summary>
    private void UpdateFilteredRecommendations()
    {
        FilteredRecommendations.Clear();

        if (ShowAllRecommendations)
        {
            // 显示所有推荐
            foreach (var recommendation in Recommendations)
            {
                FilteredRecommendations.Add(recommendation);
            }
        }
        else
        {
            // 只显示第一个可用的推荐
            var hasAvailable = Recommendations.Any(x => x.Dto.IsAvailable);

            if (hasAvailable)
            {
                var firstAvailable = Recommendations.FirstOrDefault(x => x.Dto.IsAvailable);
                if (firstAvailable != null)
                {
                    FilteredRecommendations.Add(firstAvailable);
                }
            }
        }

        // 自动选中第一个可用的推荐
        if (SelectedRecommendation == null && FilteredRecommendations.Any(x => x.Dto.IsAvailable))
        {
            var firstAvailable = FilteredRecommendations.FirstOrDefault(x => x.Dto.IsAvailable);
            if (firstAvailable != null)
            {
                SelectRecommendation(firstAvailable);
            }
        }
    }

    /// <summary>
    /// 选择推荐小车
    /// </summary>
    [RelayCommand]
    public void SelectRecommendation(SelectableAgvRecommendation recommendation)
    {
        // 取消之前的选中状态
        if (SelectedRecommendation != null)
        {
            SelectedRecommendation.IsSelected = false;
        }

        // 设置新的选中状态
        SelectedRecommendation = recommendation;
        if (SelectedRecommendation != null)
        {
            SelectedRecommendation.IsSelected = true;
        }
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
                SelectedAgvCode = SelectedRecommendation.Dto.AgvCode
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

        var hasAvailable = Recommendations.Any(x => x.Dto.IsAvailable);

        if (!hasAvailable)
            return false;

        var firstAvailableIndex = Recommendations.ToList().FindIndex(x => x.Dto.IsAvailable);
        return index == firstAvailableIndex;
    }
}
