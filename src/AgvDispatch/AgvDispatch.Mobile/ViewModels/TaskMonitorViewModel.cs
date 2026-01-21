using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Shared.DTOs.Agvs;
using AgvDispatch.Shared.DTOs.Stations;
using AgvDispatch.Shared.DTOs.Tasks;
using AgvDispatch.Shared.Enums;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 任务监控ViewModel - 包含五个Tab的逻辑
/// </summary>
public partial class TaskMonitorViewModel : ObservableObject
{
    private readonly IAgvApiService _agvApiService;
    private System.Timers.Timer? _refreshTimer;

    [ObservableProperty]
    private ObservableCollection<AgvMonitorItemDto> _allAgvList = new();

    [ObservableProperty]
    private ObservableCollection<AgvMonitorItemDto> _loadingAgvList = new();

    [ObservableProperty]
    private ObservableCollection<AgvMonitorItemDto> _unloadingAgvList = new();

    [ObservableProperty]
    private ObservableCollection<AgvMonitorItemDto> _waitingAgvList = new();

    [ObservableProperty]
    private ObservableCollection<AgvMonitorItemDto> _chargingAgvList = new();

    [ObservableProperty]
    private ObservableCollection<TaskListItemDto> _tasks = new();

    [ObservableProperty]
    private ObservableCollection<StationListItemDto> _stations = new();

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _autoRefresh = true;

    [ObservableProperty]
    private int _refreshIntervalSeconds = 2;

    [ObservableProperty]
    private string? _errorMessage;

    // 统计数据
    [ObservableProperty]
    private int _totalAgvCount;

    [ObservableProperty]
    private int _totalTaskCount;

    [ObservableProperty]
    private int _executingTaskCount;

    // 用于存储当前地图ID
    private Guid _currentMapId;

    // 对话框状态
    [ObservableProperty]
    private bool _isExceptionsDialogOpen;

    [ObservableProperty]
    private string _selectedAgvCode = string.Empty;

    [ObservableProperty]
    private bool _isSettingsDialogOpen;

    public TaskMonitorViewModel(IAgvApiService agvApiService)
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
            // 并行加载数据
            var agvTask = _agvApiService.GetAgvMonitorListAsync();
            var taskTask = _agvApiService.GetAllTasksAsync();

            await Task.WhenAll(agvTask, taskTask);

            var agvs = await agvTask;
            var tasks = await taskTask;

            // 更新任务列表
            Tasks = new ObservableCollection<TaskListItemDto>(tasks);

            // 更新统计数据
            TotalAgvCount = agvs.Count;
            TotalTaskCount = tasks.Count;
            ExecutingTaskCount = tasks.Count(t => t.TaskStatus == TaskJobStatus.Executing);

            // 加载站点数据（需要先获取地图ID）
            // 这里假设使用第一个地图，实际应该从配置或用户选择中获取
            if (_currentMapId == Guid.Empty)
            {
                // TODO: 从配置或API获取默认地图ID
                // 暂时使用硬编码的地图ID
            }

            // 如果有地图ID，加载站点数据
            if (_currentMapId != Guid.Empty)
            {
                var stations = await _agvApiService.GetAllStationsAsync(_currentMapId);
                Stations = new ObservableCollection<StationListItemDto>(stations);
            }

            // 根据任务类型分类AGV
            CategorizeAgvs(agvs, tasks);
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

    /// <summary>
    /// 根据任务类型分类AGV到不同的Tab
    /// </summary>
    private void CategorizeAgvs(List<AgvMonitorItemDto> agvs, List<TaskListItemDto> tasks)
    {
        // 所有AGV
        AllAgvList = new ObservableCollection<AgvMonitorItemDto>(agvs.OrderBy(a => a.SortNo));

        var loadingList = new List<AgvMonitorItemDto>();
        var unloadingList = new List<AgvMonitorItemDto>();
        var waitingList = new List<AgvMonitorItemDto>();
        var chargingList = new List<AgvMonitorItemDto>();

        foreach (var agv in agvs)
        {
            // 查找该AGV的当前任务
            var currentTask = tasks.FirstOrDefault(t =>
                t.AssignedAgvCode == agv.AgvCode &&
                (t.TaskStatus == TaskJobStatus.Executing || t.TaskStatus == TaskJobStatus.Assigned));

            if (currentTask != null)
            {
                // 根据任务类型分类
                switch (currentTask.TaskType)
                {
                    case TaskJobType.CallForLoading:
                        loadingList.Add(agv);
                        break;
                    case TaskJobType.SendToUnloading:
                        unloadingList.Add(agv);
                        break;
                    case TaskJobType.ReturnToWaiting:
                        waitingList.Add(agv);
                        break;
                    case TaskJobType.SendToCharge:
                        chargingList.Add(agv);
                        break;
                }
            }
            else
            {
                // 如果没有任务，根据状态分类
                if (agv.AgvStatus == AgvStatus.Charging)
                {
                    chargingList.Add(agv);
                }
                else if (agv.AgvStatus == AgvStatus.Idle)
                {
                    waitingList.Add(agv);
                }
            }
        }

        LoadingAgvList = new ObservableCollection<AgvMonitorItemDto>(loadingList.OrderBy(a => a.SortNo));
        UnloadingAgvList = new ObservableCollection<AgvMonitorItemDto>(unloadingList.OrderBy(a => a.SortNo));
        WaitingAgvList = new ObservableCollection<AgvMonitorItemDto>(waitingList.OrderBy(a => a.SortNo));
        ChargingAgvList = new ObservableCollection<AgvMonitorItemDto>(chargingList.OrderBy(a => a.SortNo));
    }

    /// <summary>
    /// 启动自动刷新
    /// </summary>
    public void StartAutoRefresh()
    {
        if (_refreshTimer != null)
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
        }

        _refreshTimer = new System.Timers.Timer(RefreshIntervalSeconds * 1000);
        _refreshTimer.Elapsed += OnRefreshTimerElapsed;

        if (AutoRefresh)
        {
            _refreshTimer.Start();
        }
    }

    /// <summary>
    /// 停止自动刷新
    /// </summary>
    public void StopAutoRefresh()
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        _refreshTimer = null;
    }

    /// <summary>
    /// 切换自动刷新
    /// </summary>
    [RelayCommand]
    private void ToggleAutoRefresh()
    {
        AutoRefresh = !AutoRefresh;

        if (AutoRefresh)
        {
            StartAutoRefresh();
        }
        else
        {
            StopAutoRefresh();
        }
    }

    /// <summary>
    /// 显示异常对话框
    /// </summary>
    [RelayCommand]
    private async Task ShowExceptionsAsync(string agvCode)
    {
        var dialogViewModel = new AgvExceptionsDialogViewModel(_agvApiService);
        await dialogViewModel.InitializeAsync(agvCode);

        var dialog = new Views.Dialogs.AgvExceptionsDialog(dialogViewModel);

        // 显示对话框
        await dialog.ShowDialog(GetMainWindow());

        // 对话框关闭后刷新数据
        await LoadDataAsync();
    }

    /// <summary>
    /// 获取主窗口
    /// </summary>
    private Avalonia.Controls.Window GetMainWindow()
    {
        return Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow!
            : throw new InvalidOperationException("Cannot find main window");
    }

    /// <summary>
    /// 打开设置对话框
    /// </summary>
    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        var dialogViewModel = new SettingsDialogViewModel();
        dialogViewModel.Initialize(AutoRefresh, RefreshIntervalSeconds);

        var dialog = new Views.Dialogs.SettingsDialog(dialogViewModel);

        // 显示对话框
        await dialog.ShowDialog(GetMainWindow());

        // 应用设置
        AutoRefresh = dialogViewModel.AutoRefresh;
        RefreshIntervalSeconds = dialogViewModel.RefreshIntervalSeconds;

        // 重新启动自动刷新
        if (AutoRefresh)
        {
            StartAutoRefresh();
        }
        else
        {
            StopAutoRefresh();
        }
    }

    /// <summary>
    /// 打开上料任务创建对话框
    /// </summary>
    [RelayCommand]
    private async Task OpenCallForLoadingDialogAsync()
    {
        var dialogViewModel = new CallForLoadingDialogViewModel(_agvApiService);
        await dialogViewModel.InitializeAsync(Stations.ToList());

        var dialog = new Views.Dialogs.CallForLoadingDialog(dialogViewModel);

        var result = await dialog.ShowDialog<bool?>(GetMainWindow());

        if (result == true)
        {
            // 任务创建成功，刷新数据
            await LoadDataAsync();
        }
    }

    /// <summary>
    /// 打开下料任务创建对话框
    /// </summary>
    [RelayCommand]
    private async Task OpenSendToUnloadingDialogAsync()
    {
        var dialogViewModel = new SendToUnloadingDialogViewModel(_agvApiService);
        await dialogViewModel.InitializeAsync(Stations.ToList());

        var dialog = new Views.Dialogs.SendToUnloadingDialog(dialogViewModel);

        var result = await dialog.ShowDialog<bool?>(GetMainWindow());

        if (result == true)
        {
            // 任务创建成功，刷新数据
            await LoadDataAsync();
        }
    }

    /// <summary>
    /// 打开等待任务创建对话框
    /// </summary>
    [RelayCommand]
    private async Task OpenReturnToWaitingDialogAsync()
    {
        var dialogViewModel = new ReturnToWaitingDialogViewModel(_agvApiService);
        await dialogViewModel.InitializeAsync(Stations.ToList());

        var dialog = new Views.Dialogs.ReturnToWaitingDialog(dialogViewModel);

        var result = await dialog.ShowDialog<bool?>(GetMainWindow());

        if (result == true)
        {
            // 任务创建成功，刷新数据
            await LoadDataAsync();
        }
    }

    /// <summary>
    /// 打开充电任务创建对话框
    /// </summary>
    [RelayCommand]
    private async Task OpenSendToChargeDialogAsync()
    {
        var dialogViewModel = new SendToChargeDialogViewModel(_agvApiService);
        await dialogViewModel.InitializeAsync(Stations.ToList());

        var dialog = new Views.Dialogs.SendToChargeDialog(dialogViewModel);

        var result = await dialog.ShowDialog<bool?>(GetMainWindow());

        if (result == true)
        {
            // 任务创建成功，刷新数据
            await LoadDataAsync();
        }
    }

    /// <summary>
    /// 刷新定时器事件
    /// </summary>
    private async void OnRefreshTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// 设置地图ID
    /// </summary>
    public void SetMapId(Guid mapId)
    {
        _currentMapId = mapId;
    }

    /// <summary>
    /// 页面导航到时调用
    /// </summary>
    public void OnNavigatedTo()
    {
        // 加载数据
        _ = LoadDataAsync();

        // 启动自动刷新
        StartAutoRefresh();
    }

    /// <summary>
    /// 页面离开时调用
    /// </summary>
    public void OnNavigatedFrom()
    {
        // 停止自动刷新
        StopAutoRefresh();
    }
}
