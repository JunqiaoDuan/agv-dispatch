-- =============================================
-- 脚本名称: 260120-02_CreateTable_TaskProgressLogs.sql
-- 说明: 创建任务进度日志表
-- 设计原则: 存储MQTT消息原样，不冗余存储需要查询才能得到的字段
-- 数据库: SQL Server
-- 创建日期: 2026-01-20
-- =============================================

-- =============================================
-- 任务进度日志表 (task_progress_logs)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[task_progress_logs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[task_progress_logs] (
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
        task_id NVARCHAR(100) NOT NULL,                      -- 任务ID (来自MQTT消息，字符串格式)
        agv_code NVARCHAR(50) NOT NULL,                      -- 小车编号 (来自MQTT消息)
        status INT NOT NULL,                                 -- 任务状态: 0=待分配, 10=已分配, 20=执行中, 30=已完成, 40=已取消, 50=失败
        progress_percentage FLOAT,                           -- 进度百分比 (0-100)
        message NVARCHAR(MAX),                               -- 进度消息
        report_time DATETIMEOFFSET(7) NOT NULL               -- 上报时间
    );
END;
GO

-- 创建索引（提升查询性能）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_progress_logs_task_id' AND object_id = OBJECT_ID('task_progress_logs'))
    CREATE INDEX idx_task_progress_logs_task_id ON task_progress_logs(task_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_progress_logs_agv_code' AND object_id = OBJECT_ID('task_progress_logs'))
    CREATE INDEX idx_task_progress_logs_agv_code ON task_progress_logs(agv_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_progress_logs_report_time' AND object_id = OBJECT_ID('task_progress_logs'))
    CREATE INDEX idx_task_progress_logs_report_time ON task_progress_logs(report_time DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_progress_logs_status' AND object_id = OBJECT_ID('task_progress_logs'))
    CREATE INDEX idx_task_progress_logs_status ON task_progress_logs(status);
GO

-- 创建复合索引（常用查询组合）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_progress_logs_task_time' AND object_id = OBJECT_ID('task_progress_logs'))
    CREATE INDEX idx_task_progress_logs_task_time ON task_progress_logs(task_id, report_time DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_progress_logs_agv_time' AND object_id = OBJECT_ID('task_progress_logs'))
    CREATE INDEX idx_task_progress_logs_agv_time ON task_progress_logs(agv_code, report_time DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_task_progress_logs_task_status' AND object_id = OBJECT_ID('task_progress_logs'))
    CREATE INDEX idx_task_progress_logs_task_status ON task_progress_logs(task_id, status);
GO

-- =============================================
-- 脚本执行完成
-- =============================================
