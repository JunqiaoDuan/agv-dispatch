using System.IO;
using System.Text.Json;

namespace AgvDispatch.Simulator;

/// <summary>
/// AGV配置服务，用于读取appsettings.json中的配置
/// </summary>
public class AgvConfigService
{
    private readonly Dictionary<string, string> _agvCredentials;
    private string _mqttHost;
    private int _mqttPort;

    public AgvConfigService()
    {
        _agvCredentials = new Dictionary<string, string>();
        _mqttHost = "localhost";
        _mqttPort = 1883;

        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        try
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (!File.Exists(configPath))
            {
                return;
            }

            var jsonContent = File.ReadAllText(configPath);
            var configDoc = JsonDocument.Parse(jsonContent);

            if (configDoc.RootElement.TryGetProperty("AgvCredentials", out var credentialsElement))
            {
                foreach (var property in credentialsElement.EnumerateObject())
                {
                    _agvCredentials[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }

            if (configDoc.RootElement.TryGetProperty("MqttBroker", out var mqttElement))
            {
                if (mqttElement.TryGetProperty("Host", out var hostElement))
                {
                    _mqttHost = hostElement.GetString() ?? "localhost";
                }
                if (mqttElement.TryGetProperty("Port", out var portElement))
                {
                    _mqttPort = portElement.GetInt32();
                }
            }
        }
        catch
        {
            // 配置加载失败，使用默认值
        }
    }

    /// <summary>
    /// 根据AGV代码获取密码
    /// </summary>
    public string? GetPassword(string agvCode)
    {
        return _agvCredentials.TryGetValue(agvCode, out var password) ? password : null;
    }

    /// <summary>
    /// MQTT Broker主机地址
    /// </summary>
    public string MqttHost => _mqttHost;

    /// <summary>
    /// MQTT Broker端口
    /// </summary>
    public int MqttPort => _mqttPort;
}
