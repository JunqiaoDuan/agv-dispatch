-- =============================================
-- 脚本名称: 260121-01_RenameCurrentStationIdToCode.sql
-- 说明: 将 agvs 表的 current_station_id 字段重构为 current_station_code
--       从使用 GUID 外键改为直接存储站点编号（station_code）
-- 数据库: SQL Server
-- 创建日期: 2026-01-21
-- =============================================

PRINT '=============================================';
PRINT '开始执行 agvs 表字段重构: current_station_id -> current_station_code';
PRINT '=============================================';
GO

-- =============================================
-- 第一步：添加新字段 current_station_code
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[agvs]')
    AND name = 'current_station_code'
)
BEGIN
    PRINT '添加新字段 current_station_code...';

    ALTER TABLE [dbo].[agvs]
    ADD current_station_code NVARCHAR(50) NULL;

    PRINT 'Column current_station_code added successfully';
END
ELSE
BEGIN
    PRINT 'Column current_station_code already exists, skipping...';
END;
GO

-- =============================================
-- 第三步：删除旧索引 idx_agvs_current_station_id
-- =============================================
IF EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'idx_agvs_current_station_id'
    AND object_id = OBJECT_ID('agvs')
)
BEGIN
    PRINT '删除旧索引 idx_agvs_current_station_id...';

    DROP INDEX idx_agvs_current_station_id ON [dbo].[agvs];

    PRINT '旧索引删除成功';
END
ELSE
BEGIN
    PRINT '旧索引 idx_agvs_current_station_id 不存在，跳过删除';
END;
GO

-- =============================================
-- 第四步：删除旧字段 current_station_id
-- =============================================
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[agvs]')
    AND name = 'current_station_id'
)
BEGIN
    PRINT '删除旧字段 current_station_id...';

    ALTER TABLE [dbo].[agvs]
    DROP COLUMN current_station_id;

    PRINT '旧字段删除成功';
END
ELSE
BEGIN
    PRINT '旧字段 current_station_id 不存在，跳过删除';
END;
GO

-- =============================================
-- 第五步：创建新索引 idx_agvs_current_station_code
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'idx_agvs_current_station_code'
    AND object_id = OBJECT_ID('agvs')
)
BEGIN
    PRINT '创建新索引 idx_agvs_current_station_code...';

    CREATE INDEX idx_agvs_current_station_code
    ON [dbo].[agvs](current_station_code);

    PRINT '新索引创建成功';
END
ELSE
BEGIN
    PRINT '新索引 idx_agvs_current_station_code 已存在，跳过创建';
END;
GO

-- =============================================
-- 脚本执行完成
-- =============================================
PRINT '=============================================';
PRINT 'agvs 表字段重构完成！';
PRINT 'current_station_id (UNIQUEIDENTIFIER) -> current_station_code (NVARCHAR(50))';
PRINT '=============================================';
GO
