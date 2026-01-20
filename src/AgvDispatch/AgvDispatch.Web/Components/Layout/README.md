# 布局组件使用指南

本项目包含三种布局组件,用于不同的页面场景:

## 1. MainLayout (主布局)
**路径**: `Components/Layout/MainLayout.razor`

### 使用场景
- 包含完整导航菜单的页面
- 需要侧边栏导航的管理页面
- 系统的主要功能页面

### 功能特性
- ✅ 顶部应用栏
- ✅ 左侧抽屉式导航菜单
- ✅ 用户信息显示
- ✅ 暗亮模式切换
- ✅ 权限验证 (从本地存储读取Token)
- ✅ 未授权自动跳转到登录页
- ✅ 登出功能

### 示例页面
```csharp
@page "/agvs"
@layout MainLayout

// 页面内容
```

---

## 2. ContentLayout (内容布局)
**路径**: `Components/Layout/ContentLayout.razor`

### 使用场景
- **不需要侧边导航菜单的内容页**
- 详情页、编辑页等专注内容的页面
- 全屏展示内容但仍需权限验证的页面

### 功能特性
- ✅ 精简的顶部应用栏
- ❌ 无侧边导航菜单
- ✅ 用户信息显示
- ✅ 暗亮模式切换
- ✅ 权限验证（依赖后端401响应）
- ✅ 未授权自动跳转到登录页
- ✅ 登出功能

### 核心权限验证逻辑
```csharp
protected override void OnInitialized()
{
    // 监听未授权事件(401响应)
    UnauthorizedRedirectService.OnUnauthorized += HandleUnauthorized;
}

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // 获取当前用户信息（仅用于UI显示）
        _currentUser = await AuthStateService.GetCurrentUserAsync();
        StateHasChanged();
    }
}

private void HandleUnauthorized()
{
    InvokeAsync(async () =>
    {
        // 当后端API返回401时，统一处理
        await AuthStateService.LogoutAsync();
        await JS.InvokeVoidAsync("appHelpers.redirectTo", "/login");
    });
}
```

**重要设计说明**：
- ✅ **不主动检查 token** - 避免首次加载时的竞态条件
- ✅ **依赖后端 API 返回 401** - 权限验证由后端统一控制
- ✅ **UnauthorizedRedirectService 统一处理** - 所有布局的未授权处理保持一致
- ✅ **与 MainLayout 保持完全一致** - 确保行为统一可预测

这种设计确保：
1. 首次启动时不会因为 token 加载延迟而误跳转
2. 权限验证由后端 JWT 中间件统一控制
3. 所有页面的认证行为保持一致

### 示例页面
```csharp
@page "/map-studio/{MapId:guid}"
@layout ContentLayout

// 页面内容 - 全屏地图编辑器,不需要侧边菜单
```

---

## 3. EmptyLayout (空布局)
**路径**: `Components/Layout/EmptyLayout.razor`

### 使用场景
- **仅限登录页等公开页面**
- 不需要任何导航和权限验证的页面
- 完全独立的页面(如错误页)

### 功能特性
- ❌ 无顶部栏
- ❌ 无导航菜单
- ❌ 无权限验证
- ✅ 仅包含MudBlazor基础组件 (ThemeProvider, DialogProvider等)

### 示例页面
```csharp
@page "/login"
@layout EmptyLayout

// 登录表单
```

---

## 布局选择决策树

```
是否需要权限验证?
├── 否 → EmptyLayout (登录页、公开页)
└── 是 → 是否需要侧边导航菜单?
    ├── 是 → MainLayout (管理页面、列表页)
    └── 否 → ContentLayout (详情页、编辑器、全屏内容页)
```

---

## 重要提示

### ⚠️ 关键原则
**对于内容页(除了登录页),即使没有菜单功能,也不应该使用EmptyLayout**

应该使用 `ContentLayout`,因为它提供了统一的权限管理：
1. ✅ **后端401统一处理** - 当API返回401时自动跳转登录
2. ✅ **与MainLayout行为一致** - 避免首次加载的竞态条件
3. ✅ **不主动检查token** - 依赖后端JWT中间件验证
4. ✅ **提供一致的用户体验** - 顶部栏、用户菜单、暗亮模式

### 错误示例 ❌
```csharp
// 错误:内容页使用EmptyLayout
@page "/user-profile"
@layout EmptyLayout  // ❌ 没有权限验证!
```

### 正确示例 ✅
```csharp
// 正确:内容页使用ContentLayout
@page "/user-profile"
@layout ContentLayout  // ✅ 有权限验证,无侧边菜单
```

---

## 在页面中指定布局

### 方式1: 使用 @layout 指令
```csharp
@page "/my-page"
@layout ContentLayout

// 页面内容
```

### 方式2: 在 Routes.razor 中设置默认布局
```csharp
<RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
```

---

## 技术实现细节

### Token管理服务
- `IAuthStateService`: 管理Token和用户信息
- `ProtectedLocalStorage`: 加密本地存储
- 静态缓存: 解决HttpClientFactory隔离问题

### 未授权处理
- `IUnauthorizedRedirectService`: 提供未授权事件
- `AuthorizationMessageHandler`: 检测401响应
- 自动清除认证状态并重定向

### 依赖注入
```csharp
@inject IAuthStateService AuthStateService
@inject IUnauthorizedRedirectService UnauthorizedRedirectService
@inject NavigationManager NavigationManager
```

---

## 维护说明

如果需要修改权限验证逻辑,需要同步更新:
1. `MainLayout.razor` - 包含导航菜单的布局
2. `ContentLayout.razor` - 不包含导航菜单的布局

EmptyLayout不需要权限验证,保持简单即可。
