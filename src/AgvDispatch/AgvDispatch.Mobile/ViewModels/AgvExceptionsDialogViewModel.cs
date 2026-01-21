using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Shared.DTOs.Agvs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// AGV异常对话框ViewModel
/// </summary>
public partial class AgvExceptionsDialogViewModel : ObservableObject
{
    private readonly IAgvApiService _agvApiService;

    [ObservableProperty]
    private string _agvCode = string.Empty;

    [ObservableProperty]
    private ObservableCollection<AgvExceptionSummaryDto> _unresolvedExceptions = new();

    [ObservableProperty]
    private ObservableCollection<AgvExceptionSummaryDto> _selectedExceptions = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isResolving;

    [ObservableProperty]
    private string? _errorMessage;

    public AgvExceptionsDialogViewModel(IAgvApiService agvApiService)
    {
        _agvApiService = agvApiService;
    }

    /// <summary>
    /// 初始化对话框
    /// </summary>
    public async Task InitializeAsync(string agvCode)
    {
        AgvCode = agvCode;
        await LoadExceptionsAsync();
    }

    /// <summary>
    /// 加载异常列表
    /// </summary>
    [RelayCommand]
    private async Task LoadExceptionsAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var exceptions = await _agvApiService.GetAgvUnresolvedExceptionsAsync(AgvCode);
            UnresolvedExceptions = new ObservableCollection<AgvExceptionSummaryDto>(exceptions);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"加载异常失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 全选
    /// </summary>
    [RelayCommand]
    private void SelectAll()
    {
        SelectedExceptions.Clear();
        foreach (var exception in UnresolvedExceptions)
        {
            SelectedExceptions.Add(exception);
        }
    }

    /// <summary>
    /// 解决选中的异常
    /// </summary>
    [RelayCommand]
    private async Task ResolveSelectedAsync()
    {
        if (!SelectedExceptions.Any())
        {
            ErrorMessage = "请至少选择一条异常";
            return;
        }

        IsResolving = true;
        ErrorMessage = null;

        try
        {
            var exceptionIds = SelectedExceptions.Select(e => e.Id).ToList();
            var success = await _agvApiService.ResolveExceptionsAsync(exceptionIds);

            if (success)
            {
                // 重新加载数据
                await LoadExceptionsAsync();
                SelectedExceptions.Clear();
            }
            else
            {
                ErrorMessage = "消除异常失败";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"消除异常失败: {ex.Message}";
        }
        finally
        {
            IsResolving = false;
        }
    }
}
