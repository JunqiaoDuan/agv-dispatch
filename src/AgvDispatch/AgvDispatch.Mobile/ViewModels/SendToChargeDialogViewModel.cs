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
/// 充电任务创建对话框ViewModel
/// </summary>
public partial class SendToChargeDialogViewModel : ObservableObject
{
    private readonly IAgvApiService _agvApiService;

    [ObservableProperty]
    private ObservableCollection<StationListItemDto> _stations = new();

    [ObservableProperty]
    private StationListItemDto? _selectedStation;

    [ObservableProperty]
    private ObservableCollection<AgvPendingItemDto> _pendingItems = new();

    [ObservableProperty]
    private AgvPendingItemDto? _selectedItem;

    [ObservableProperty]
    private bool _isLoadingItems;

    [ObservableProperty]
    private bool _isCreatingTask;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _showAllItems;

    public SendToChargeDialogViewModel(IAgvApiService agvApiService)
    {
        _agvApiService = agvApiService;
    }

    /// <summary>
    /// 初始化对话框
    /// </summary>
    public async Task InitializeAsync(List<StationListItemDto> stations)
    {
        // 筛选充电站点
        var chargeStations = stations.Where(s => s.StationType == StationType.Charge).ToList();
        Stations = new ObservableCollection<StationListItemDto>(chargeStations);

        // 加载可充电小车
        await LoadPendingItemsAsync();
    }

    /// <summary>
    /// 加载可充电小车
    /// </summary>
    [RelayCommand]
    private async Task LoadPendingItemsAsync()
    {
        IsLoadingItems = true;
        ErrorMessage = null;
        PendingItems.Clear();
        SelectedItem = null;

        try
        {
            var items = await _agvApiService.GetChargeableAgvsAsync();
            PendingItems = new ObservableCollection<AgvPendingItemDto>(items);
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
    [RelayCommand]
    private void ToggleShowAll()
    {
        ShowAllItems = !ShowAllItems;
    }

    /// <summary>
    /// 选择小车
    /// </summary>
    [RelayCommand]
    public void SelectItem(AgvPendingItemDto item)
    {
        SelectedItem = item;
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
                TaskType = TaskJobType.SendToCharge,
                TargetStationCode = SelectedStation.StationCode,
                SelectedAgvCode = SelectedItem.AgvCode
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
