using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;

namespace AgvDispatch.Mobile.Services;

/// <summary>
/// 导航服务接口
/// </summary>
public interface INavigationService
{
    void NavigateTo(Window window);
}

/// <summary>
/// 导航服务实现
/// </summary>
public class NavigationService : INavigationService
{
    public void NavigateTo(Window window)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var oldWindow = desktop.MainWindow;

            // 先设置新窗口为主窗口
            desktop.MainWindow = window;
            window.Show();

            // 然后关闭旧窗口（如果存在）
            oldWindow?.Close();
        }
    }
}
