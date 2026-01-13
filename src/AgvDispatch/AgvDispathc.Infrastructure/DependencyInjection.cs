using AgvDispatch.Infrastructure.Db.EF;
using AgvDispatch.Shared.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgvDispatch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册 SQL Server DbContext
        services.AddDbContext<AgvDispatchContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .UseSnakeCaseNamingConvention());

        // 注册泛型仓储
        services.AddScoped(typeof(IRepository<>), typeof(EFRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EFRepository<>));

        return services;
    }
}
