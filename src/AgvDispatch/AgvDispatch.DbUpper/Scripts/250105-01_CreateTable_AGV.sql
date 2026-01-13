-- =============================================
-- 脚本名称: 250105-01_CreateTable_agv.sql
-- 说明: 创建AGV调度系统核心数据表
-- 数据库: SQL Server
-- 创建日期: 2026-01-05
-- =============================================

-- =============================================
-- 1. 小车表 (agvs)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[agvs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[agvs] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 小车ID (Guid)
        is_valid BIT NOT NULL DEFAULT 1,  -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                  -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),            -- 创建人姓名
        creation_date DATETIMEOFFSET(7),               -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),           -- 修改人姓名
        modified_date DATETIMEOFFSET(7),               -- 修改时间

        -- Agv 实体字段
        agv_code NVARCHAR(50) NOT NULL,           -- 小车编号，如 V001
        display_name NVARCHAR(100) NOT NULL,      -- 小车名称，如 1号车
        password_hash NVARCHAR(MAX) NOT NULL,             -- MQTT 连接密码哈希
        password_salt NVARCHAR(MAX) NOT NULL,             -- 密码盐值
        agv_status INT NOT NULL DEFAULT 0,       -- 运行状态: 0=离线, 10=空闲, 20=执行任务中, 30=充电中, 90=故障
        battery INT NOT NULL DEFAULT 100,        -- 电量百分比 (0-100)
        speed DECIMAL(10,2) NOT NULL DEFAULT 0,  -- 当前速度 (m/s)
        position_x DECIMAL(10,2) NOT NULL DEFAULT 0, -- X坐标 (厘米)
        position_y DECIMAL(10,2) NOT NULL DEFAULT 0, -- Y坐标 (厘米)
        position_angle DECIMAL(5,2) NOT NULL DEFAULT 0, -- 朝向角度 (0-360度，正北为0)
        current_station_id UNIQUEIDENTIFIER,                 -- 当前所在站点ID
        current_task_id UNIQUEIDENTIFIER,                    -- 当前执行的任务ID
        error_code NVARCHAR(100),                 -- 错误码
        last_online_time DATETIMEOFFSET(7),            -- 最后在线时间
        sort_no INT NOT NULL DEFAULT 0,          -- 排序号
        description NVARCHAR(MAX)                         -- 描述
    );
END;
GO

-- 创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agvs_agv_status' AND object_id = OBJECT_ID('agvs'))
    CREATE INDEX idx_agvs_agv_status ON agvs(agv_status);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agvs_agv_code' AND object_id = OBJECT_ID('agvs'))
    CREATE INDEX idx_agvs_agv_code ON agvs(agv_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agvs_current_task_id' AND object_id = OBJECT_ID('agvs'))
    CREATE INDEX idx_agvs_current_task_id ON agvs(current_task_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agvs_current_station_id' AND object_id = OBJECT_ID('agvs'))
    CREATE INDEX idx_agvs_current_station_id ON agvs(current_station_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agvs_last_online_time' AND object_id = OBJECT_ID('agvs'))
    CREATE INDEX idx_agvs_last_online_time ON agvs(last_online_time);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_agvs_is_valid' AND object_id = OBJECT_ID('agvs'))
    CREATE INDEX idx_agvs_is_valid ON agvs(is_valid);
GO

-- =============================================
-- 脚本执行完成
-- =============================================
