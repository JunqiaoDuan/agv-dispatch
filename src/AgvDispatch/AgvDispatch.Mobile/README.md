# AGV调度系统 - 移动端App

基于Avalonia UI框架开发的AGV调度系统移动端应用程序。

## 功能特性

### 第一阶段(已完成)
- ✅ 用户登录认证
- ✅ 主页展示(小车状态概览)
- ✅ 小车列表查询
- ✅ 状态统计(总数、在线、运行中、故障)

### 后续规划
- ⏳ 任务管理(创建、查询任务)
- ⏳ 小车控制(暂停、继续)
- ⏳ 异常通知
- ⏳ 实时状态刷新

## 技术架构

### 框架和库
- **UI框架**: Avalonia 11.0.10
- **MVVM框架**: CommunityToolkit.Mvvm 8.2.2
- **HTTP通信**: System.Net.Http
- **依赖注入**: Microsoft.Extensions.DependencyInjection

### 项目结构
```
AgvDispatch.Mobile/
├── Views/              # 视图(AXAML)
│   ├── LoginWindow.axaml       # 登录窗口
│   ├── MainWindow.axaml        # 主窗口
│   └── AgvListView.axaml       # 小车列表视图
├── ViewModels/         # 视图模型
│   ├── LoginViewModel.cs       # 登录ViewModel
│   ├── MainWindowViewModel.cs  # 主窗口ViewModel
│   └── AgvListViewModel.cs     # 小车列表ViewModel
├── Services/           # 服务层
│   ├── IAgvApiService.cs       # AGV API接口
│   ├── AgvApiService.cs        # AGV API实现
│   ├── IAuthService.cs         # 认证服务接口
│   ├── AuthService.cs          # 认证服务实现
│   └── NavigationService.cs    # 导航服务
├── Models/             # 数据模型(复用Shared项目)
├── App.axaml           # 应用入口
├── App.axaml.cs        # 应用配置(DI容器)
└── Program.cs          # 主程序入口
```

## 构建和运行

### 开发环境要求
- .NET 8.0 SDK
- Visual Studio 2022 或 Rider

### 桌面运行(测试)
```bash
cd src/AgvDispatch/AgvDispatch.Mobile
dotnet restore
dotnet run
```

### Android打包
```bash
dotnet publish -f net8.0-android -c Release
```

## 配置说明

### API地址配置

**重要提示**：移动端应用现在会自动从 `appsettings.json` 读取 API 地址配置。

编辑 `appsettings.json` 文件：
```json
{
  "ApiSettings": {
    "BaseUrl": "https://your-api-server.com"
  }
}
```

#### 开发环境配置
- **默认值**: `http://localhost:5223`（后端服务的HTTP端口）
- **桌面测试**: 使用 `http://localhost:5223` 或 `https://localhost:7202`（HTTPS端口）
- **手机测试**: 必须使用服务器的实际IP地址，例如：
  - `http://192.168.1.100:5223` （局域网IP，HTTP）
  - `https://192.168.1.100:7202` （局域网IP，HTTPS）
  - `https://your-domain.com` （公网域名）

**注意**: 手机端不能使用 `localhost`，因为 localhost 指向手机设备本身，而不是运行后端服务的服务器。

#### 如何获取服务器IP地址
在服务器上运行以下命令查看IP地址：
- Windows: `ipconfig`
- Linux/Mac: `ifconfig` 或 `ip addr`

选择与手机在同一网络的IP地址（通常是 192.168.x.x）。


## 使用说明

### 登录
- 用户名: 管理员账号
- 密码: 对应密码
- 点击"登录"按钮

### 主页
- 查看小车统计信息(总数、在线、运行中、故障)
- 查看小车列表(编号、名称、状态、电量、位置)
- 点击"刷新"按钮更新数据

## 开发指南

### 添加新页面
1. 在 `Views/` 创建 `.axaml` 和 `.axaml.cs` 文件
2. 在 `ViewModels/` 创建对应的ViewModel
3. 在 `App.axaml.cs` 中注册ViewModel到DI容器
4. 在 `MainWindow.axaml` 的DataTemplates中添加映射

### 调用API
参考 `AgvApiService.cs`:
```csharp
var client = GetHttpClient(); // 自动添加认证Token
var response = await client.GetFromJsonAsync<ApiResponse<T>>("api/endpoint");
```

## 参考文档
- [Avalonia UI 文档](https://docs.avaloniaui.net/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/zh-cn/dotnet/communitytoolkit/mvvm/)
- [架构设计文档](../../../docs/方案-架构设计.md)
