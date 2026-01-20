using System.Windows;
using System.Windows.Controls;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Simulator;

public partial class MainWindow : Window
{
    private AgvSimulatorClient? _simulatorClient;

    public MainWindow()
    {
        InitializeComponent();
        UpdateUI();
    }

    private async void BtnConnect_Click(object sender, RoutedEventArgs e)
    {
        if (_simulatorClient != null && _simulatorClient.IsConnected)
        {
            // 断开连接
            await _simulatorClient.DisconnectAsync();
            _simulatorClient = null;
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

            if (string.IsNullOrEmpty(agvId) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("请输入小车ID和密码", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 创建模拟器客户端
            _simulatorClient = new AgvSimulatorClient(agvId, password, brokerHost, brokerPort);

            // 订阅事件
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

            // 连接
            await _simulatorClient.ConnectAsync();

            BtnConnect.Content = "断开";
            UpdateUI();

            // 启动状态更新定时器
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, e) =>
            {
                if (_simulatorClient != null && _simulatorClient.IsConnected)
                {
                    TxtBatteryVoltage.Text = _simulatorClient.BatteryVoltage.ToString("F1");
                    TxtPosition.Text = $"({_simulatorClient.PositionX:F1}, {_simulatorClient.PositionY:F1})";
                }
                else
                {
                    timer.Stop();
                }
            };
            timer.Start();
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

            // 获取站点ID
            var stationCode = string.Empty;
            if (CmbStation.SelectedItem is ComboBoxItem selectedItem)
            {
                stationCode = selectedItem.Tag?.ToString() ?? string.Empty;
            }
            else
            {
                // 用户手动输入的情况
                stationCode = CmbStation.Text.Trim();
            }

            _simulatorClient.SetPosition(x, y, angle, stationCode);
            _simulatorClient.SetBatteryVoltage(batteryVoltage);
            _simulatorClient.SetSpeed(speed);

            // 立即发送状态更新到MQTT
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
            var message = typeItem.Content.ToString()!;

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
            await _simulatorClient.PublishTaskProgressAsync(taskId, progress);

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

    protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_simulatorClient != null && _simulatorClient.IsConnected)
        {
            await _simulatorClient.DisconnectAsync();
        }
        base.OnClosing(e);
    }
}
