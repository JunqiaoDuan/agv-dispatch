-- =============================================
-- 脚本名称: 260113-01_AddCurvatureToMapEdges.sql
-- 说明: 为 map_edges 表添加 curvature 列，支持曲率弧线类型
-- 数据库: PostgreSQL
-- 创建日期: 2026-01-13
-- =============================================

-- =============================================
-- 1. 为 map_edges 表添加 curvature 列
-- =============================================
DO $$
BEGIN
    -- 检查列是否已存在
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_name = 'map_edges'
        AND column_name = 'curvature'
    ) THEN
        -- 添加 curvature 列
        ALTER TABLE map_edges
        ADD COLUMN curvature DECIMAL(6,3);

        -- 添加列注释
        COMMENT ON COLUMN map_edges.curvature IS '曲率值（EdgeType=21时使用，-1到1之间，0为直线，正值向右弯，负值向左弯）';

        RAISE NOTICE 'Column curvature added to map_edges table successfully.';
    ELSE
        RAISE NOTICE 'Column curvature already exists in map_edges table.';
    END IF;
END $$;

-- =============================================
-- 2. 更新 edge_type 列注释（可选）
-- =============================================
COMMENT ON COLUMN map_edges.edge_type IS '边类型: 10=直线, 20=弧线（经过点）, 21=弧线（曲率）';
COMMENT ON COLUMN map_edges.arc_via_x IS '弧线经过点X（EdgeType=20时使用）';
COMMENT ON COLUMN map_edges.arc_via_y IS '弧线经过点Y（EdgeType=20时使用）';

-- =============================================
-- 脚本执行完成
-- =============================================
