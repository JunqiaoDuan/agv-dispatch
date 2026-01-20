-- =============================================
-- 脚本名称: 260120-03_AddBatteryVoltageToAgvs.sql
-- 说明: 给 agvs 表添加 battery_voltage 字段（电池电压真实值）
-- 数据库: SQL Server
-- 创建日期: 2026-01-20
-- =============================================

-- =============================================
-- 添加 battery_voltage 字段到 agvs 表
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[agvs]')
    AND name = 'battery_voltage'
)
BEGIN
    ALTER TABLE [dbo].[agvs]
    ADD battery_voltage DECIMAL(18, 2) NOT NULL DEFAULT 0;

    PRINT 'Column battery_voltage added to agvs table successfully';
END
ELSE
BEGIN
    PRINT 'Column battery_voltage already exists in agvs table';
END;
GO

-- =============================================
-- 脚本执行完成
-- =============================================
PRINT '=============================================';
PRINT 'agvs 表 battery_voltage 字段添加完成！';
PRINT '=============================================';
GO
