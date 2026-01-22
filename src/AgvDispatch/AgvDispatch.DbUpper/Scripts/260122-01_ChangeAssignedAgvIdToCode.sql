-- =============================================
-- 脚本名称: 260122-01_ChangeAssignedAgvIdToCode.sql
-- 说明: 将 task_jobs 表的 assigned_agv_id 字段重构为 assigned_agv_code
--       从使用 GUID 外键改为直接存储 AGV 编号（agv_code）
-- 数据库: SQL Server
-- 创建日期: 2026-01-22
-- =============================================

PRINT '=============================================';
PRINT '开始执行 task_jobs 表字段重构: assigned_agv_id -> assigned_agv_code';
PRINT '=============================================';
GO

-- =============================================
-- 第一步：添加新字段 assigned_agv_code
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[task_jobs]')
    AND name = 'assigned_agv_code'
)
BEGIN
    PRINT '添加新字段 assigned_agv_code...';

    ALTER TABLE [dbo].[task_jobs]
    ADD assigned_agv_code NVARCHAR(50) NULL;

    PRINT 'Column assigned_agv_code added successfully';
END
ELSE
BEGIN
    PRINT 'Column assigned_agv_code already exists, skipping...';
END;
GO

-- =============================================
-- 第二步：删除旧索引 idx_task_jobs_assigned_agv
-- =============================================
IF EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'idx_task_jobs_assigned_agv'
    AND object_id = OBJECT_ID('task_jobs')
)
BEGIN
    PRINT '删除旧索引 idx_task_jobs_assigned_agv...';

    DROP INDEX idx_task_jobs_assigned_agv ON [dbo].[task_jobs];

    PRINT '旧索引删除成功';
END
ELSE
BEGIN
    PRINT '旧索引 idx_task_jobs_assigned_agv 不存在，跳过删除';
END;
GO

-- =============================================
-- 第三步：删除旧字段 assigned_agv_id
-- =============================================
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[task_jobs]')
    AND name = 'assigned_agv_id'
)
BEGIN
    PRINT '删除旧字段 assigned_agv_id...';

    ALTER TABLE [dbo].[task_jobs]
    DROP COLUMN assigned_agv_id;

    PRINT '旧字段删除成功';
END
ELSE
BEGIN
    PRINT '旧字段 assigned_agv_id 不存在，跳过删除';
END;
GO

-- =============================================
-- 第四步：创建新索引 idx_task_jobs_assigned_agv_code
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'idx_task_jobs_assigned_agv_code'
    AND object_id = OBJECT_ID('task_jobs')
)
BEGIN
    PRINT '创建新索引 idx_task_jobs_assigned_agv_code...';

    CREATE INDEX idx_task_jobs_assigned_agv_code
    ON [dbo].[task_jobs](assigned_agv_code);

    PRINT '新索引创建成功';
END
ELSE
BEGIN
    PRINT '新索引 idx_task_jobs_assigned_agv_code 已存在，跳过创建';
END;
GO

-- =============================================
-- 脚本执行完成
-- =============================================
PRINT '=============================================';
PRINT 'task_jobs 表字段重构完成！';
PRINT 'assigned_agv_id (UNIQUEIDENTIFIER) -> assigned_agv_code (NVARCHAR(50))';
PRINT '=============================================';
GO
