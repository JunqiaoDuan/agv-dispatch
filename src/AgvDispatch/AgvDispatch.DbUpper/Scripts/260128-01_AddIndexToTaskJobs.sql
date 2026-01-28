-- =============================================
-- 脚本名称: 260128-01_AddIndexToTaskJobs.sql
-- 说明: 为 task_jobs 表添加性能索引，优化活动任务查询
--       解决 /api/tasks/active 接口的查询超时问题
--       索引包含: IsValid, TaskStatus, CreationDate (DESC)
-- 数据库: SQL Server
-- 创建日期: 2026-01-28
-- =============================================

-- =============================================
-- 为 task_jobs 表添加复合索引
-- =============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_TaskJobs_IsValid_TaskStatus_CreationDate'
    AND object_id = OBJECT_ID(N'[dbo].[task_jobs]')
)
BEGIN
    -- 创建复合索引：IsValid + TaskStatus + CreationDate
    -- 用于优化活动任务列表查询（Pending/Assigned/Executing 状态）
    CREATE INDEX IX_TaskJobs_IsValid_TaskStatus_CreationDate
    ON task_jobs(is_valid, task_status, creation_date DESC)
    WHERE is_valid = 1;

    PRINT 'Index IX_TaskJobs_IsValid_TaskStatus_CreationDate created successfully.';
    PRINT '该索引将显著提升活动任务查询性能，解决查询超时问题。';
END
ELSE
BEGIN
    PRINT 'Index IX_TaskJobs_IsValid_TaskStatus_CreationDate already exists.';
END;
GO

-- =============================================
-- 脚本执行完成
-- =============================================
