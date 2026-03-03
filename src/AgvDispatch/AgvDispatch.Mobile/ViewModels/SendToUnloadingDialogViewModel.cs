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
/// 下料任务创建对话框ViewModel
/// </summary>
public partial class SendToUnloadingDialogViewModel : ObservableObject
{
    private readonly IAgvApiService _agvApiService;

    [ObservableProperty]
    private ObservableCollection<StationListItemDto> _stations = new();

    [ObservableProperty]
    private StationListItemDto? _selectedStation;

    [ObservableProperty]
    private ObservableCollection<SelectableAgvPendingItem> _pendingItems = new();

    [ObservableProperty]
    private ObservableCollection<SelectableAgvPendingItem> _filteredPendingItems = new();

    [ObservableProperty]
    private SelectableAgvPendingItem? _selectedItem;

    [ObservableProperty]
    private bool _isLoadingItems;

    [ObservableProperty]
    private bool _isCreatingTask;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _showAllItems;

    [ObservableProperty]
    private int _availableCount;

    public SendToUnloadingDialogViewModel(IAgvApiService agvApiService)
    {
        _agvApiService = agvApiService;
    }

    /// <summary>
    /// 初始化对话框
    /// </summary>
    public async Task InitializeAsync(List<StationListItemDto> stations)
    {
        // 筛选下料站点
        var dropoffStations = stations.Where(s => s.StationType == StationType.Dropoff).ToList();
        Stations = new ObservableCollection<StationListItemDto>(dropoffStations);

        // 加载待下料小车
        await LoadPendingItemsAsync();
    }

    /// <summary>
    /// 加载待下料小车
    /// </summary>
    [RelayCommand]
    private async Task LoadPendingItemsAsync()
    {
        IsLoadingItems = true;
        ErrorMessage = null;
        PendingItems.Clear();
        FilteredPendingItems.Clear();
        SelectedItem = null;
        AvailableCount = 0;

        try
        {
            var items = await _agvApiService.GetPendingUnloadingAgvsAsync();
            var selectableItems = items.Select(i => new SelectableAgvPendingItem(i)).ToList();
            PendingItems = new ObservableCollection<SelectableAgvPendingItem>(selectableItems);

            // 计算可用数量
            AvailableCount = PendingItems.Count(x => x.Dto.IsAvailable);

            // 更新过滤后的列表
            UpdateFilteredPendingItems();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"获取小车列表失败: {ex.Message}";
        }
        finally
        {
            IsLoadingItems = false;
        }
    }

    /// <summary>
    /// 切换显示所有项
    /// </summary>
    public void ToggleShowAll()
    {
        ShowAllItems = !ShowAllItems;
        UpdateFilteredPendingItems();
    }

    /// <summary>
    /// 更新过滤后的小车列表
    /// </summary>
    private void UpdateFilteredPendingItems()
    {
        FilteredPendingItems.Clear();

        if (ShowAllItems)
        {
            // 显示所有小车
            foreach (var item in PendingItems)
            {
                FilteredPendingItems.Add(item);
            }
        }
        else
        {
            // 只显示所有可用的小车（而不是只第一个）
            foreach (var item in PendingItems.Where(x => x.Dto.IsAvailable))
            {
                FilteredPendingItems.Add(item);
            }
        }

        // 注意：下料弹窗不自动选择任何小车
    }

    /// <summary>
    /// 选择小车
    /// </summary>
    [RelayCommand]
    public void SelectItem(SelectableAgvPendingItem item)
    {
        // 取消之前的选中状态
        if (SelectedItem != null)
        {
            SelectedItem.IsSelected = false;
        }

        // 设置新的选中状态
        SelectedItem = item;
        if (SelectedItem != null)
        {
            SelectedItem.IsSelected = true;
        }

        // haining 项目特殊处理：根据小车编号自动设置默认目标站点
        var defaultStationCode = GetDefaultStationCodeForAgv(item.Dto.AgvCode);
        if (!string.IsNullOrEmpty(defaultStationCode))
        {
            var defaultStation = Stations.FirstOrDefault(s => s.StationCode == defaultStationCode);
            if (defaultStation != null)
            {
                SelectedStation = defaultStation;
            }
        }
    }

    /// <summary>
    /// 获取小车的默认目标站点编号
    /// 备注：haining 项目特殊处理
    /// </summary>
    private string? GetDefaultStationCodeForAgv(string agvCode)
    {
        return agvCode switch
        {
            "V001" => "S201",
            "V002" => "S202",
            "V003" => "S203",
            _ => null
        };
    }

    /// <summary>
    /// 确认创建任务
    /// </summary>
    [RelayCommand]
    public async Task<bool> ConfirmAsync()
    {
        if (SelectedStation == null || SelectedItem == null)
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
                TaskType = TaskJobType.SendToUnloading,
                TargetStationCode = SelectedStation.StationCode,
                SelectedAgvCode = SelectedItem.Dto.AgvCode
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
}
