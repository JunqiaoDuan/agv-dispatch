using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;
using AgvDispatch.Shared.Constants;
using AgvDispatch.Shared.Messages;
using AgvDispatch.Shared.Enums;
using TaskJobStatus = AgvDispatch.Shared.Enums.TaskJobStatus;

namespace AgvDispatch.Simulator;

/// <summary>
/// AGV模拟器客户端
/// 模拟真实AGV小车的行为,通过MQTT与服务器通信
/// </summary>
public class AgvSimulatorClient
{
    private readonly string _agvCode;
    private readonly string _password;
    private readonly string _brokerHost;
    private readonly int _brokerPort;
    private IMqttClient? _mqttClient;
    private System.Timers.Timer? _statusTimer;

    // 模拟状态
    private AgvStatus _currentStatus = AgvStatus.Idle;
    private double _batteryVoltage = (double)AgvConstants.MaxBatteryVoltage; // 默认满电电压
    private double _speed = 0;
    private double _positionX = 0;
    private double _positionY = 0;
    private double _positionAngle = 0;
    private string _stationCode = string.Empty;
    private string? _currentTaskId = null;

    // 当前任务信息
    private TaskAssignMessage? _currentTask = null;

    public event EventHandler<string>? OnLogMessage;
    public event EventHandler<string>? OnStatusChanged;
    public event EventHandler<TaskAssignMessage>? OnTaskReceived;

    #region 公开属性

    public string AgvCode => _agvCode;
    public AgvStatus CurrentStatus => _currentStatus;
    public double BatteryVoltage => _batteryVoltage;
    public double PositionX => _positionX;
    public double PositionY => _positionY;
    public bool IsConnected => _mqttClient?.IsConnected ?? false;

    #endregion

    public AgvSimulatorClient(
        string agvCode, 
        string password, 
        string brokerHost, 
        int brokerPort)
    {
        _agvCode = agvCode;
        _password = password;
        _brokerHost = brokerHost;
        _brokerPort = brokerPort;
    }

    /// <summary>
    /// 连接到MQTT Broker
    /// </summary>
    public async Task ConnectAsync()
    {
        try
        {
            Log($"正在连接到 MQTT Broker: {_brokerHost}:{_brokerPort}");

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_brokerHost, _brokerPort)
                .WithClientId(_agvCode)
                .WithCredentials(_agvCode, _password)
                // false: 断线期间消息-保留; 重连后订阅-自动恢复; 适用场景-AGV（推荐）
                .WithCleanSession(false)
                // 协议层心跳（Keep Alive）
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
                .Build();

            // 设置消息接收处理器
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

            // 设置连接断开处理器
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;

            await _mqttClient.ConnectAsync(options);

            Log("已连接到 MQTT Broker");

            // 订阅 Topic
            await SubscribeTopicsAsync();

            // 启动状态上报定时器 (每5秒)
            _statusTimer = new System.Timers.Timer(5000);
            _statusTimer.Elapsed += async (s, e) => await PublishStatusAsync();
            _statusTimer.Start();

            // 立即上报一次状态
            await PublishStatusAsync();
        }
        catch (Exception ex)
        {
            Log($"连接失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public async Task DisconnectAsync()
    {
        _statusTimer?.Stop();
        _statusTimer?.Dispose();

        if (_mqttClient != null && _mqttClient.IsConnected)
        {
            await _mqttClient.DisconnectAsync();
            Log("已断开连接");
        }
    }

    /// <summary>
    /// 订阅相关 Topic
    /// </summary>
    private async Task SubscribeTopicsAsync()
    {
        var topics = new[]
        {
            MqttTopics.TaskAssign(_agvCode),
            MqttTopics.TaskCancel(_agvCode),
            MqttTopics.Command(_agvCode)
        };

        foreach (var topic in topics)
        {
            await _mqttClient!.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());

            Log($"已订阅: {topic}");
        }
    }

    #region 事件处理

    /// <summary>
    /// 处理接收到的消息
    /// </summary>
    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

        Log($"收到消息: {topic}");

        try
        {
            // 解析 Topic 获取 AgvCode 和消息类型
            var (agvCode, messageType) = MqttTopics.ParseTopic(topic);
            if (string.IsNullOrWhiteSpace(messageType))
            {
                Log($"无法解析消息类型: {topic}");
                return;
            }

            // 根据消息类型精确匹配分发处理
            switch (messageType)
            {
                case MqttTopics.MessageTypeTaskAssign:
                    var taskAssignMessage = JsonSerializer.Deserialize<TaskAssignMessage>(payload);
                    if (taskAssignMessage != null)
                    {
                        await HandleTaskAssignAsync(taskAssignMessage);
                    }
                    break;

                case MqttTopics.MessageTypeTaskCancel:
                    var taskCancelMessage = JsonSerializer.Deserialize<TaskCancelMessage>(payload);
                    if (taskCancelMessage != null)
                    {
                        await HandleTaskCancelAsync(taskCancelMessage);
                    }
                    break;

                case MqttTopics.MessageTypeCommand:
                    var commandMessage = JsonSerializer.Deserialize<CommandMessage>(payload);
                    if (commandMessage != null)
                    {
                        await HandleCommandAsync(commandMessage);
                    }
                    break;

                default:
                    Log($"未知的消息类型: {messageType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log($"处理消息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理连接断开
    /// </summary>
    private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        Log($"连接已断开: {args.Reason}");
        UpdateStatus(AgvStatus.Offline);

        // 自动重连 (简单实现,每5秒重试一次)
        await Task.Delay(5000);

        try
        {
            await ConnectAsync();
        }
        catch
        {
            // 重连失败,继续等待下次重试
        }
    }

    #endregion

    #region 任务分发处理

    /// <summary>
    /// 处理任务下发
    /// </summary>
    private async Task HandleTaskAssignAsync(TaskAssignMessage message)
    {
        Log($"收到任务: {message.TaskId}, 类型: {message.TaskType}, 开始站点：{message.StartStationCode}, 目标站点: {message.EndStationCode}");
        OnTaskReceived?.Invoke(this, message);

        _currentTask = message;
        _currentTaskId = message.TaskId;

        // 应答任务已接收
        await PublishTaskProgressAsync(TaskJobStatus.Assigned, "任务已接收,准备执行");

        // 开始执行任务 (模拟)
        _ = Task.Run(async () => await ExecuteTaskAsync());
    }

    /// <summary>
    /// 处理任务取消
    /// </summary>
    private async Task HandleTaskCancelAsync(TaskCancelMessage message)
    {
        Log($"任务取消: {message.TaskId}, 原因: {message.Reason}");

        if (_currentTaskId == message.TaskId)
        {
            _currentTask = null;
            _currentTaskId = null;
            UpdateStatus(AgvStatus.Idle);

            await PublishTaskProgressAsync(TaskJobStatus.Cancelled, $"任务已取消: {message.Reason}");
        }
    }

    /// <summary>
    /// 处理控制指令
    /// </summary>
    private async Task HandleCommandAsync(CommandMessage message)
    {
        Log($"收到指令: {message.CommandType}");

        switch (message.CommandType)
        {
            case CommandType.Pause:
                UpdateStatus(AgvStatus.Idle);
                Log("已暂停");
                break;
            case CommandType.Resume:
                UpdateStatus(AgvStatus.Running);
                Log("已继续");
                break;
            default:
                Log("未知指令");
                break;
        }

        await Task.CompletedTask;
    }

    #endregion

    #region 信息发布

    /// <summary>
    /// 上报状态
    /// </summary>
    private async Task PublishStatusAsync()
    {
        if (_mqttClient == null || !_mqttClient.IsConnected)
            return;

        var message = new StatusMessage
        {
            AgvCode = _agvCode,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Status = _currentStatus,
            BatteryVoltage = _batteryVoltage,
            Speed = _speed,
            Position = new PositionInfo
            {
                X = _positionX,
                Y = _positionY,
                Angle = _positionAngle,
                StationCode = _stationCode,
            },
            CurrentTaskId = _currentTaskId,
            ErrorCode = null
        };

        var topic = MqttTopics.Status(_agvCode);
        var payload = JsonSerializer.Serialize(message);

        await PublishAsync(topic, payload, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce, retain: true);
    }

    /// <summary>
    /// 发布任务进度
    /// </summary>
    private async Task PublishTaskProgressAsync(TaskJobStatus status, string message, double? progressPercentage = null)
    {
        if (_mqttClient == null || !_mqttClient.IsConnected)
            return;

        var progress = new TaskProgressMessage
        {
            AgvCode = _agvCode,
            TaskId = _currentTaskId ?? "",
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Status = status,
            ProgressPercentage = progressPercentage,
            Message = message
        };

        var topic = MqttTopics.TaskProgress(_agvCode);
        var payload = JsonSerializer.Serialize(progress);
        await PublishAsync(topic, payload, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
    }

    /// <summary>
    /// 模拟异常上报
    /// </summary>
    public async Task PublishExceptionAsync(AgvExceptionType type, AgvExceptionSeverity severity, string message)
    {
        if (_mqttClient == null || !_mqttClient.IsConnected)
            return;

        var exception = new ExceptionMessage
        {
            AgvCode = _agvCode,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ExceptionType = type,
            Severity = severity,
            Message = message,
            Position = new PositionInfo
            {
                X = _positionX,
                Y = _positionY,
                Angle = _positionAngle,
                StationCode = _stationCode,
            },
            TaskId = _currentTaskId
        };

        var topic = MqttTopics.Exception(_agvCode);
        var payload = JsonSerializer.Serialize(exception);
        await PublishAsync(topic, payload, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

        Log($"已上报异常: {type} - {message}");
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    private async Task PublishAsync(string topic, string payload,
        MQTTnet.Protocol.MqttQualityOfServiceLevel qos, bool retain = false)
    {
        if (_mqttClient == null || !_mqttClient.IsConnected)
            return;

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(qos)
            .WithRetainFlag(retain)
            .Build();

        await _mqttClient.PublishAsync(message);
    }

    #endregion

    #region 模拟执行任务

    /// <summary>
    /// 执行任务 (模拟)
    /// </summary>
    private async Task ExecuteTaskAsync()
    {
        if (_currentTask == null)
            return;

        UpdateStatus(AgvStatus.Running);
        await PublishTaskProgressAsync(TaskJobStatus.Executing, "开始执行任务", 0);

        // 模拟任务执行过程，分为多个阶段
        var stages = new[]
        {
            (Progress: 10.0, Message: "离开起点站点", Delay: 1000),
            (Progress: 30.0, Message: "正在前往目标站点", Delay: 2000),
            (Progress: 50.0, Message: "行驶中...", Delay: 2000),
            (Progress: 70.0, Message: "接近目标站点", Delay: 2000),
            (Progress: 90.0, Message: "到达目标站点", Delay: 1000),
            (Progress: 100.0, Message: "任务执行完成", Delay: 1000)
        };

        foreach (var stage in stages)
        {
            // 检查任务是否被取消
            if (_currentTask == null || _currentTaskId != _currentTask.TaskId)
            {
                return;
            }

            await Task.Delay(stage.Delay);

            // 模拟移动
            _speed = 0.5;
            Log(stage.Message);

            await PublishTaskProgressAsync(TaskJobStatus.Executing, stage.Message, stage.Progress);

        }

        // 任务完成
        _speed = 0;
        _currentTask = null;
        _currentTaskId = null;
        UpdateStatus(AgvStatus.Idle);

        Log("任务执行完成");
        await PublishTaskProgressAsync(TaskJobStatus.Completed, "任务已完成", 100);
    }

    #endregion

    #region 手动控制方法

    /// <summary>
    /// 设置位置
    /// </summary>
    public void SetPosition(double x, double y, double angle, string stationCode)
    {
        _positionX = x;
        _positionY = y;
        _positionAngle = angle;
        _stationCode = stationCode;
    }

    /// <summary>
    /// 设置电池电压（会自动计算电量百分比）
    /// </summary>
    public void SetBatteryVoltage(double voltage)
    {
        _batteryVoltage = voltage;
    }

    /// <summary>
    /// 手动上报任务进度
    /// </summary>
    public async Task PublishTaskProgressAsync(string taskId, int progress)
    {
        if (_mqttClient == null || !_mqttClient.IsConnected)
            return;

        _currentTaskId = taskId;

        var status = progress >= 100 ? TaskJobStatus.Completed : TaskJobStatus.Executing;
        var message = progress >= 100 ? "任务已完成（手动上报）" : $"任务进度: {progress}%";

        await PublishTaskProgressAsync(status, message, progress);

        if (progress >= 100)
        {
            _currentTask = null;
            _currentTaskId = null;
            UpdateStatus(AgvStatus.Idle);
        }

        Log($"已上报任务进度: {taskId}, 进度: {progress}%");
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 更新状态
    /// </summary>
    private void UpdateStatus(AgvStatus status)
    {
        _currentStatus = status;
        OnStatusChanged?.Invoke(this, status.ToString());
    }

    /// <summary>
    /// 记录日志
    /// </summary>
    private void Log(string message)
    {
        var logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
        OnLogMessage?.Invoke(this, logMessage);
    }

    #endregion

}
