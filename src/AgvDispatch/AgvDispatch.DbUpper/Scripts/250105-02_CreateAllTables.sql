-- =============================================
-- 脚本名称: 250105-02_CreateAllTables.sql
-- 说明: 创建AGV调度系统所有数据表
-- 数据库: PostgreSQL
-- 创建日期: 2026-01-05
-- =============================================

-- =============================================
-- 2. 地图表 (maps)
-- =============================================
CREATE TABLE IF NOT EXISTS maps (
    -- BaseEntity 字段
    id UUID PRIMARY KEY,                     -- 地图ID (Guid)
    is_valid BOOLEAN NOT NULL DEFAULT TRUE,  -- 是否有效
    reason_of_invalid TEXT,                  -- 失效原因
    created_by UUID,                         -- 创建人ID
    created_by_name VARCHAR(255),            -- 创建人姓名
    creation_date TIMESTAMPTZ,               -- 创建时间
    modified_by UUID,                        -- 修改人ID
    modified_by_name VARCHAR(255),           -- 修改人姓名
    modified_date TIMESTAMPTZ,               -- 修改时间
    
    -- Map 实体字段
    map_code VARCHAR(50) NOT NULL,           -- 地图编号，如 M001
    display_name VARCHAR(100) NOT NULL,      -- 地图名称
    description TEXT,                        -- 描述
    width DECIMAL(10,2) NOT NULL,            -- 宽度 (厘米)
    height DECIMAL(10,2) NOT NULL,           -- 高度 (厘米)
    is_active BOOLEAN NOT NULL DEFAULT TRUE, -- 是否启用
    sort_no INT NOT NULL DEFAULT 0           -- 排序号
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_maps_map_code ON maps(map_code);
CREATE INDEX IF NOT EXISTS idx_maps_is_active ON maps(is_active);
CREATE INDEX IF NOT EXISTS idx_maps_is_valid ON maps(is_valid);

-- =============================================
-- 3. 地图节点表 (map_nodes)
-- =============================================
CREATE TABLE IF NOT EXISTS map_nodes (
    -- BaseEntity 字段
    id UUID PRIMARY KEY,                     -- 节点ID (Guid)
    is_valid BOOLEAN NOT NULL DEFAULT TRUE,  -- 是否有效
    reason_of_invalid TEXT,                  -- 失效原因
    created_by UUID,                         -- 创建人ID
    created_by_name VARCHAR(255),            -- 创建人姓名
    creation_date TIMESTAMPTZ,               -- 创建时间
    modified_by UUID,                        -- 修改人ID
    modified_by_name VARCHAR(255),           -- 修改人姓名
    modified_date TIMESTAMPTZ,               -- 修改时间
    
    -- MapNode 实体字段
    map_id UUID NOT NULL,                    -- 所属地图ID
    node_code VARCHAR(50) NOT NULL,          -- 节点编号，如 N001
    display_name VARCHAR(100) NOT NULL,      -- 节点名称
    x DECIMAL(10,2) NOT NULL,                -- X坐标 (厘米)
    y DECIMAL(10,2) NOT NULL,                -- Y坐标 (厘米)
    remark TEXT,                             -- 备注
    sort_no INT NOT NULL DEFAULT 0           -- 排序号
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_map_nodes_map_id ON map_nodes(map_id);
CREATE INDEX IF NOT EXISTS idx_map_nodes_node_code ON map_nodes(node_code);
CREATE INDEX IF NOT EXISTS idx_map_nodes_is_valid ON map_nodes(is_valid);

-- =============================================
-- 4. 地图边表 (map_edges)
-- =============================================
CREATE TABLE IF NOT EXISTS map_edges (
    -- BaseEntity 字段
    id UUID PRIMARY KEY,                     -- 边ID (Guid)
    is_valid BOOLEAN NOT NULL DEFAULT TRUE,  -- 是否有效
    reason_of_invalid TEXT,                  -- 失效原因
    created_by UUID,                         -- 创建人ID
    created_by_name VARCHAR(255),            -- 创建人姓名
    creation_date TIMESTAMPTZ,               -- 创建时间
    modified_by UUID,                        -- 修改人ID
    modified_by_name VARCHAR(255),           -- 修改人姓名
    modified_date TIMESTAMPTZ,               -- 修改时间
    
    -- MapEdge 实体字段
    map_id UUID NOT NULL,                    -- 所属地图ID
    edge_code VARCHAR(50) NOT NULL,          -- 边编号，如 E001
    start_node_id UUID NOT NULL,             -- 起点节点ID
    end_node_id UUID NOT NULL,               -- 终点节点ID
    edge_type INT NOT NULL DEFAULT 10,       -- 边类型: 10=直线, 20=弧线
    is_bidirectional BOOLEAN NOT NULL DEFAULT TRUE, -- 是否双向通行
    arc_via_x DECIMAL(10,2),                 -- 弧线经过点X（弧线时使用）
    arc_via_y DECIMAL(10,2),                 -- 弧线经过点Y（弧线时使用）
    distance DECIMAL(10,2) NOT NULL DEFAULT 0 -- 边长度
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_map_edges_map_id ON map_edges(map_id);
CREATE INDEX IF NOT EXISTS idx_map_edges_edge_code ON map_edges(edge_code);
CREATE INDEX IF NOT EXISTS idx_map_edges_start_node_id ON map_edges(start_node_id);
CREATE INDEX IF NOT EXISTS idx_map_edges_end_node_id ON map_edges(end_node_id);
CREATE INDEX IF NOT EXISTS idx_map_edges_is_valid ON map_edges(is_valid);

-- =============================================
-- 5. 路线表 (routes)
-- =============================================
CREATE TABLE IF NOT EXISTS routes (
    -- BaseEntity 字段
    id UUID PRIMARY KEY,                     -- 路线ID (Guid)
    is_valid BOOLEAN NOT NULL DEFAULT TRUE,  -- 是否有效
    reason_of_invalid TEXT,                  -- 失效原因
    created_by UUID,                         -- 创建人ID
    created_by_name VARCHAR(255),            -- 创建人姓名
    creation_date TIMESTAMPTZ,               -- 创建时间
    modified_by UUID,                        -- 修改人ID
    modified_by_name VARCHAR(255),           -- 修改人姓名
    modified_date TIMESTAMPTZ,               -- 修改时间
    
    -- Route 实体字段
    map_id UUID NOT NULL,                    -- 所属地图ID
    route_code VARCHAR(50) NOT NULL,         -- 路线编号，如 R001
    display_name VARCHAR(100) NOT NULL,      -- 路线名称
    description TEXT,                        -- 描述
    is_active BOOLEAN NOT NULL DEFAULT TRUE, -- 是否启用
    sort_no INT NOT NULL DEFAULT 0           -- 排序号
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_routes_map_id ON routes(map_id);
CREATE INDEX IF NOT EXISTS idx_routes_route_code ON routes(route_code);
CREATE INDEX IF NOT EXISTS idx_routes_is_active ON routes(is_active);
CREATE INDEX IF NOT EXISTS idx_routes_is_valid ON routes(is_valid);

-- =============================================
-- 6. 路线段表 (route_segments)
-- =============================================
CREATE TABLE IF NOT EXISTS route_segments (
    -- BaseEntity 字段
    id UUID PRIMARY KEY,                     -- 路线段ID (Guid)
    is_valid BOOLEAN NOT NULL DEFAULT TRUE,  -- 是否有效
    reason_of_invalid TEXT,                  -- 失效原因
    created_by UUID,                         -- 创建人ID
    created_by_name VARCHAR(255),            -- 创建人姓名
    creation_date TIMESTAMPTZ,               -- 创建时间
    modified_by UUID,                        -- 修改人ID
    modified_by_name VARCHAR(255),           -- 修改人姓名
    modified_date TIMESTAMPTZ,               -- 修改时间
    
    -- RouteSegment 实体字段
    route_id UUID NOT NULL,                  -- 所属路线ID
    edge_id UUID NOT NULL,                   -- 引用的边ID
    seq INT NOT NULL,                        -- 顺序号（10、20、30、40等）
    direction INT NOT NULL DEFAULT 1,        -- 行驶方向: 1=正向, 2=反向
    action INT NOT NULL DEFAULT 0,           -- 到达后动作: 0=无动作, 10=停车, 20=装货, 30=卸货
    wait_time INT NOT NULL DEFAULT 0         -- 等待时间（秒）
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_route_segments_route_id ON route_segments(route_id);
CREATE INDEX IF NOT EXISTS idx_route_segments_edge_id ON route_segments(edge_id);
CREATE INDEX IF NOT EXISTS idx_route_segments_route_seq ON route_segments(route_id, seq);
CREATE INDEX IF NOT EXISTS idx_route_segments_is_valid ON route_segments(is_valid);

-- =============================================
-- 7. 站点表 (stations)
-- =============================================
CREATE TABLE IF NOT EXISTS stations (
    -- BaseEntity 字段
    id UUID PRIMARY KEY,                     -- 站点ID (Guid)
    is_valid BOOLEAN NOT NULL DEFAULT TRUE,  -- 是否有效
    reason_of_invalid TEXT,                  -- 失效原因
    created_by UUID,                         -- 创建人ID
    created_by_name VARCHAR(255),            -- 创建人姓名
    creation_date TIMESTAMPTZ,               -- 创建时间
    modified_by UUID,                        -- 修改人ID
    modified_by_name VARCHAR(255),           -- 修改人姓名
    modified_date TIMESTAMPTZ,               -- 修改时间
    
    -- Station 实体字段
    map_id UUID NOT NULL,                    -- 所属地图ID
    node_id UUID NOT NULL,                   -- 关联的地图节点ID
    station_code VARCHAR(50) NOT NULL,       -- 站点编号，如 S001
    display_name VARCHAR(100) NOT NULL,      -- 站点名称
    station_type INT NOT NULL,               -- 站点类型: 10=取货点, 20=卸货点, 30=充电站, 40=待命点, 90=交叉口防撞等待点
    description TEXT,                        -- 描述
    sort_no INT NOT NULL DEFAULT 0           -- 排序号
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_stations_map_id ON stations(map_id);
CREATE INDEX IF NOT EXISTS idx_stations_node_id ON stations(node_id);
CREATE INDEX IF NOT EXISTS idx_stations_station_code ON stations(station_code);
CREATE INDEX IF NOT EXISTS idx_stations_station_type ON stations(station_type);
CREATE INDEX IF NOT EXISTS idx_stations_is_valid ON stations(is_valid);

-- =============================================
-- 脚本执行完成
-- =============================================

