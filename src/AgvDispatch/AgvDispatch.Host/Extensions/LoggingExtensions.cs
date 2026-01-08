using Serilog;
using Serilog.Events;

namespace AgvDispatch.Host.Extensions;

public static class LoggingExtensions
{
    private const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}";
    private const string ConsoleTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// 配置 Serilog 日志系统
    /// 日志分为三个文件：error（仅错误）、info（信息及以上）、debug（全部）
    /// 按月份存放到不同文件夹，每天一个日志文件
    /// </summary>
    public static void ConfigureSerilog(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            // 控制台输出
            .WriteTo.Console(outputTemplate: ConsoleTemplate)
            // Error 日志 - 只记录 Error 和 Fatal
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                .WriteTo.File(
                    GetLogPath("error"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: null,
                    outputTemplate: OutputTemplate))
            // Info 日志 - 记录 Information、Warning、Error、Fatal
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Information)
                .WriteTo.File(
                    GetLogPath("info"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: null,
                    outputTemplate: OutputTemplate))
            // Debug 日志 - 记录所有级别
            .WriteTo.File(
                GetLogPath("debug"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: null,
                outputTemplate: OutputTemplate)
            .CreateLogger();

        // 写入初始化日志，确认各级别日志文件生效
        Log.Debug("========== 日志系统初始化 [Debug] ==========");
        Log.Information("========== 日志系统初始化 [Info] ==========");
        Log.Error("========== 日志系统初始化 [Error] ==========");
    }

    /// <summary>
    /// 按月份创建日志文件夹路径
    /// </summary>
    private static string GetLogPath(string level) => $"logs/{DateTime.Now:yyyy-MM}/{DateTime.Now:yyyyMMdd}-{level}.log";
}
