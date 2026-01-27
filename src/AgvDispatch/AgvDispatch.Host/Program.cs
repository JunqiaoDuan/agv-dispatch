using AgvDispatch.Business.Extensions;
using AgvDispatch.Business.Services;
using AgvDispatch.Host.Extensions;
using AgvDispatch.Host.Middlewares;
using AgvDispatch.Host.Services;
using AgvDispatch.Infrastructure;
using AgvDispatch.Infrastructure.Jobs;
using AgvDispatch.Infrastructure.Mqtt;
using AgvDispatch.Infrastructure.Options;
using AgvDispatch.Shared.Options;
using AgvDispatch.Web;
using AgvDispatch.Web.Components;
using Quartz;
using Serilog;

try
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build();

    LoggingExtensions.ConfigureSerilog(configuration);

    Log.Information("启动 AGV Dispatch 服务...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // API 服务
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // 注册认证服务 (JWT)
    builder.Services.AddAuthServices(builder.Configuration);

    // 注册 Infrastructure 服务 (EF Core + PostgreSQL)
    builder.Services.AddInfrastructure(builder.Configuration);

    // 注册系统配置
    builder.Services.Configure<SystemOptions>(builder.Configuration.GetSection("System"));

    // 注册健康检测配置
    builder.Services.Configure<HealthCheckOptions>(builder.Configuration.GetSection("HealthCheck"));

    // 注册 Quartz 定时任务
    builder.Services.AddQuartz(q =>
    {
        // 配置 AGV 健康检测任务
        var jobKey = new JobKey("AgvHealthCheckJob");
        q.AddJob<AgvHealthCheckJob>(opts => opts.WithIdentity(jobKey));

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("AgvHealthCheckTrigger")
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(builder.Configuration.GetValue<int>("HealthCheck:CheckIntervalSeconds", 30))
                .RepeatForever())
            .StartNow());
    });

    builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

    // 注册任务相关服务
    builder.Services.AddTaskServices();

    // 注册 MQTT 服务
    builder.Services.AddSingleton<IMqttMessageHandler, MqttMessageHandler>();
    builder.Services.AddSingleton<MqttBrokerService>();
    builder.Services.AddSingleton<IMqttBrokerService>(sp => sp.GetRequiredService<MqttBrokerService>());
    builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttBrokerService>());

    // 注册 Blazor Web UI 服务
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001";
    builder.Services.AddWebUI(apiBaseUrl);

    var app = builder.Build();

    // 全局异常处理（必须在最前面）
    app.UseGlobalExceptionHandler();

    // API 请求/响应日志
    app.UseApiRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // 配置 API
    app.MapControllers();

    // 配置 Blazor Web UI
    app.UseWebUI<App>();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用程序启动失败");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
