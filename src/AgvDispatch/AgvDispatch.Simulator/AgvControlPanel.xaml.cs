using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Messages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AgvDispatch.Simulator;

public partial class AgvControlPanel : UserControl
{
    private AgvSimulatorClient? _simulatorClient;
    private System.Windows.Threading.DispatcherTimer? _statusTimer;
    private readonly AgvConfigService _configService;

    public AgvControlPanel()
    {
        InitializeComponent();
        _configService = new AgvConfigService();

        // 从配置初始化MQTT broker信息
        TxtBrokerHost.Text = _configService.MqttHost;
        TxtBrokerPort.Text = _configService.MqttPort.ToString();
    }

    public void SetDefaultAgvId(string agvId)
    {
        TxtAgvId.Text = agvId;
    }

    private async void BtnConnect_Click(object sender, RoutedEventArgs e)
    {
        if (_simulatorClient != null && _simulatorClient.IsConnected)
        {
            await _simulatorClient.DisconnectAsync();
            _simulatorClient = null;
            _statusTimer?.Stop();
            BtnConnect.Content = "连接";
            UpdateUI();
            return;
        }

        try
        {
            var agvId = TxtAgvId.Text.Trim();
            var password = TxtPassword.Password;
            var brokerHost = TxtBrokerHost.Text.Trim();
            var brokerPort = int.Parse(TxtBrokerPort.Text.Trim());

            if (string.IsNullOrEmpty(agvId))
            {
                MessageBox.Show("请输入小车ID", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 如果密码为空，尝试从配置文件中读取
            if (string.IsNullOrEmpty(password))
            {
                var configPassword = _configService.GetPassword(agvId);
                if (string.IsNullOrEmpty(configPassword))
                {
                    MessageBox.Show($"未找到小车 {agvId} 的配置密码，请手动输入密码", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                password = configPassword;
            }

            _simulatorClient = new AgvSimulatorClient(agvId, password, brokerHost, brokerPort);

            _simulatorClient.OnLogMessage += (s, msg) =>
            {
                Dispatcher.Invoke(() =>
                {
                    TxtLog.AppendText(msg + Environment.NewLine);
                    TxtLog.ScrollToEnd();
                });
            };

            _simulatorClient.OnStatusChanged += (s, status) =>
            {
                Dispatcher.Invoke(() =>
                {
                    TxtStatus.Text = status;
                    UpdateStatusColor(status);
                });
            };

            _simulatorClient.OnTaskReceived += (s, task) =>
            {
                Dispatcher.Invoke(() =>
                {
                    TxtCurrentTask.Text = $"{task.TaskId} ({task.TaskType})";
                });
            };

            _simulatorClient.OnPathLockResponse += (s, response) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (response.Approved)
                    {
                        TxtLog.AppendText($"锁定申请已批准\n路径: {response.FromStationCode} → {response.ToStationCode}\n");
                        TxtLog.ScrollToEnd();
                    }
                    else
                    {
                        TxtLog.AppendText($"锁定申请被拒绝\n原因: {response.Reason}\n");
                        TxtLog.ScrollToEnd();
                    }
                });
            };

            await _simulatorClient.ConnectAsync();

            BtnConnect.Content = "断开";
            UpdateUI();

            _statusTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _statusTimer.Tick += (s, e) =>
            {
                if (_simulatorClient != null && _simulatorClient.IsConnected)
                {
                    TxtBatteryVoltage.Text = _simulatorClient.BatteryVoltage.ToString("F1");
                    TxtPosition.Text = $"({_simulatorClient.PositionX:F1}, {_simulatorClient.PositionY:F1})";
                }
                else
                {
                    _statusTimer.Stop();
                }
            };
            _statusTimer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"连接失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnSetPosition_Click(object sender, RoutedEventArgs e)
    {
        if (_simulatorClient == null || !_simulatorClient.IsConnected)
        {
            MessageBox.Show("请先连接到服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var x = double.Parse(TxtPositionX.Text.Trim());
            var y = double.Parse(TxtPositionY.Text.Trim());
            var angle = double.Parse(TxtPositionAngle.Text.Trim());
            var batteryVoltage = double.Parse(TxtBatteryVoltageInput.Text.Trim());
            var speed = double.Parse(TxtSpeedInput.Text.Trim());

            var stationCode = string.Empty;
            if (CmbStation.SelectedItem is ComboBoxItem selectedItem)
            {
                stationCode = selectedItem.Tag?.ToString() ?? string.Empty;
            }
            else
            {
                stationCode = CmbStation.Text.Trim();
            }

            _simulatorClient.SetPosition(x, y, angle, stationCode);
            _simulatorClient.SetBatteryVoltage(batteryVoltage);
            _simulatorClient.SetSpeed(speed);

            await _simulatorClient.PublishStatusAsync();

            MessageBox.Show("位置、角度、电压和速度已更新并发送", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"更新失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnReportException_Click(object sender, RoutedEventArgs e)
    {
        if (_simulatorClient == null || !_simulatorClient.IsConnected)
        {
            MessageBox.Show("请先连接到服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var typeItem = (ComboBoxItem)CmbExceptionType.SelectedItem;
            var severityItem = (ComboBoxItem)CmbExceptionSeverity.SelectedItem;

            var exceptionType = (AgvExceptionType)int.Parse(typeItem.Tag.ToString()!);
            var severity = (AgvExceptionSeverity)int.Parse(severityItem.Tag.ToString()!);

            // 优先使用用户输入的消息，如果为空则使用异常类型名称
            var message = string.IsNullOrWhiteSpace(TxtExceptionMessage.Text)
                ? typeItem.Content.ToString()!
                : TxtExceptionMessage.Text.Trim();

            await _simulatorClient.PublishExceptionAsync(exceptionType, severity, message);

            MessageBox.Show("异常已上报", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"上报异常失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtProgressValue != null)
        {
            TxtProgressValue.Text = $"{(int)e.NewValue}%";
        }
    }

    private async void BtnReportProgress_Click(object sender, RoutedEventArgs e)
    {
        if (_simulatorClient == null || !_simulatorClient.IsConnected)
        {
            MessageBox.Show("请先连接到服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var taskId = TxtTaskId.Text.Trim();
        if (string.IsNullOrEmpty(taskId))
        {
            MessageBox.Show("请输入任务ID", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var progress = (int)SliderProgress.Value;
            var statusItem = (ComboBoxItem)CmbTaskStatus.SelectedItem;
            var status = (TaskJobStatus)int.Parse(statusItem.Tag.ToString()!);

            await _simulatorClient.PublishTaskProgressAsync(taskId, progress, status);

            if (progress >= 100)
            {
                TxtCurrentTask.Text = "无";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"上报失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnClearLog_Click(object sender, RoutedEventArgs e)
    {
        TxtLog.Clear();
    }

    private void UpdateUI()
    {
        var isConnected = _simulatorClient?.IsConnected ?? false;

        TxtAgvId.IsEnabled = !isConnected;
        TxtPassword.IsEnabled = !isConnected;
        TxtBrokerHost.IsEnabled = !isConnected;
        TxtBrokerPort.IsEnabled = !isConnected;

        BtnSetPosition.IsEnabled = isConnected;
        BtnReportException.IsEnabled = isConnected;
    }

    private void UpdateStatusColor(string status)
    {
        TxtStatus.Foreground = status switch
        {
            "Idle" => System.Windows.Media.Brushes.Green,
            "Running" => System.Windows.Media.Brushes.Blue,
            "Charging" => System.Windows.Media.Brushes.Orange,
            "Error" => System.Windows.Media.Brushes.Red,
            "Offline" => System.Windows.Media.Brushes.Gray,
            _ => System.Windows.Media.Brushes.Black
        };
    }

    #region 路径锁定功能

    /// <summary>
    /// 申请路径锁定
    /// </summary>
    private async void BtnRequestLock_Click(object sender, RoutedEventArgs e)
    {
        if (_simulatorClient == null || !_simulatorClient.IsConnected)
        {
            MessageBox.Show("请先连接到服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var fromStation = GetSelectedStationCode(CmbLockFromStation);
            var toStation = GetSelectedStationCode(CmbLockToStation);
            var taskId = TxtLockTaskId.Text.Trim();

            if (string.IsNullOrEmpty(fromStation) || string.IsNullOrEmpty(toStation))
            {
                MessageBox.Show("请选择起点和终点站点", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(taskId))
            {
                MessageBox.Show("请输入任务ID", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnRequestLock.IsEnabled = false;
            BtnRequestLock.Content = "申请中...";

            await _simulatorClient.PublishPathLockRequestAsync(fromStation, toStation, taskId);

            TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 已发送锁定请求: {fromStation} → {toStation}\n");
            TxtLog.ScrollToEnd();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"申请锁定失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 申请锁定失败: {ex.Message}\n");
        }
        finally
        {
            BtnRequestLock.IsEnabled = true;
            BtnRequestLock.Content = "申请锁定";
        }
    }

    /// <summary>
    /// 获取选中的站点编码
    /// </summary>
    private string GetSelectedStationCode(ComboBox comboBox)
    {
        if (comboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            return selectedItem.Tag?.ToString() ?? string.Empty;
        }
        return comboBox.Text.Trim();
    }

    #endregion

    public async Task CleanupAsync()
    {
        if (_simulatorClient != null && _simulatorClient.IsConnected)
        {
            await _simulatorClient.DisconnectAsync();
        }
        _statusTimer?.Stop();
    }
}
