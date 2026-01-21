using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 设置对话框ViewModel
/// </summary>
public partial class SettingsDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _autoRefresh;

    [ObservableProperty]
    private int _refreshIntervalSeconds;

    public SettingsDialogViewModel()
    {
    }

    /// <summary>
    /// 初始化设置
    /// </summary>
    public void Initialize(bool autoRefresh, int refreshIntervalSeconds)
    {
        AutoRefresh = autoRefresh;
        RefreshIntervalSeconds = refreshIntervalSeconds;
    }
}
