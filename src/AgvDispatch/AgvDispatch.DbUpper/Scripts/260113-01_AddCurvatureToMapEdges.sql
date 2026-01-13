-- =============================================
-- 脚本名称: 260113-01_AddCurvatureToMapEdges.sql
-- 说明: 为 map_edges 表添加 curvature 列，支持曲率弧线类型
-- 数据库: SQL Server
-- 创建日期: 2026-01-13
-- =============================================

-- =============================================
-- 1. 为 map_edges 表添加 curvature 列
-- =============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[map_edges]')
    AND name = 'curvature'
)
BEGIN
    -- 添加 curvature 列
    ALTER TABLE map_edges
    ADD curvature DECIMAL(6,3);

    -- 添加列注释
    EXEC sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'曲率值（EdgeType=21时使用，-1到1之间，0为直线，正值向右弯，负值向左弯）',
        @level0type = N'SCHEMA', @level0name = N'dbo',
        @level1type = N'TABLE',  @level1name = N'map_edges',
        @level2type = N'COLUMN', @level2name = N'curvature';

    PRINT 'Column curvature added to map_edges table successfully.';
END
ELSE
BEGIN
    PRINT 'Column curvature already exists in map_edges table.';
END;
GO

-- =============================================
-- 2. 更新 edge_type 列注释（可选）
-- =============================================
EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'边类型: 10=直线, 20=弧线（经过点）, 21=弧线（曲率）',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'map_edges',
    @level2type = N'COLUMN', @level2name = N'edge_type';
GO

EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'弧线经过点X（EdgeType=20时使用）',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'map_edges',
    @level2type = N'COLUMN', @level2name = N'arc_via_x';
GO

EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'弧线经过点Y（EdgeType=20时使用）',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'map_edges',
    @level2type = N'COLUMN', @level2name = N'arc_via_y';
GO

-- =============================================
-- 脚本执行完成
-- =============================================
