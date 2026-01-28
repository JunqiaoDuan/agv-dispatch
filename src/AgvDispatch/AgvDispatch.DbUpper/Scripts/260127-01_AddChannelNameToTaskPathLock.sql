-- =============================================
-- 脚本名称: 260127-01_AddChannelNameToTaskPathLock.sql
-- 说明: 为 task_path_locks 表添加 channel_name 列，用于显示通道名称
-- 数据库: SQL Server
-- 创建日期: 2026-01-27
-- =============================================

-- =============================================
-- 1. 为 task_path_locks 表添加 channel_name 列
-- =============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[task_path_locks]')
    AND name = 'channel_name'
)
BEGIN
    -- 添加 channel_name 列
    ALTER TABLE task_path_locks
    ADD channel_name NVARCHAR(50) NULL;

    -- 添加列注释
    EXEC sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'通道名称（用于显示，如：进厂通道、出厂通道、西边窄路上料等）',
        @level0type = N'SCHEMA', @level0name = N'dbo',
        @level1type = N'TABLE',  @level1name = N'task_path_locks',
        @level2type = N'COLUMN', @level2name = N'channel_name';

    PRINT 'Column channel_name added to task_path_locks table successfully.';
END
ELSE
BEGIN
    PRINT 'Column channel_name already exists in task_path_locks table.';
END;
GO

-- =============================================
-- 2. 添加索引以提升查询性能
-- =============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'idx_task_path_locks_channel_status'
    AND object_id = OBJECT_ID(N'[dbo].[task_path_locks]')
)
BEGIN
    CREATE INDEX idx_task_path_locks_channel_status
    ON task_path_locks(channel_name, status)
    WHERE channel_name IS NOT NULL;

    PRINT 'Index idx_task_path_locks_channel_status created successfully.';
END
ELSE
BEGIN
    PRINT 'Index idx_task_path_locks_channel_status already exists.';
END;
GO

-- =============================================
-- 脚本执行完成
-- =============================================
