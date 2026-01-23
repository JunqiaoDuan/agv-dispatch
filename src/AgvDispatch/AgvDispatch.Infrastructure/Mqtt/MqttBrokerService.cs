using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Text;
using System.Text.Json;
using AgvDispatch.Shared.Constants;
using AgvDispatch.Shared.Messages;
using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Business.Entities.MqttMessageLogAggregate;
using AgvDispatch.Shared.Repository;
using AgvDispatch.Business.Specifications.Agvs;
using AgvDispatch.Business.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AgvDispatch.Infrastructure.Mqtt;

/// <summary>
/// MQTT Broker 托管服务
/// 内嵌MQTT Broker，处理小车连接和消息
///
/// 注意：本服务注册为 Singleton，不能直接注入 Scoped 的 DbContext/Repository，
/// 需通过 IServiceScopeFactory 在使用时创建 scope 来获取
/// </summary>
public class MqttBrokerService : IHostedService, IMqttBrokerService, IDisposable
{
    private readonly ILogger<MqttBrokerService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IMqttMessageHandler _messageHandler;
    private MqttServer? _mqttServer;

    public MqttBrokerService(
        ILogger<MqttBrokerService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        IMqttMessageHandler messageHandler)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _messageHandler = messageHandler;
    }

    #region 实现 IHostedService 接口

    /// <summary>
    /// 实现 IHostedService 接口，启动 MQTT Broker
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var port = _configuration.GetValue<int>("Mqtt:Port");

            _logger.LogInformation("[MQTT Broker] 开始启动，端口: {Port}", port);

            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(port);

            var options = optionsBuilder.Build();

            _mqttServer = new MqttFactory().CreateMqttServer(options);

            // 客户端连接验证
            // 触发时机：当客户端（AGV小车）尝试建立连接时触发（在连接建立之前）
            // 作用：
            //    -验证客户端的身份信息（ClientId、用户名、密码等）
            //    -决定是否允许该客户端连接到 MQTT Broker
            //    -可以根据业务规则拒绝或允许连接
            _mqttServer.ValidatingConnectionAsync += OnValidatingConnectionAsync;

            // 消息拦截处理
            // 触发时机：当有消息发布到 Broker 时（无论是客户端发布还是服务端发布）
            // 作用：
            //    - 拦截、检查或修改即将发布的消息
            //    - 可以过滤敏感信息、记录日志、转换消息格式
            //    - 可以阻止某些消息的发布
            _mqttServer.InterceptingPublishAsync += OnInterceptingPublishAsync;

            // 客户端连接事件
            // 触发时机：客户端成功连接后触发（已通过验证）
            _mqttServer.ClientConnectedAsync += OnClientConnectedAsync;

            // 客户端断开事件
            // 触发时机：客户端断开连接时触发（主动断开或异常断开）
            _mqttServer.ClientDisconnectedAsync += OnClientDisconnectedAsync;

            await _mqttServer.StartAsync();

            _logger.LogInformation("[MQTT Broker] 启动成功，端口: {Port}", port);

            // 订阅所有小车的消息（内嵌Broker不需要显式订阅，拦截器会处理所有消息）

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT Broker] 启动失败");
            throw;
        }
    }

    /// <summary>
    /// 实现 IHostedService 接口，停止 MQTT Broker
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttServer != null)
        {
            _logger.LogInformation("[MQTT Broker] 开始停止");
            await _mqttServer.StopAsync();
            _logger.LogInformation("[MQTT Broker] 已停止");
        }
    }

    #endregion

    #region 事件处理

    /// <summary>
    /// 验证客户端连接
    /// </summary>
    private async Task OnValidatingConnectionAsync(ValidatingConnectionEventArgs args)
    {
        try
        {
            var clientId = args.ClientId;
            var username = args.UserName;
            var password = args.Password;

            _logger.LogInformation("客户端连接验证: ClientId={ClientId}, Username={Username}", clientId, username);

            // 验证客户端ID和用户名是否一致
            if (string.IsNullOrEmpty(clientId) || clientId != username)
            {
                _logger.LogError("[MQTT Broker] 客户端 {ClientId} 认证失败: ClientId和Username不一致", clientId);
                args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                return;
            }

            // 从数据库验证小车
            using var scope = _serviceScopeFactory.CreateScope();
            var agvRepository = scope.ServiceProvider.GetRequiredService<IRepository<Agv>>();

            var spec = new AgvByAgvCodeSpec(clientId);
            var agv = await agvRepository.FirstOrDefaultAsync(spec);

            if (agv == null)
            {
                _logger.LogError("[MQTT Broker] 客户端 {ClientId} 认证失败: 小车不存在", clientId);
                args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                return;
            }

            // 验证密码
            var (isValid, message) = agv.VerifyPassword(password);
            if (!isValid)
            {
                _logger.LogWarning("[MQTT Broker] 客户端 {ClientId} 认证失败 - {Message}", clientId, message);
                args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                return;
            }

            _logger.LogInformation("[MQTT Broker] 客户端 {ClientId} 认证成功", clientId);
            args.ReasonCode = MqttConnectReasonCode.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT Broker] 验证客户端连接时发生错误");
            args.ReasonCode = MqttConnectReasonCode.ServerUnavailable;
        }
    }

    /// <summary>
    /// 处理发布的消息
    /// </summary>
    private async Task OnInterceptingPublishAsync(InterceptingPublishEventArgs args)
    {
        try
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

            _logger.LogDebug("[MQTT Broker] 收到消息: Topic={Topic}, Payload={Payload}", topic, payload);

            // 解析 Topic 获取 AgvCode 和消息类型
            var (agvCode, messageType) = MqttTopics.ParseTopic(topic);
            if (string.IsNullOrWhiteSpace(agvCode) || string.IsNullOrWhiteSpace(messageType))
            {
                _logger.LogError("[MQTT Broker] 无法从 Topic {Topic} 中解析 AgvCode 或消息类型", topic);
                return;
            }

            // 持久化消息到数据库（异步不阻塞消息处理）
            _ = Task.Run(async () =>
            {
                _logger.LogDebug("[MQTT Broker] 开始入栈保存消息: Topic={Topic}, Payload={Payload}", topic, payload);
                await SaveMessageLogAsync(
                    topic,
                    payload,
                    args.ClientId,
                    (int)args.ApplicationMessage.QualityOfServiceLevel,
                    MqttMessageDirection.Inbound,
                    agvCode,
                    messageType
                );
            });

            // 根据消息类型精确匹配分发处理
            switch (messageType)
            {
                case MqttTopics.MessageTypeStatus:
                    var statusMessage = JsonSerializer.Deserialize<StatusMessage>(payload);
                    if (statusMessage != null)
                    {
                        await _messageHandler.HandleStatusAsync(agvCode, statusMessage);
                    }
                    break;

                case MqttTopics.MessageTypeTaskProgress:
                    var progressMessage = JsonSerializer.Deserialize<TaskProgressMessage>(payload);
                    if (progressMessage != null)
                    {
                        await _messageHandler.HandleTaskProgressAsync(agvCode, progressMessage);
                    }
                    break;

                case MqttTopics.MessageTypeException:
                    var exceptionMessage = JsonSerializer.Deserialize<ExceptionMessage>(payload);
                    if (exceptionMessage != null)
                    {
                        await _messageHandler.HandleExceptionAsync(agvCode, exceptionMessage);
                    }
                    break;

                default:
                    _logger.LogDebug("[MQTT Broker] 未知的消息类型: {MessageType}", messageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT Broker] 处理消息时发生错误: Topic={Topic}", args.ApplicationMessage.Topic);
        }
    }

    /// <summary>
    /// 客户端已连接事件
    /// </summary>
    private async Task OnClientConnectedAsync(ClientConnectedEventArgs args)
    {
        _logger.LogInformation("[MQTT Broker] 客户端已连接: {ClientId}", args.ClientId);

        // 更新数据库中的小车状态为在线
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var agvRepository = scope.ServiceProvider.GetRequiredService<IRepository<Agv>>();

            var spec = new AgvByAgvCodeSpec(args.ClientId);
            var agv = await agvRepository.FirstOrDefaultAsync(spec);

            if (agv != null)
            {
                agv.AgvStatus = Shared.Enums.AgvStatus.Online;
                agv.LastOnlineTime = DateTimeOffset.UtcNow;
                await agvRepository.UpdateAsync(agv);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT Broker] 更新小车 {ClientId} 在线状态失败", args.ClientId);
        }
    }

    /// <summary>
    /// 客户端已断开事件
    /// </summary>
    private async Task OnClientDisconnectedAsync(ClientDisconnectedEventArgs args)
    {
        _logger.LogInformation("[MQTT Broker] 客户端已断开: {ClientId}, 类型={DisconnectType}",
            args.ClientId, args.DisconnectType);

        // 更新数据库中的小车状态为离线
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var agvRepository = scope.ServiceProvider.GetRequiredService<IRepository<Agv>>();

            var spec = new AgvByAgvCodeSpec(args.ClientId);
            var agv = await agvRepository.FirstOrDefaultAsync(spec);

            if (agv != null)
            {
                agv.AgvStatus = Shared.Enums.AgvStatus.Offline;
                agv.LastOnlineTime = DateTimeOffset.UtcNow;
                await agvRepository.UpdateAsync(agv);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT Broker] 更新小车 {ClientId} 离线状态失败", args.ClientId);
        }
    }

    #endregion

    #region IMqttBrokerService 实现

    /// <summary>
    /// 发布任务下发消息
    /// </summary>
    public async Task PublishTaskAssignAsync(string agvCode, TaskAssignMessage message)
    {
        var topic = MqttTopics.TaskAssign(agvCode);
        var payload = JsonSerializer.Serialize(message);
        await PublishAsync(topic, payload, MqttQualityOfServiceLevel.AtLeastOnce);
    }

    /// <summary>
    /// 发布取消任务消息
    /// </summary>
    public async Task PublishTaskCancelAsync(string agvCode, TaskCancelMessage message)
    {
        var topic = MqttTopics.TaskCancel(agvCode);
        var payload = JsonSerializer.Serialize(message);
        await PublishAsync(topic, payload, MqttQualityOfServiceLevel.AtLeastOnce);
    }

    /// <summary>
    /// 发布控制指令消息
    /// </summary>
    public async Task PublishCommandAsync(string agvCode, CommandMessage message)
    {
        var topic = MqttTopics.Command(agvCode);
        var payload = JsonSerializer.Serialize(message);
        await PublishAsync(topic, payload, MqttQualityOfServiceLevel.AtLeastOnce);
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 保存MQTT消息日志到数据库
    /// </summary>
    private async Task SaveMessageLogAsync(
        string topic,
        string payload,
        string? clientId,
        int qos,
        MqttMessageDirection direction,
        string? agvCode,
        string? messageType)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var logRepository = scope.ServiceProvider.GetRequiredService<IRepository<MqttMessageLog>>();

            var log = new MqttMessageLog
            {
                Timestamp = DateTimeOffset.UtcNow,
                Topic = topic,
                Payload = payload,
                ClientId = clientId,
                Qos = qos,
                Direction = direction,
                AgvCode = agvCode,
                MessageType = messageType
            };

            log.OnCreate();
            await logRepository.AddAsync(log);

            _logger.LogTrace("[MQTT Broker] 消息日志已保存: Topic={Topic}, Direction={Direction}", topic, direction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT Broker] 保存消息日志失败: Topic={Topic}", topic);
        }
    }

    /// <summary>
    /// 发布消息到MQTT
    /// 注意：这是内嵌Broker模式，消息直接注入本地服务器后分发给订阅的客户端，无需指定IP和端口
    /// </summary>
    private async Task PublishAsync(string topic, string payload, MqttQualityOfServiceLevel qos)
    {
        if (_mqttServer == null)
        {
            _logger.LogWarning("[MQTT Broker] 未启动，无法发布消息");
            return;
        }

        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(qos)
                // 不保留消息
                .WithRetainFlag(false)
                .Build();

            await _mqttServer.InjectApplicationMessage(
                new InjectedMqttApplicationMessage(message)
                {
                    SenderClientId = "Server"
                });

            _logger.LogDebug("[MQTT Broker] 已发布消息: Topic={Topic}, Payload={Payload}", topic, payload);

            // 持久化出站消息到数据库（异步不阻塞消息发送）
            var (agvCode, messageType) = MqttTopics.ParseTopic(topic);
            _ = Task.Run(async () =>
            {
                _logger.LogDebug("[MQTT Broker] 开始出栈保存消息: Topic={Topic}, Payload={Payload}", topic, payload);
                await SaveMessageLogAsync(
                    topic,
                    payload,
                    "Server",
                    (int)qos,
                    MqttMessageDirection.Outbound,
                    agvCode,
                    messageType
                );
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT Broker] 发布消息失败: Topic={Topic}", topic);
            throw;
        }
    }

    #endregion

    public void Dispose()
    {
        _mqttServer?.Dispose();
    }
}
