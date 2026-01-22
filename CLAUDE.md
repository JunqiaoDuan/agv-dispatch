# 项目架构

## 项目概述
AgvDispatch 是一个 AGV（自动导引车）调度管理系统，采用分层架构设计，支持 Web 端和移动端多平台访问。

## 技术栈
**.NET 8.0** | **EF Core (SQL Server)** | **MQTTnet** | **JWT** | **Blazor/MudBlazor** | **Avalonia** | **Ardalis.Specification** | **Serilog**

## 项目结构

| 项目 | 类型 | 职责 | 主要依赖 |
|------|------|------|----------|
| **Host** | Web API | 应用主机，提供 RESTful API 和托管 Blazor Web UI | Infrastructure, Web |
| **Web** | Blazor 库 | Web 用户界面（任务/AGV 监控、地图/路径管理） | Shared, Business |
| **Mobile** | Avalonia | 跨平台移动客户端（MVVM + ReactiveUI） | Shared |
| **Business** | 类库 | 业务逻辑层（实体、服务、规约、仓储接口） | Shared |
| **Infrastructure** | 类库 | 基础设施层（EF Core、仓储实现、MQTT） | Business, Shared |
| **Shared** | 类库 | 共享层（DTOs、枚举、常量） | - |
| **Simulator** | WPF | AGV 模拟器 | Shared |
| **DbUpper** | 控制台 | 数据库升级工具 | Infrastructure |

## 业务领域（聚合根）
**AgvAggregate**: Agv, AgvExceptionLog
**TaskAggregate**: TaskJob, TaskProgressLog
**RouteAggregate**: TaskRoute, TaskRouteSegment, TaskRouteCheckpoint
**MapAggregate**: Map, MapNode, MapEdge
**StationAggregate**: Station
**UserAggregate**: User
**MqttMessageLogAggregate**: MqttMessageLog

## 架构模式
**分层架构**: Presentation (Web/Mobile) → Business → Infrastructure | **DDD**: 聚合根设计 | **仓储模式**: Ardalis.Specification 规约

## 通信机制
**HTTP API**: RESTful (Web/Mobile ↔ Host) | **MQTT**: AGV 设备通信 | **SignalR**: 实时推送（可选）

# 开发约定与规范

## 1. 解决方案文件
- 项目使用 **AgvDispatch.slnx**（新格式），而非 AgvDispatch.sln
- 执行构建、测试等命令时，确保使用正确的解决方案文件

## 2. 代码风格：快速失败（Fail Fast）
优先使用早期返回，避免深层嵌套：
```csharp
// ✅ 推荐：早期返回
if (task == null) return;
if (task.Status != TaskStatus.Pending) return;
ExecuteTask(task);

// ❌ 避免：深层嵌套
if (task != null) {
    if (task.Status == TaskStatus.Pending) { ... }
}
```

## 3. 避免硬编码
使用常量、枚举、配置文件代替魔法值：
```csharp
// ✅ 推荐
public const int MaxRetryCount = 3;
public enum TaskStatus { Pending, Running, Completed }

// ❌ 避免
if (retryCount > 3) { ... }
if (status == "pending") { ... }
```

## 4. 命名规范
- **实体类**: 使用领域术语（如 `Agv`、`TaskJob`、`Station`）
- **DTOs**: 添加 `Dto` 后缀（如 `AgvDto`、`TaskJobDto`）
- **服务类**: 添加 `Service` 后缀（如 `TaskService`、`AgvService`）
- **仓储类**: 遵循 `IRepository<TEntity>` 模式

## 5. 异常处理原则
Web 端已配置全局异常处理，仅在以下场景使用 try-catch：
- 需要释放特定资源（文件、连接等）
- 需要转换或包装异常类型
- 需要从异常中恢复并继续执行

```csharp
// ✅ 推荐：让全局异常处理器处理
public async Task<AgvDto> GetAgvAsync(int id)
{
    var agv = await _repository.GetByIdAsync(id);
    if (agv == null) throw new NotFoundException($"AGV {id} not found");
    return _mapper.Map<AgvDto>(agv);
}

// ❌ 避免：不必要的 try-catch
try {
    var agv = await _repository.GetByIdAsync(id);
    return _mapper.Map<AgvDto>(agv);
} catch (Exception ex) {
    _logger.LogError(ex, "Error getting AGV"); // 全局处理器已经会做
    throw;
}
```

## 6. 服务方法返回值设计
对于可能失败的操作，服务方法应返回详细的错误信息，便于调用方处理：

```csharp
// ✅ 推荐：返回元组，包含成功标志和错误消息
public async Task<(bool Success, string? Message)> CancelTaskAsync(Guid taskId, string? reason, Guid? userId)
{}
```

## 99. 文档更新原则
**当学习到新的项目规则、模式或约定时，立即更新此 CLAUDE.md 文件，确保文档始终反映项目最新实践。**
