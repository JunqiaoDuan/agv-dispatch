using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Shared.DTOs.Agvs;
using System;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 货物状态切换对话框 ViewModel
/// </summary>
public partial class ToggleCargoDialogViewModel : ObservableObject
{
    private readonly IAgvApiService _agvApiService;

    [ObservableProperty]
    private Guid _agvId;

    [ObservableProperty]
    private string _agvCode = string.Empty;

    [ObservableProperty]
    private bool _currentHasCargo;

    [ObservableProperty]
    private bool _targetHasCargo;

    [ObservableProperty]
    private string? _reason;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string? _errorMessage;

    public ToggleCargoDialogViewModel(IAgvApiService agvApiService)
    {
        _agvApiService = agvApiService;
    }

    public void Initialize(AgvMonitorItemDto agv)
    {
        AgvId = agv.Id;
        AgvCode = agv.AgvCode;
        CurrentHasCargo = agv.HasCargo;
        TargetHasCargo = !agv.HasCargo;
    }

    public async Task<bool> ConfirmAsync()
    {
        if (IsProcessing)
        {
            return false;
        }

        IsProcessing = true;
        ErrorMessage = null;

        try
        {
            var request = new ManualControlAgvRequest
            {
                HasCargo = TargetHasCargo,
                Reason = Reason
            };

            var result = await _agvApiService.ManualControlAsync(AgvId, request);

            if (result != null)
            {
                return true; // 成功
            }
            else
            {
                ErrorMessage = "切换失败";
                return false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"切换失败: {ex.Message}";
            return false;
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
