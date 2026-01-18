-- =============================================
-- 脚本名称: 260118-02_AddHasCargoToAgvs.sql
-- 说明: 给 agvs 表添加 has_cargo 字段
-- 数据库: SQL Server
-- 创建日期: 2026-01-18
-- =============================================

-- =============================================
-- 添加 has_cargo 字段到 agvs 表
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[agvs]')
    AND name = 'has_cargo'
)
BEGIN
    ALTER TABLE [dbo].[agvs]
    ADD has_cargo BIT NOT NULL DEFAULT 0;

    PRINT 'Column has_cargo added to agvs table successfully';
END
ELSE
BEGIN
    PRINT 'Column has_cargo already exists in agvs table';
END;
GO

-- =============================================
-- 创建索引
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agvs_has_cargo' AND object_id = OBJECT_ID('agvs'))
    CREATE INDEX idx_agvs_has_cargo ON agvs(has_cargo);
GO

-- =============================================
-- 脚本执行完成
-- =============================================
PRINT '=============================================';
PRINT 'agvs 表 has_cargo 字段添加完成！';
PRINT '=============================================';
GO
