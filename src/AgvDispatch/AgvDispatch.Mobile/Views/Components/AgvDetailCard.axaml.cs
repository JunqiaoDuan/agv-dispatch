using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AgvDispatch.Shared.DTOs.Agvs;
using AgvDispatch.Shared.DTOs.Tasks;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Mobile.ViewModels;
using System.Linq;
using System.ComponentModel;

namespace AgvDispatch.Mobile.Views.Components;

public partial class AgvDetailCard : UserControl
{
    private TaskListItemDto? _currentTask;
    private TaskMonitorViewModel? _viewModel;

    public AgvDetailCard()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is AgvMonitorItemDto agv)
        {
            // 订阅父 ViewModel 的属性变化通知
            SubscribeToViewModelChanges();
            UpdateTaskInfo(agv);
        }
    }

    /// <summary>
    /// 控件添加到可视树时
    /// </summary>
    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // 在添加到可视树时再次尝试更新（此时 Parent 已经设置）
        if (DataContext is AgvMonitorItemDto agv)
        {
            SubscribeToViewModelChanges();
            UpdateTaskInfo(agv);
        }
    }

    /// <summary>
    /// 订阅父 ViewModel 的属性变化通知
    /// </summary>
    private void SubscribeToViewModelChanges()
    {
        // 取消之前的订阅
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        // 获取父 ViewModel 并订阅
        _viewModel = GetTaskMonitorViewModel();
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// 处理 ViewModel 属性变化
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 当 Tasks 属性变化时，重新更新任务信息
        if (e.PropertyName == nameof(TaskMonitorViewModel.Tasks) && DataContext is AgvMonitorItemDto agv)
        {
            UpdateTaskInfo(agv);
        }
    }

    /// <summary>
    /// 更新任务信息显示
    /// </summary>
    private void UpdateTaskInfo(AgvMonitorItemDto agv)
    {
        // 从父 ViewModel 获取任务列表
        var viewModel = GetTaskMonitorViewModel();
        if (viewModel == null)
        {
            SetTaskInfo(null);
            return;
        }

        // 查找该 AGV 的当前任务(Executing 或 Assigned 状态)
        _currentTask = viewModel.Tasks
            .FirstOrDefault(t =>
                t.AssignedAgvCode == agv.AgvCode &&
                (t.TaskStatus == TaskJobStatus.Executing || t.TaskStatus == TaskJobStatus.Assigned));

        SetTaskInfo(_currentTask);
    }

    /// <summary>
    /// 设置任务信息显示
    /// </summary>
    private void SetTaskInfo(TaskListItemDto? task)
    {
        if (TaskText == null || TaskIcon == null) return;

        if (task != null)
        {
            // 显示任务类型
            TaskText.Text = task.TaskType.ToDisplayText();

            // 根据任务类型设置颜色
            var color = GetTaskTypeColor(task.TaskType);
            TaskText.Foreground = new SolidColorBrush(color);
            TaskIcon.Foreground = new SolidColorBrush(color);
        }
        else
        {
            // 无任务
            TaskText.Text = "无任务";
            var grayColor = Color.Parse("#757575");
            TaskText.Foreground = new SolidColorBrush(grayColor);
            TaskIcon.Foreground = new SolidColorBrush(grayColor);
        }
    }

    /// <summary>
    /// 根据任务类型获取颜色
    /// </summary>
    private Color GetTaskTypeColor(TaskJobType taskType) => taskType switch
    {
        TaskJobType.CallForLoading => Color.Parse("#4CAF50"),     // 绿色 - 召唤小车上料
        TaskJobType.SendToUnloading => Color.Parse("#2196F3"),    // 蓝色 - 告知小车去下料
        TaskJobType.ReturnToWaiting => Color.Parse("#FF9800"),    // 橙色 - 确认下料去等待区
        TaskJobType.SendToCharge => Color.Parse("#9C27B0"),       // 紫色 - 让小车充电
        _ => Color.Parse("#757575")
    };

    private void OnExceptionTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is AgvMonitorItemDto agv)
        {
            // 获取 ViewModel
            var viewModel = GetTaskMonitorViewModel();
            if (viewModel != null && agv.UnresolvedExceptionCount > 0)
            {
                viewModel.ShowAgvExceptionsCommand.Execute(agv.AgvCode);
            }
        }
    }

    private void OnCargoTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is AgvMonitorItemDto agv)
        {
            // 获取 ViewModel
            var viewModel = GetTaskMonitorViewModel();
            if (viewModel != null)
            {
                viewModel.ShowToggleCargoCommand.Execute(agv);
            }
        }
    }

    /// <summary>
    /// 任务点击事件 - 打开任务详情
    /// </summary>
    private void OnTaskTapped(object? sender, TappedEventArgs e)
    {
        if (_currentTask == null) return;

        var viewModel = GetTaskMonitorViewModel();
        if (viewModel != null)
        {
            viewModel.ShowTaskDetailCommand.Execute(_currentTask);
        }
    }

    private TaskMonitorViewModel? GetTaskMonitorViewModel()
    {
        // 从父控件层级查找 TaskMonitorViewModel
        var parent = this.Parent;
        while (parent != null)
        {
            if (parent is UserControl userControl && userControl.DataContext is TaskMonitorViewModel vm)
            {
                return vm;
            }
            parent = parent.Parent;
        }
        return null;
    }

    /// <summary>
    /// 组件卸载时清理资源
    /// </summary>
    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        // 取消 ViewModel 订阅
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = null;
        }
    }
}
