-- =============================================
-- 脚本名称: 260116-01_CreateTable_MqttMessageLogs.sql
-- 说明: 创建MQTT消息日志表
-- 数据库: SQL Server
-- 创建日期: 2026-01-16
-- =============================================

-- =============================================
-- MQTT消息日志表 (mqtt_message_logs)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[mqtt_message_logs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[mqtt_message_logs] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 日志ID (Guid)
        is_valid BIT NOT NULL DEFAULT 1,                     -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                     -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),                       -- 创建人姓名
        creation_date DATETIMEOFFSET(7),                     -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),                      -- 修改人姓名
        modified_date DATETIMEOFFSET(7),                     -- 修改时间

        -- MqttMessageLog 实体字段
        timestamp DATETIMEOFFSET(7) NOT NULL,                -- 消息时间戳
        topic NVARCHAR(500) NOT NULL,                        -- MQTT Topic
        payload NVARCHAR(MAX) NOT NULL,                      -- 消息负载内容（JSON格式）
        client_id NVARCHAR(100),                             -- 客户端ID（发送者）
        qos INT NOT NULL DEFAULT 0,                          -- QoS级别（0/1/2）
        direction INT NOT NULL,                              -- 消息方向（1=Inbound, 2=Outbound）
        agv_code NVARCHAR(50),                               -- AGV编号（从Topic中解析）
        message_type NVARCHAR(100)                           -- 消息类型（从Topic中解析）
    );
END;
GO

-- 创建索引（提升查询性能）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_mqtt_message_logs_timestamp' AND object_id = OBJECT_ID('mqtt_message_logs'))
    CREATE INDEX idx_mqtt_message_logs_timestamp ON mqtt_message_logs(timestamp DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_mqtt_message_logs_topic' AND object_id = OBJECT_ID('mqtt_message_logs'))
    CREATE INDEX idx_mqtt_message_logs_topic ON mqtt_message_logs(topic);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_mqtt_message_logs_agv_code' AND object_id = OBJECT_ID('mqtt_message_logs'))
    CREATE INDEX idx_mqtt_message_logs_agv_code ON mqtt_message_logs(agv_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_mqtt_message_logs_direction' AND object_id = OBJECT_ID('mqtt_message_logs'))
    CREATE INDEX idx_mqtt_message_logs_direction ON mqtt_message_logs(direction);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_mqtt_message_logs_message_type' AND object_id = OBJECT_ID('mqtt_message_logs'))
    CREATE INDEX idx_mqtt_message_logs_message_type ON mqtt_message_logs(message_type);
GO

-- 创建复合索引（常用查询组合）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_mqtt_message_logs_agv_timestamp' AND object_id = OBJECT_ID('mqtt_message_logs'))
    CREATE INDEX idx_mqtt_message_logs_agv_timestamp ON mqtt_message_logs(agv_code, timestamp DESC);
GO

-- =============================================
-- 脚本执行完成
-- =============================================
