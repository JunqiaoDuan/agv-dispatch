-- =============================================
-- 脚本名称: 260120-01_CreateTable_AgvExceptionLogs.sql
-- 说明: 创建AGV异常日志表
-- 设计原则: 存储MQTT消息原样 + 最小必要的业务字段
-- 数据库: SQL Server
-- 创建日期: 2026-01-20
-- =============================================

-- =============================================
-- AGV异常日志表 (agv_exception_logs)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[agv_exception_logs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[agv_exception_logs] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 日志ID (Guid)
        is_valid BIT NOT NULL DEFAULT 1,                     -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                     -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),                       -- 创建人姓名
        creation_date DATETIMEOFFSET(7),                     -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),                      -- 修改人姓名
        modified_date DATETIMEOFFSET(7),                     -- 修改时间

        -- ========== MQTT消息原样字段 ==========
        agv_code NVARCHAR(50) NOT NULL,                      -- 小车编号 (来自MQTT)
        task_id NVARCHAR(100),                               -- 关联的任务ID (来自MQTT，字符串格式，可选)
        exception_type INT NOT NULL,                         -- 异常类型: 10=障碍物, 20=低电量, 30=网络异常, 31=GPS异常, 40=急停, 80=其他
        severity INT NOT NULL,                               -- 严重级别: 10=Info, 20=Warning, 30=Error, 40=Critical
        message NVARCHAR(MAX),                               -- 异常消息
        position_x DECIMAL(10,2),                            -- 发生时的X坐标 (厘米，来自MQTT的position，可能为null)
        position_y DECIMAL(10,2),                            -- 发生时的Y坐标 (厘米，来自MQTT的position，可能为null)
        position_angle DECIMAL(5,2),                         -- 发生时的朝向角度 (0-360度，来自MQTT的position，可能为null)
        station_code NVARCHAR(100),                            -- 发生时所在站点ID (来自MQTT的position.stationId，字符串格式，可能为null)
        exception_time DATETIMEOFFSET(7) NOT NULL,           -- 异常发生时间

        -- ========== 业务必要字段 ==========
        error_code NVARCHAR(100),                            -- 错误码 (系统生成，用于标识和追溯)
        is_resolved BIT NOT NULL DEFAULT 0,                  -- 是否已处理
        resolved_time DATETIMEOFFSET(7),                     -- 处理时间
        resolved_remark NVARCHAR(MAX)                        -- 处理备注
    );
END;
GO

-- 创建索引（提升查询性能）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agv_exception_logs_agv_code' AND object_id = OBJECT_ID('agv_exception_logs'))
    CREATE INDEX idx_agv_exception_logs_agv_code ON agv_exception_logs(agv_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agv_exception_logs_task_id' AND object_id = OBJECT_ID('agv_exception_logs'))
    CREATE INDEX idx_agv_exception_logs_task_id ON agv_exception_logs(task_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agv_exception_logs_exception_time' AND object_id = OBJECT_ID('agv_exception_logs'))
    CREATE INDEX idx_agv_exception_logs_exception_time ON agv_exception_logs(exception_time DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agv_exception_logs_severity' AND object_id = OBJECT_ID('agv_exception_logs'))
    CREATE INDEX idx_agv_exception_logs_severity ON agv_exception_logs(severity);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agv_exception_logs_exception_type' AND object_id = OBJECT_ID('agv_exception_logs'))
    CREATE INDEX idx_agv_exception_logs_exception_type ON agv_exception_logs(exception_type);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agv_exception_logs_is_resolved' AND object_id = OBJECT_ID('agv_exception_logs'))
    CREATE INDEX idx_agv_exception_logs_is_resolved ON agv_exception_logs(is_resolved);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agv_exception_logs_station_code' AND object_id = OBJECT_ID('agv_exception_logs'))
    CREATE INDEX idx_agv_exception_logs_station_code ON agv_exception_logs(station_code);
GO

-- 创建复合索引（常用查询组合）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agv_exception_logs_agv_time' AND object_id = OBJECT_ID('agv_exception_logs'))
    CREATE INDEX idx_agv_exception_logs_agv_time ON agv_exception_logs(agv_code, exception_time DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agv_exception_logs_severity_resolved' AND object_id = OBJECT_ID('agv_exception_logs'))
    CREATE INDEX idx_agv_exception_logs_severity_resolved ON agv_exception_logs(severity, is_resolved);
GO

-- =============================================
-- 脚本执行完成
-- =============================================
