-- =============================================
-- 脚本名称: 250105-01_CreateTable_agv.sql
-- 说明: 创建AGV调度系统核心数据表
-- 数据库: PostgreSQL
-- 创建日期: 2026-01-05
-- =============================================

-- =============================================
-- 1. 小车表 (agvs)
-- =============================================
CREATE TABLE IF NOT EXISTS agvs (
    -- BaseEntity 字段
    id UUID PRIMARY KEY,                     -- 小车ID (Guid)
    is_valid BOOLEAN NOT NULL DEFAULT TRUE,  -- 是否有效
    reason_of_invalid TEXT,                  -- 失效原因
    created_by UUID,                         -- 创建人ID
    created_by_name VARCHAR(255),            -- 创建人姓名
    creation_date TIMESTAMPTZ,               -- 创建时间
    modified_by UUID,                        -- 修改人ID
    modified_by_name VARCHAR(255),           -- 修改人姓名
    modified_date TIMESTAMPTZ,               -- 修改时间
    
    -- Agv 实体字段
    agv_code VARCHAR(50) NOT NULL,           -- 小车编号，如 V001
    display_name VARCHAR(100) NOT NULL,      -- 小车名称，如 1号车
    password_hash TEXT NOT NULL,             -- MQTT 连接密码哈希
    password_salt TEXT NOT NULL,             -- 密码盐值
    agv_status INT NOT NULL DEFAULT 0,       -- 运行状态: 0=离线, 10=空闲, 20=执行任务中, 30=充电中, 90=故障
    battery INT NOT NULL DEFAULT 100,        -- 电量百分比 (0-100)
    speed DECIMAL(10,2) NOT NULL DEFAULT 0,  -- 当前速度 (m/s)
    position_x DECIMAL(10,2) NOT NULL DEFAULT 0, -- X坐标 (厘米)
    position_y DECIMAL(10,2) NOT NULL DEFAULT 0, -- Y坐标 (厘米)
    position_angle DECIMAL(5,2) NOT NULL DEFAULT 0, -- 朝向角度 (0-360度，正北为0)
    current_station_id UUID,                 -- 当前所在站点ID
    current_task_id UUID,                    -- 当前执行的任务ID
    error_code VARCHAR(100),                 -- 错误码
    last_online_time TIMESTAMPTZ,            -- 最后在线时间
    sort_no INT NOT NULL DEFAULT 0,          -- 排序号
    description TEXT                         -- 描述
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_agvs_agv_status ON agvs(agv_status);
CREATE INDEX IF NOT EXISTS idx_agvs_agv_code ON agvs(agv_code);
CREATE INDEX IF NOT EXISTS idx_agvs_current_task_id ON agvs(current_task_id);
CREATE INDEX IF NOT EXISTS idx_agvs_current_station_id ON agvs(current_station_id);
CREATE INDEX IF NOT EXISTS idx_agvs_last_online_time ON agvs(last_online_time);
CREATE INDEX IF NOT EXISTS idx_agvs_is_valid ON agvs(is_valid);

-- =============================================
-- 脚本执行完成
-- =============================================

