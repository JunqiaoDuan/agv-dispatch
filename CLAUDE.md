# 项目架构

## 项目概述
AgvDispatch 是一个 AGV（自动导引车）调度管理系统，采用分层架构设计，支持 Web 端和移动端多平台访问。

## 技术栈
- **.NET 8.0** - 核心框架
- **Entity Framework Core** - ORM，使用 SQL Server 数据库
- **MQTTnet** - MQTT 消息通信
- **JWT** - 身份认证
- **Blazor + MudBlazor** - Web UI
- **Avalonia** - 跨平台移动客户端
- **WPF** - AGV 模拟器
- **Ardalis.Specification** - 仓储规约模式
- **Serilog** - 日志记录

## 项目结构

### 1. AgvDispatch.Host
- **类型**: ASP.NET Core Web API
- **职责**: 应用程序主机，提供 RESTful API 和托管 Blazor Web UI
- **依赖**: Infrastructure, Web
- **主要功能**:
  - API 控制器
  - JWT 认证配置
  - MQTT Broker 服务托管
  - Swagger API 文档

### 2. AgvDispatch.Web
- **类型**: Blazor Razor 类库
- **职责**: Web 用户界面
- **依赖**: Shared, Business
- **技术**: Blazor + MudBlazor
- **主要功能**:
  - 任务监控面板
  - AGV 监控面板
  - 地图管理
  - 路径管理

### 3. AgvDispatch.Mobile
- **类型**: Avalonia 跨平台应用
- **职责**: 移动端客户端（支持 Windows/Linux/macOS/Android）
- **依赖**: Shared
- **技术**: Avalonia + ReactiveUI + MVVM
- **主要功能**:
  - AGV 状态监控
  - 任务监控
  - 移动端操作界面

### 4. AgvDispatch.Business
- **类型**: 类库
- **职责**: 业务逻辑层
- **依赖**: Shared
- **主要内容**:
  - 领域实体（Entities）
  - 业务服务（Services）
  - 规约定义（Specifications）
  - 仓储接口（Repository Interfaces）

### 5. AgvDispatch.Infrastructure
- **类型**: 类库
- **职责**: 基础设施层
- **依赖**: Business, Shared
- **主要功能**:
  - Entity Framework Core 配置
  - 数据库上下文（DbContext）
  - 仓储实现（Repository）
  - MQTT 消息处理
  - 数据库迁移

### 6. AgvDispatch.Shared
- **类型**: 类库
- **职责**: 共享层
- **主要内容**:
  - DTOs（数据传输对象）
  - 枚举类型
  - 常量定义
  - AGV 模拟器逻辑

### 7. AgvDispatch.Simulator
- **类型**: WPF 应用
- **职责**: AGV 模拟器
- **依赖**: Shared
- **主要功能**:
  - 模拟 AGV 设备
  - MQTT 消息发送
  - 测试和开发辅助工具

### 8. AgvDispatch.DbUpper
- **类型**: 控制台应用
- **职责**: 数据库升级工具
- **主要功能**:
  - 执行数据库迁移
  - 数据库初始化

## 业务领域（聚合根）

### AgvAggregate
- **Agv** - AGV 设备实体
- **AgvExceptionLog** - AGV 异常日志

### TaskAggregate
- **TaskJob** - 任务作业
- **TaskProgressLog** - 任务进度日志

### RouteAggregate
- **TaskRoute** - 任务路径
- **TaskRouteSegment** - 路径段
- **TaskRouteCheckpoint** - 路径检查点

### MapAggregate
- **Map** - 地图
- **MapNode** - 地图节点
- **MapEdge** - 地图边

### StationAggregate
- **Station** - 站点

### UserAggregate
- **User** - 用户

### MqttMessageLogAggregate
- **MqttMessageLog** - MQTT 消息日志

## 架构模式
- **分层架构**: Presentation (Web/Mobile) → Business → Infrastructure
- **DDD**: 采用聚合根设计，领域驱动
- **仓储模式**: 使用 Ardalis.Specification 规约模式
- **CQRS**: 查询和命令分离（通过 DTOs）

## 通信机制
- **HTTP API**: RESTful API（Web/Mobile 与 Host）
- **MQTT**: AGV 设备通信协议
- **SignalR**: 实时数据推送（可选）

# 开发工作流

1. 方案名称是AgvDispatch.slnx，不是AgvDispatch.sln

2. 采用快速失败（Fail Fast）原则，使用早期返回处理边界情况，避免过度防御性编程。

3. 不要硬编码
