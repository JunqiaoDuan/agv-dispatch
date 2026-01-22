using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.BackgroundJobLogAggregate;
using AgvDispatch.Business.Entities.MapAggregate;
using AgvDispatch.Business.Entities.MqttMessageLogAggregate;
using AgvDispatch.Business.Entities.RouteAggregate;
using AgvDispatch.Business.Entities.StationAggregate;
using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Business.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AgvDispatch.Infrastructure.Db.EF;

public class AgvDispatchContext : DbContext
{
    public AgvDispatchContext()
    {
    }

    public AgvDispatchContext(DbContextOptions<AgvDispatchContext> options)
        : base(options)
    {
    }

    #region Tables

    public DbSet<Map> Maps { get; set; }
    public DbSet<MapNode> MapNodes { get; set; }
    public DbSet<MapEdge> MapEdges { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<TaskRoute> TaskRoutes { get; set; }
    public DbSet<TaskRouteSegment> TaskRouteSegments { get; set; }
    public DbSet<TaskRouteCheckpoint> TaskRouteCheckpoints { get; set; }
    public DbSet<TaskProgressLog> TaskProgressLogs { get; set; }
    public DbSet<Agv> Agvs { get; set; }
    public DbSet<AgvExceptionLog> AgvExceptionLogs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<MqttMessageLog> MqttMessageLogs { get; set; }
    public DbSet<BackgroundJobLog> BackgroundJobLogs { get; set; }
    public DbSet<TaskJob> TaskJobs { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
