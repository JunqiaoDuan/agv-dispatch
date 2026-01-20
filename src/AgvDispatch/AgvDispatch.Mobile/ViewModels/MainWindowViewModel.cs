using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 主窗口ViewModel
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableObject? _currentPage;

    [ObservableProperty]
    private string _currentUserName = string.Empty;

    public MainWindowViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;

        try
        {
            var user = _authService.GetCurrentUser();
            CurrentUserName = user?.DisplayName ?? user?.Username ?? "用户";

            // 延迟创建页面，避免构造函数中出现问题
            try
            {
                var agvListViewModel = App.Services.GetRequiredService<AgvListViewModel>();
                CurrentPage = agvListViewModel;

                // 触发数据加载
                agvListViewModel.OnNavigatedTo();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建AgvListViewModel失败: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                throw;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainWindowViewModel初始化失败: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            throw;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();

        var loginWindow = new LoginWindow
        {
            DataContext = App.Services.GetRequiredService<LoginViewModel>()
        };
        _navigationService.NavigateTo(loginWindow);
    }
}
