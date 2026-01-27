-- =============================================
-- 脚本名称: 260124-01_AddUniqueIndexToPathLocks.sql
-- 说明: 为 task_path_locks 表添加字段和唯一约束索引
--       1. 添加 released_time 字段，记录锁定释放时间
--       2. 添加唯一约束索引，确保同一路段只能有一个 Approved 状态的锁定
--       用于路径锁定系统的双向冲突检测
-- 数据库: SQL Server
-- 创建日期: 2026-01-24
-- =============================================

-- =============================================
-- 第一部分：添加 released_time 字段
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[task_path_locks]') AND type in (N'U'))
AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('task_path_locks') AND name = 'released_time')
BEGIN
    ALTER TABLE task_path_locks
    ADD released_time DATETIMEOFFSET(7);

    PRINT 'Column released_time added to task_path_locks successfully';
    PRINT '用于记录路径锁定的释放时间';
END
ELSE IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('task_path_locks') AND name = 'released_time')
BEGIN
    PRINT 'Column released_time already exists in task_path_locks';
END
ELSE
BEGIN
    PRINT 'WARNING: Table task_path_locks does not exist!';
END;
GO
