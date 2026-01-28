using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Shared.DTOs.Agvs;
using AgvDispatch.Shared.DTOs.PathLocks;
using AgvDispatch.Shared.DTOs.Stations;
using AgvDispatch.Shared.DTOs.Tasks;
using AgvDispatch.Shared.Enums;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 任务监控ViewModel - 重构为3个Tab
/// </summary>
public partial class TaskMonitorViewModel : ObservableObject
{
    private readonly IAgvApiService _agvApiService;
    private System.Timers.Timer? _refreshTimer;

    [ObservableProperty]
    private ObservableCollection<AgvMonitorItemDto> _allAgvList = new();

    [ObservableProperty]
    private ObservableCollection<TaskListItemDto> _executingTaskList = new();

    [ObservableProperty]
    private ObservableCollection<TaskListItemDto> _tasks = new();

    [ObservableProperty]
    private ObservableCollection<StationListItemDto> _stations = new();

    [ObservableProperty]
    private ObservableCollection<ActiveChannelDto> _activeChannels = new();

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _autoRefresh = true;

    [ObservableProperty]
    private int _refreshIntervalSeconds = 10;

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
            var channelsTask = _agvApiService.GetActiveChannelsAsync();

            await Task.WhenAll(agvTask, taskTask, channelsTask);

            var agvs = await agvTask;
            var tasks = await taskTask;
            var channels = await channelsTask;

            // 更新任务列表
            Tasks = new ObservableCollection<TaskListItemDto>(tasks);

            // 更新 AllAgvList
            AllAgvList = new ObservableCollection<AgvMonitorItemDto>(agvs.OrderBy(a => a.SortNo));

            // 筛选 Executing 状态的任务到 ExecutingTaskList
            var executingTasks = tasks
                .Where(t => t.TaskStatus == TaskJobStatus.Executing)
                .OrderByDescending(t => t.CreationDate);
            ExecutingTaskList = new ObservableCollection<TaskListItemDto>(executingTasks);

            // 更新已放行通道列表
            ActiveChannels = new ObservableCollection<ActiveChannelDto>(channels);

            // 更新统计数据
            TotalAgvCount = agvs.Count;
            TotalTaskCount = tasks.Count;
            ExecutingTaskCount = tasks.Count(t => t.TaskStatus == TaskJobStatus.Executing);

            // 加载站点数据（需要先获取地图ID）
            if (_currentMapId != Guid.Empty)
            {
                var stations = await _agvApiService.GetAllStationsAsync(_currentMapId);
                Stations = new ObservableCollection<StationListItemDto>(stations);
            }
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
    /// 显示任务详情对话框
    /// </summary>
    [RelayCommand]
    private async Task ShowTaskDetailAsync(TaskListItemDto task)
    {
        if (task == null) return;

        var dialogViewModel = new TaskDetailDialogViewModel(_agvApiService);
        await dialogViewModel.InitializeAsync(task.Id);

        var dialog = new Views.Dialogs.TaskDetailDialog(dialogViewModel);

        await dialog.ShowDialog(GetMainWindow());

        // 对话框关闭后刷新数据
        await LoadDataAsync();
    }

    /// <summary>
    /// 显示 AGV 异常对话框
    /// </summary>
    [RelayCommand]
    private async Task ShowAgvExceptionsAsync(string agvCode)
    {
        if (string.IsNullOrEmpty(agvCode)) return;

        var dialogViewModel = new AgvExceptionsDialogViewModel(_agvApiService);
        await dialogViewModel.InitializeAsync(agvCode);

        var dialog = new Views.Dialogs.AgvExceptionsDialog(dialogViewModel);

        // 显示对话框
        await dialog.ShowDialog(GetMainWindow());

        // 对话框关闭后刷新数据
        await LoadDataAsync();
    }

    /// <summary>
    /// 显示货物状态切换对话框
    /// </summary>
    [RelayCommand]
    private async Task ShowToggleCargoAsync(AgvMonitorItemDto agv)
    {
        if (agv == null) return;

        var dialogViewModel = new ToggleCargoDialogViewModel(_agvApiService);
        dialogViewModel.Initialize(agv);

        var dialog = new Views.Dialogs.ToggleCargoDialog(dialogViewModel);

        var result = await dialog.ShowDialog<bool?>(GetMainWindow());

        if (result == true)
        {
            // 切换成功，刷新数据
            await LoadDataAsync();
        }
    }

    /// <summary>
    /// 显示 AGV 当前任务详情
    /// </summary>
    [RelayCommand]
    private async Task ShowAgvTaskDetailAsync(string agvCode)
    {
        if (string.IsNullOrEmpty(agvCode)) return;

        // 查找该 AGV 的当前任务
        var currentTask = Tasks.FirstOrDefault(t =>
            t.AssignedAgvCode == agvCode &&
            (t.TaskStatus == TaskJobStatus.Executing || t.TaskStatus == TaskJobStatus.Assigned));

        if (currentTask != null)
        {
            await ShowTaskDetailAsync(currentTask);
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

    /// <summary>
    /// 获取主窗口
    /// </summary>
    private Avalonia.Controls.Window GetMainWindow()
    {
        return Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow!
            : throw new InvalidOperationException("Cannot find main window");
    }
}
