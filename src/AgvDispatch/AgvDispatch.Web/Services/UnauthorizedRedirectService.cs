namespace AgvDispatch.Web.Services;

/// <summary>
/// 用于在检测到401未授权响应时通知UI跳转到登录页面
/// </summary>
public interface IUnauthorizedRedirectService
{
    /// <summary>
    /// 当检测到401时触发的事件
    /// </summary>
    event Action? OnUnauthorized;

    /// <summary>
    /// 触发未授权事件
    /// </summary>
    void NotifyUnauthorized();
}

/// <summary>
/// 未授权重定向服务实现
/// </summary>
public class UnauthorizedRedirectService : IUnauthorizedRedirectService
{
    public event Action? OnUnauthorized;

    public void NotifyUnauthorized()
    {
        OnUnauthorized?.Invoke();
    }
}
