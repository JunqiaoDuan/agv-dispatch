using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Shared.DTOs.Tasks;
using AgvDispatch.Shared.Enums;
using System;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 任务详情对话框 ViewModel
/// </summary>
public partial class TaskDetailDialogViewModel : ObservableObject
{
    private readonly IAgvApiService _agvApiService;

    [ObservableProperty]
    private TaskDetailDto? _task;

    /// <summary>
    /// 计算等待时间
    /// </summary>
    public string WaitingTime
    {
        get
        {
            if (Task?.CreationDate == null || Task.AssignedAt == null)
            {
                return "--";
            }

            var waitingTime = Task.AssignedAt.Value - Task.CreationDate.Value;
            return FormatTimeSpan(waitingTime);
        }
    }

    /// <summary>
    /// 计算已耗时
    /// </summary>
    public string ElapsedTime
    {
        get
        {
            if (Task?.StartedAt == null)
            {
                return "--";
            }

            var endTime = Task.CompletedAt ?? DateTimeOffset.Now;
            var elapsedTime = endTime - Task.StartedAt.Value;
            return FormatTimeSpan(elapsedTime);
        }
    }

    /// <summary>
    /// 计算总耗时
    /// </summary>
    public string TotalTime
    {
        get
        {
            if (Task?.CreationDate == null || Task.CompletedAt == null)
            {
                return "--";
            }

            var totalTime = Task.CompletedAt.Value - Task.CreationDate.Value;
            return FormatTimeSpan(totalTime);
        }
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _canCancel;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _cancelReason;

    [ObservableProperty]
    private bool _isCanceling;

    public TaskDetailDialogViewModel(IAgvApiService agvApiService)
    {
        _agvApiService = agvApiService;
    }

    public async Task InitializeAsync(Guid taskId)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var task = await _agvApiService.GetTaskDetailAsync(taskId);
            if (task != null)
            {
                Task = task;

                // 判断是否可以取消
                CanCancel = task.TaskStatus == TaskJobStatus.Pending ||
                           task.TaskStatus == TaskJobStatus.Assigned ||
                           task.TaskStatus == TaskJobStatus.Executing;
            }
            else
            {
                ErrorMessage = "无法加载任务详情";
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

    [RelayCommand]
    private async Task CancelTaskAsync()
    {
        if (Task == null || IsCanceling)
        {
            return;
        }

        IsCanceling = true;

        try
        {
            var request = new CancelTaskRequestDto
            {
                TaskId = Task.Id,
                Reason = CancelReason
            };

            var result = await _agvApiService.CancelTaskAsync(request);

            if (result != null)
            {
                // 取消成功，重新加载任务详情
                await InitializeAsync(Task.Id);
            }
            else
            {
                ErrorMessage = "取消任务失败";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"取消失败: {ex.Message}";
        }
        finally
        {
            IsCanceling = false;
        }
    }

    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        else
        {
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}
