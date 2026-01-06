using AgvDispatch.Host.Middlewares;
using AgvDispatch.Infrastructure;
using AgvDispatch.Web;
using AgvDispatch.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// API 服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册 Infrastructure 服务 (EF Core + PostgreSQL)
builder.Services.AddInfrastructure(builder.Configuration);

// 注册 Blazor Web UI 服务
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001";
builder.Services.AddWebUI(apiBaseUrl);

var app = builder.Build();

// 全局异常处理（必须在最前面）
app.UseGlobalExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// 配置 API
app.MapControllers();

// 配置 Blazor Web UI
app.UseWebUI<App>();

app.Run();
