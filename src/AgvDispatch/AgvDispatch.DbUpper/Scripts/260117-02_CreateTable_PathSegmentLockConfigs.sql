-- =============================================
-- 脚本名称: 260117-02_CreateTable_PathSegmentLockConfigs.sql
-- 说明: 创建路径段锁定配置表
-- 数据库: SQL Server
-- 创建日期: 2026-01-17
-- =============================================

-- =============================================
-- 路径段锁定配置表 (path_segment_lock_configs)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[path_segment_lock_configs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[path_segment_lock_configs] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 配置ID (Guid)
        is_valid BIT NOT NULL,                               -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                     -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),                       -- 创建人姓名
        creation_date DATETIMEOFFSET(7),                     -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),                      -- 修改人姓名
        modified_date DATETIMEOFFSET(7),                     -- 修改时间

        -- PathSegmentLockConfig 实体字段
        from_station_code NVARCHAR(50) NOT NULL,             -- 起始站点编号
        to_station_code NVARCHAR(50) NOT NULL,               -- 目标站点编号
        is_lock_required BIT NOT NULL,                       -- 是否需要锁定
        lock_reason NVARCHAR(MAX),                           -- 锁定原因
        timeout_minutes INT NOT NULL,                        -- 超时分钟数
        priority INT,                                        -- 优先级
        is_active BIT NOT NULL                               -- 是否激活
    );

    PRINT 'Table path_segment_lock_configs created successfully';
END;
GO

-- =============================================
-- 创建索引
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_path_segment_lock_configs_stations' AND object_id = OBJECT_ID('path_segment_lock_configs'))
    CREATE INDEX idx_path_segment_lock_configs_stations ON path_segment_lock_configs(from_station_code, to_station_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_path_segment_lock_configs_active' AND object_id = OBJECT_ID('path_segment_lock_configs'))
    CREATE INDEX idx_path_segment_lock_configs_active ON path_segment_lock_configs(is_active);
GO

-- =============================================
-- 脚本执行完成
-- =============================================
PRINT '=============================================';
PRINT '路径段锁定配置表创建完成！';
PRINT '=============================================';
GO
