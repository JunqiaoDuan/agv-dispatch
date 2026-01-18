-- =============================================
-- 脚本名称: 260118-01_AddPriorityToStations.sql
-- 说明: 给 stations 表添加 priority 字段
-- 数据库: SQL Server
-- 创建日期: 2026-01-18
-- =============================================

-- =============================================
-- 添加 priority 字段到 stations 表
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[stations]')
    AND name = 'priority'
)
BEGIN
    ALTER TABLE [dbo].[stations]
    ADD priority INT NOT NULL DEFAULT 0;

    PRINT 'Column priority added to stations table successfully';
END
ELSE
BEGIN
    PRINT 'Column priority already exists in stations table';
END;
GO

-- =============================================
-- 创建索引
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_stations_priority' AND object_id = OBJECT_ID('stations'))
    CREATE INDEX idx_stations_priority ON stations(priority);
GO

-- =============================================
-- 脚本执行完成
-- =============================================
PRINT '=============================================';
PRINT 'stations 表 priority 字段添加完成！';
PRINT '=============================================';
GO
