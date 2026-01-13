-- =============================================
-- 脚本名称: 250105-02_CreateAllTables.sql
-- 说明: 创建AGV调度系统所有数据表
-- 数据库: SQL Server
-- 创建日期: 2026-01-05
-- =============================================

-- =============================================
-- 2. 地图表 (maps)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[maps]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[maps] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 地图ID (Guid)
        is_valid BIT NOT NULL DEFAULT 1,  -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                  -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),            -- 创建人姓名
        creation_date DATETIMEOFFSET(7),               -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),           -- 修改人姓名
        modified_date DATETIMEOFFSET(7),               -- 修改时间

        -- Map 实体字段
        map_code NVARCHAR(50) NOT NULL,           -- 地图编号，如 M001
        display_name NVARCHAR(100) NOT NULL,      -- 地图名称
        description NVARCHAR(MAX),                        -- 描述
        width DECIMAL(10,2) NOT NULL,            -- 宽度 (厘米)
        height DECIMAL(10,2) NOT NULL,           -- 高度 (厘米)
        is_active BIT NOT NULL DEFAULT 1, -- 是否启用
        sort_no INT NOT NULL DEFAULT 0           -- 排序号
    );
END;
GO

-- 创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_maps_map_code' AND object_id = OBJECT_ID('maps'))
    CREATE INDEX idx_maps_map_code ON maps(map_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_maps_is_active' AND object_id = OBJECT_ID('maps'))
    CREATE INDEX idx_maps_is_active ON maps(is_active);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_maps_is_valid' AND object_id = OBJECT_ID('maps'))
    CREATE INDEX idx_maps_is_valid ON maps(is_valid);
GO

-- =============================================
-- 3. 地图节点表 (map_nodes)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[map_nodes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[map_nodes] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 节点ID (Guid)
        is_valid BIT NOT NULL DEFAULT 1,  -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                  -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),            -- 创建人姓名
        creation_date DATETIMEOFFSET(7),               -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),           -- 修改人姓名
        modified_date DATETIMEOFFSET(7),               -- 修改时间

        -- MapNode 实体字段
        map_id UNIQUEIDENTIFIER NOT NULL,                    -- 所属地图ID
        node_code NVARCHAR(50) NOT NULL,          -- 节点编号，如 N001
        display_name NVARCHAR(100) NOT NULL,      -- 节点名称
        x DECIMAL(10,2) NOT NULL,                -- X坐标 (厘米)
        y DECIMAL(10,2) NOT NULL,                -- Y坐标 (厘米)
        remark NVARCHAR(MAX),                             -- 备注
        sort_no INT NOT NULL DEFAULT 0           -- 排序号
    );
END;
GO

-- 创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_map_nodes_map_id' AND object_id = OBJECT_ID('map_nodes'))
    CREATE INDEX idx_map_nodes_map_id ON map_nodes(map_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_map_nodes_node_code' AND object_id = OBJECT_ID('map_nodes'))
    CREATE INDEX idx_map_nodes_node_code ON map_nodes(node_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_map_nodes_is_valid' AND object_id = OBJECT_ID('map_nodes'))
    CREATE INDEX idx_map_nodes_is_valid ON map_nodes(is_valid);
GO

-- =============================================
-- 4. 地图边表 (map_edges)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[map_edges]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[map_edges] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 边ID (Guid)
        is_valid BIT NOT NULL DEFAULT 1,  -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                  -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),            -- 创建人姓名
        creation_date DATETIMEOFFSET(7),               -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),           -- 修改人姓名
        modified_date DATETIMEOFFSET(7),               -- 修改时间

        -- MapEdge 实体字段
        map_id UNIQUEIDENTIFIER NOT NULL,                    -- 所属地图ID
        edge_code NVARCHAR(50) NOT NULL,          -- 边编号，如 E001
        start_node_id UNIQUEIDENTIFIER NOT NULL,             -- 起点节点ID
        end_node_id UNIQUEIDENTIFIER NOT NULL,               -- 终点节点ID
        edge_type INT NOT NULL DEFAULT 10,       -- 边类型: 10=直线, 20=弧线
        is_bidirectional BIT NOT NULL DEFAULT 1, -- 是否双向通行
        arc_via_x DECIMAL(10,2),                 -- 弧线经过点X（弧线时使用）
        arc_via_y DECIMAL(10,2),                 -- 弧线经过点Y（弧线时使用）
        distance DECIMAL(10,2) NOT NULL DEFAULT 0 -- 边长度
    );
END;
GO

-- 创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_map_edges_map_id' AND object_id = OBJECT_ID('map_edges'))
    CREATE INDEX idx_map_edges_map_id ON map_edges(map_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_map_edges_edge_code' AND object_id = OBJECT_ID('map_edges'))
    CREATE INDEX idx_map_edges_edge_code ON map_edges(edge_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_map_edges_start_node_id' AND object_id = OBJECT_ID('map_edges'))
    CREATE INDEX idx_map_edges_start_node_id ON map_edges(start_node_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_map_edges_end_node_id' AND object_id = OBJECT_ID('map_edges'))
    CREATE INDEX idx_map_edges_end_node_id ON map_edges(end_node_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_map_edges_is_valid' AND object_id = OBJECT_ID('map_edges'))
    CREATE INDEX idx_map_edges_is_valid ON map_edges(is_valid);
GO

-- =============================================
-- 5. 路线表 (routes)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[routes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[routes] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 路线ID (Guid)
        is_valid BIT NOT NULL DEFAULT 1,  -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                  -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),            -- 创建人姓名
        creation_date DATETIMEOFFSET(7),               -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),           -- 修改人姓名
        modified_date DATETIMEOFFSET(7),               -- 修改时间

        -- Route 实体字段
        map_id UNIQUEIDENTIFIER NOT NULL,                    -- 所属地图ID
        route_code NVARCHAR(50) NOT NULL,         -- 路线编号，如 R001
        display_name NVARCHAR(100) NOT NULL,      -- 路线名称
        description NVARCHAR(MAX),                        -- 描述
        is_active BIT NOT NULL DEFAULT 1, -- 是否启用
        sort_no INT NOT NULL DEFAULT 0           -- 排序号
    );
END;
GO

-- 创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_routes_map_id' AND object_id = OBJECT_ID('routes'))
    CREATE INDEX idx_routes_map_id ON routes(map_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_routes_route_code' AND object_id = OBJECT_ID('routes'))
    CREATE INDEX idx_routes_route_code ON routes(route_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_routes_is_active' AND object_id = OBJECT_ID('routes'))
    CREATE INDEX idx_routes_is_active ON routes(is_active);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_routes_is_valid' AND object_id = OBJECT_ID('routes'))
    CREATE INDEX idx_routes_is_valid ON routes(is_valid);
GO

-- =============================================
-- 6. 路线段表 (route_segments)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[route_segments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[route_segments] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 路线段ID (Guid)
        is_valid BIT NOT NULL DEFAULT 1,  -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                  -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),            -- 创建人姓名
        creation_date DATETIMEOFFSET(7),               -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),           -- 修改人姓名
        modified_date DATETIMEOFFSET(7),               -- 修改时间

        -- RouteSegment 实体字段
        route_id UNIQUEIDENTIFIER NOT NULL,                  -- 所属路线ID
        edge_id UNIQUEIDENTIFIER NOT NULL,                   -- 引用的边ID
        seq INT NOT NULL,                        -- 顺序号（10、20、30、40等）
        direction INT NOT NULL DEFAULT 1,        -- 行驶方向: 1=正向, 2=反向
        action INT NOT NULL DEFAULT 0,           -- 到达后动作: 0=无动作, 10=停车, 20=装货, 30=卸货
        wait_time INT NOT NULL DEFAULT 0         -- 等待时间（秒）
    );
END;
GO

-- 创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_route_segments_route_id' AND object_id = OBJECT_ID('route_segments'))
    CREATE INDEX idx_route_segments_route_id ON route_segments(route_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_route_segments_edge_id' AND object_id = OBJECT_ID('route_segments'))
    CREATE INDEX idx_route_segments_edge_id ON route_segments(edge_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_route_segments_route_seq' AND object_id = OBJECT_ID('route_segments'))
    CREATE INDEX idx_route_segments_route_seq ON route_segments(route_id, seq);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_route_segments_is_valid' AND object_id = OBJECT_ID('route_segments'))
    CREATE INDEX idx_route_segments_is_valid ON route_segments(is_valid);
GO

-- =============================================
-- 7. 站点表 (stations)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[stations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[stations] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 站点ID (Guid)
        is_valid BIT NOT NULL DEFAULT 1,  -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                  -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),            -- 创建人姓名
        creation_date DATETIMEOFFSET(7),               -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),           -- 修改人姓名
        modified_date DATETIMEOFFSET(7),               -- 修改时间

        -- Station 实体字段
        map_id UNIQUEIDENTIFIER NOT NULL,                    -- 所属地图ID
        node_id UNIQUEIDENTIFIER NOT NULL,                   -- 关联的地图节点ID
        station_code NVARCHAR(50) NOT NULL,       -- 站点编号，如 S001
        display_name NVARCHAR(100) NOT NULL,      -- 站点名称
        station_type INT NOT NULL,               -- 站点类型: 10=取货点, 20=卸货点, 30=充电站, 40=待命点, 90=交叉口防撞等待点
        description NVARCHAR(MAX),                        -- 描述
        sort_no INT NOT NULL DEFAULT 0           -- 排序号
    );
END;
GO

-- 创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_stations_map_id' AND object_id = OBJECT_ID('stations'))
    CREATE INDEX idx_stations_map_id ON stations(map_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_stations_node_id' AND object_id = OBJECT_ID('stations'))
    CREATE INDEX idx_stations_node_id ON stations(node_id);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_stations_station_code' AND object_id = OBJECT_ID('stations'))
    CREATE INDEX idx_stations_station_code ON stations(station_code);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_stations_station_type' AND object_id = OBJECT_ID('stations'))
    CREATE INDEX idx_stations_station_type ON stations(station_type);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_stations_is_valid' AND object_id = OBJECT_ID('stations'))
    CREATE INDEX idx_stations_is_valid ON stations(is_valid);
GO

-- =============================================
-- 脚本执行完成
-- =============================================
