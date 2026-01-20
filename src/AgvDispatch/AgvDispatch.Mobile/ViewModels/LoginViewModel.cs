using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgvDispatch.Mobile.Services;
using AgvDispatch.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 登录页面ViewModel
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "请输入用户名和密码";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var (success, message) = await _authService.LoginAsync(Username, Password);

            if (success)
            {
                try
                {
                    // 登录成功，导航到主窗口
                    System.Diagnostics.Debug.WriteLine("登录成功，开始创建主窗口");

                    var mainWindowViewModel = App.Services.GetRequiredService<MainWindowViewModel>();
                    System.Diagnostics.Debug.WriteLine("MainWindowViewModel 创建成功");

                    var mainWindow = new MainWindow
                    {
                        DataContext = mainWindowViewModel
                    };
                    System.Diagnostics.Debug.WriteLine("MainWindow 创建成功，开始导航");

                    _navigationService.NavigateTo(mainWindow);
                    System.Diagnostics.Debug.WriteLine("导航完成");
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"导航到主窗口失败: {navEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {navEx.StackTrace}");
                    ErrorMessage = $"打开主窗口失败: {navEx.Message}";
                }
            }
            else
            {
                ErrorMessage = message ?? "登录失败";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"登录失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
