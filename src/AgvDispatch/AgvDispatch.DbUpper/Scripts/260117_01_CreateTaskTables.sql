-- =============================================
-- 脚本名称: 260117_Merged_CreateTaskTables.sql
-- 说明: 合并的任务相关表创建脚本
--       包含以下内容：
--       1. 创建任务相关表(tasks, task_path_locks)
--       2. 删除旧的路线表,创建新的任务路线表(task_routes, task_route_segments, task_route_checkpoints)
--       3. 建立任务与路线的1:1关系
--       4. 重构路径锁定表结构
-- 数据库: SQL Server
-- 创建日期: 2026-01-17
-- =============================================

-- =============================================
-- 第一部分：创建任务表和路径锁定表
-- =============================================

-- =============================================
-- 任务表 (task_jobs)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[task_jobs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[task_jobs] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 任务ID (Guid)
        is_valid BIT NOT NULL,                               -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                     -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),                       -- 创建人姓名
        creation_date DATETIMEOFFSET(7),                     -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),                      -- 修改人姓名
        modified_date DATETIMEOFFSET(7),                     -- 修改时间

        -- Task 实体字段
        task_type INT NOT NULL,                              -- 任务类型(10=召唤上料,20=告知下料,30=确认归位,40=让小车充电)
        task_status INT NOT NULL,                            -- 任务状态(0=Pending,10=Assigned,20=Executing,30=Completed,40=Cancelled,50=Failed)
        priority INT NOT NULL,                               -- 优先级(10最高,50最低,默认30)

        start_station_code NVARCHAR(50) NOT NULL,            -- 起点站点编号
        end_station_code NVARCHAR(50) NOT NULL,              -- 终点站点编号

        assigned_agv_id UNIQUEIDENTIFIER,                    -- 分配的小车ID
        progress_percentage DECIMAL(5,2),                    -- 进度百分比(0-100)
        description NVARCHAR(MAX),                           -- 任务描述

        -- 手动调度版新增字段
        assigned_by UNIQUEIDENTIFIER,                        -- 确认分配的工人ID
        assigned_by_name NVARCHAR(100),                      -- 确认分配的工人姓名

        -- 时间字段
        assigned_at DATETIMEOFFSET(7),                       -- 分配时间
        started_at DATETIMEOFFSET(7),                        -- 开始执行时间
        completed_at DATETIMEOFFSET(7),                      -- 完成时间
        cancelled_at DATETIMEOFFSET(7),                      -- 取消时间

        cancel_reason NVARCHAR(MAX),                         -- 取消原因
        failure_reason NVARCHAR(MAX),                        -- 失败原因
        sort_no INT NOT NULL                                 -- 排序号
    );
END;
GO

-- =============================================
-- 任务路径锁定表 (task_path_locks)
-- 记录路段占用情况,防止路径冲突
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[task_path_locks]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[task_path_locks] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 锁定ID (Guid)
        is_valid BIT NOT NULL,                               -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                     -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),                       -- 创建人姓名
        creation_date DATETIMEOFFSET(7),                     -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),                      -- 修改人姓名
        modified_date DATETIMEOFFSET(7),                     -- 修改时间

        -- TaskPathLock 实体字段
        from_station_code NVARCHAR(50) NOT NULL,             -- 起始站点编号
        to_station_code NVARCHAR(50) NOT NULL,               -- 目标站点编号
        locked_by_agv_id UNIQUEIDENTIFIER NOT NULL,          -- 占用的小车ID
        task_id UNIQUEIDENTIFIER NOT NULL,                   -- 关联任务ID
        status INT NOT NULL,                                 -- 状态(0=Pending,10=Approved,20=Rejected)
        request_time DATETIMEOFFSET(7) NOT NULL,             -- 请求时间
        approved_time DATETIMEOFFSET(7),                     -- 批准时间(null表示被拒绝或超时)
        rejected_reason NVARCHAR(200),                       -- 拒绝原因
        expire_at DATETIMEOFFSET(7)                          -- 过期时间(可选,防止死锁)
    );
END;
GO

-- =============================================
-- 创建任务表索引
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_jobs_task_status' AND object_id = OBJECT_ID('task_jobs'))
    CREATE INDEX idx_task_jobs_task_status ON task_jobs(task_status);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_jobs_assigned_agv' AND object_id = OBJECT_ID('task_jobs'))
    CREATE INDEX idx_task_jobs_assigned_agv ON task_jobs(assigned_agv_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_jobs_creation_date' AND object_id = OBJECT_ID('task_jobs'))
    CREATE INDEX idx_task_jobs_creation_date ON task_jobs(creation_date DESC);
GO

-- =============================================
-- 创建路径锁定表索引
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_path_locks_agv_status' AND object_id = OBJECT_ID('task_path_locks'))
    CREATE INDEX idx_task_path_locks_agv_status ON task_path_locks(locked_by_agv_id, status, request_time DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_path_locks_task' AND object_id = OBJECT_ID('task_path_locks'))
    CREATE INDEX idx_task_path_locks_task ON task_path_locks(task_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_path_locks_expire' AND object_id = OBJECT_ID('task_path_locks'))
    CREATE INDEX idx_task_path_locks_expire ON task_path_locks(expire_at);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_path_locks_status' AND object_id = OBJECT_ID('task_path_locks'))
    CREATE INDEX idx_task_path_locks_status ON task_path_locks(status);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_path_locks_segment_status' AND object_id = OBJECT_ID('task_path_locks'))
    CREATE INDEX idx_task_path_locks_segment_status ON task_path_locks(from_station_code, to_station_code, status);
GO

-- =============================================
-- 第二部分：创建任务路线表
-- =============================================

-- =============================================
-- 任务路线定义表 (task_routes)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[task_routes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[task_routes] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 路线ID (Guid)
        is_valid BIT NOT NULL,                               -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                     -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),                       -- 创建人姓名
        creation_date DATETIMEOFFSET(7),                     -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),                      -- 修改人姓名
        modified_date DATETIMEOFFSET(7),                     -- 修改时间

        -- TaskRoute 实体字段
        task_id UNIQUEIDENTIFIER NOT NULL,                   -- 任务ID
        start_station_code NVARCHAR(50) NOT NULL,            -- 起始站点编号
        end_station_code NVARCHAR(50) NOT NULL,              -- 终点站点编号
        description NVARCHAR(MAX),                           -- 描述
        sort_no INT NOT NULL                                 -- 排序号
    );

    PRINT 'Table task_routes created successfully';
END;
GO

-- =============================================
-- 任务路线段表 (task_route_segments)
-- Web显示路径，用于可视化渲染
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[task_route_segments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[task_route_segments] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 路线段ID (Guid)
        is_valid BIT NOT NULL,                               -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                     -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),                       -- 创建人姓名
        creation_date DATETIMEOFFSET(7),                     -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),                      -- 修改人姓名
        modified_date DATETIMEOFFSET(7),                     -- 修改时间

        -- TaskRouteSegment 实体字段
        task_route_id UNIQUEIDENTIFIER NOT NULL,             -- 路线ID (外键)
        seq INT NOT NULL,                                    -- 序号(10,20,30,...)
        map_edge_id UNIQUEIDENTIFIER NOT NULL,               -- 地图边ID
        direction INT NOT NULL,                              -- 行驶方向(10=Forward正向,20=Backward反向)
        final_action INT NOT NULL                            -- 到达后动作(0=None,10=Stop,20=Load,30=Unload)
    );

    PRINT 'Table task_route_segments created successfully';
END;
GO

-- =============================================
-- 任务路线检查点表 (task_route_checkpoints)
-- AGV对接路径，用于AGV导航，只包含关键站点
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[task_route_checkpoints]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[task_route_checkpoints] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 检查点ID (Guid)
        is_valid BIT NOT NULL,                               -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                     -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),                       -- 创建人姓名
        creation_date DATETIMEOFFSET(7),                     -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),                      -- 修改人姓名
        modified_date DATETIMEOFFSET(7),                     -- 修改时间

        -- TaskRouteCheckpoint 实体字段
        task_route_id UNIQUEIDENTIFIER NOT NULL,             -- 路线ID (外键)
        seq INT NOT NULL,                                    -- 序号(10,20,30,...)
        station_code NVARCHAR(50) NOT NULL,                  -- 站点编号
        checkpoint_type INT NOT NULL,                        -- 检查点类型(10=Waypoint,20=Destination)
        is_lock_required BIT NOT NULL                        -- 是否需要锁定(申请通行权)
    );

    PRINT 'Table task_route_checkpoints created successfully';
END;
GO

-- =============================================
-- 第三部分：建立任务与路线的关系
-- =============================================

-- 创建唯一索引（提升查询性能并强制1:1关系）
IF NOT EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'idx_task_routes_task_id'
    AND object_id = OBJECT_ID('task_routes')
)
BEGIN
    CREATE UNIQUE INDEX idx_task_routes_task_id ON task_routes(task_id);
    PRINT 'Unique index idx_task_routes_task_id created (enforcing 1:1 relationship)';
END;
GO

-- =============================================
-- 创建任务路线表索引
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_routes_start_end' AND object_id = OBJECT_ID('task_routes'))
    CREATE INDEX idx_task_routes_start_end ON task_routes(start_station_code, end_station_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_routes_sort' AND object_id = OBJECT_ID('task_routes'))
    CREATE INDEX idx_task_routes_sort ON task_routes(sort_no);
GO

-- task_route_segments表索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_route_segments_route_seq' AND object_id = OBJECT_ID('task_route_segments'))
    CREATE INDEX idx_task_route_segments_route_seq ON task_route_segments(task_route_id, seq);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_route_segments_edge' AND object_id = OBJECT_ID('task_route_segments'))
    CREATE INDEX idx_task_route_segments_edge ON task_route_segments(map_edge_id);
GO

-- task_route_checkpoints表索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_route_checkpoints_route_seq' AND object_id = OBJECT_ID('task_route_checkpoints'))
    CREATE INDEX idx_task_route_checkpoints_route_seq ON task_route_checkpoints(task_route_id, seq);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_route_checkpoints_station' AND object_id = OBJECT_ID('task_route_checkpoints'))
    CREATE INDEX idx_task_route_checkpoints_station ON task_route_checkpoints(station_code);
GO

-- =============================================
-- 脚本执行完成
-- =============================================
PRINT '=============================================';
PRINT '合并脚本执行完成！';
PRINT '已创建以下表:';
PRINT '  - task_jobs (任务表)';
PRINT '  - task_path_locks (任务路径锁定表)';
PRINT '  - task_routes (任务路线表)';
PRINT '  - task_route_segments (任务路线段表)';
PRINT '  - task_route_checkpoints (任务路线检查点表)';
PRINT '已删除旧表: routes, route_segments';
PRINT '已建立 Task-Route 1:1 关系';
PRINT '=============================================';
GO
