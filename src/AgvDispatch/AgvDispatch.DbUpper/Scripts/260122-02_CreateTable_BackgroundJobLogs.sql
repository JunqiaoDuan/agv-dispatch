-- =============================================
-- 脚本名称: 260122-02_CreateTable_BackgroundJobLogs.sql
-- 说明: 创建后台任务执行日志表
-- 数据库: SQL Server
-- 创建日期: 2026-01-22
-- =============================================

-- =============================================
-- 后台任务执行日志表 (background_job_logs)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[background_job_logs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[background_job_logs] (
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

        -- BackgroundJobLog 实体字段
        job_name NVARCHAR(255) NOT NULL,                     -- 任务名称（如 AgvHealthCheckJob）
        job_display_name NVARCHAR(255) NOT NULL,             -- 任务显示名称（如 AGV健康检测）
        execute_time DATETIMEOFFSET(7) NOT NULL,             -- 执行时间
        result INT NOT NULL,                                 -- 执行结果：1=Success, 2=Failed, 3=Skipped
        message NVARCHAR(MAX),                               -- 执行内容描述（如：标记 2 台小车为离线）
        details NVARCHAR(MAX),                               -- 详细信息（JSON格式，记录具体操作的数据）
        affected_count INT NOT NULL DEFAULT 0,               -- 影响的实体数量
        duration_ms BIGINT NOT NULL DEFAULT 0,               -- 执行耗时（毫秒）
        error_message NVARCHAR(MAX)                          -- 错误信息（如果失败）
    );
END;
GO

-- 创建索引（提升查询性能）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_background_job_logs_execute_time' AND object_id = OBJECT_ID('background_job_logs'))
    CREATE INDEX idx_background_job_logs_execute_time ON background_job_logs(execute_time DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_background_job_logs_job_name' AND object_id = OBJECT_ID('background_job_logs'))
    CREATE INDEX idx_background_job_logs_job_name ON background_job_logs(job_name);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_background_job_logs_result' AND object_id = OBJECT_ID('background_job_logs'))
    CREATE INDEX idx_background_job_logs_result ON background_job_logs(result);
GO

-- 创建复合索引（常用查询组合）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_background_job_logs_job_time' AND object_id = OBJECT_ID('background_job_logs'))
    CREATE INDEX idx_background_job_logs_job_time ON background_job_logs(job_name, execute_time DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_background_job_logs_result_time' AND object_id = OBJECT_ID('background_job_logs'))
    CREATE INDEX idx_background_job_logs_result_time ON background_job_logs(result, execute_time DESC);
GO

-- =============================================
-- 脚本执行完成
-- =============================================
