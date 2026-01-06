-- =============================================
-- 脚本名称: 260106-01_CreateTable_Users.sql
-- 说明: 创建用户表及初始数据
-- 数据库: PostgreSQL
-- 创建日期: 2026-01-06
-- =============================================

-- =============================================
-- 用户表 (users)
-- =============================================
CREATE TABLE IF NOT EXISTS users (
    -- BaseEntity 字段
    id UUID PRIMARY KEY,                     -- 用户ID (Guid)
    is_valid BOOLEAN NOT NULL DEFAULT TRUE,  -- 是否有效
    reason_of_invalid TEXT,                  -- 失效原因
    created_by UUID,                         -- 创建人ID
    created_by_name VARCHAR(255),            -- 创建人姓名
    creation_date TIMESTAMPTZ,               -- 创建时间
    modified_by UUID,                        -- 修改人ID
    modified_by_name VARCHAR(255),           -- 修改人姓名
    modified_date TIMESTAMPTZ,               -- 修改时间

    -- User 实体字段
    username VARCHAR(50) NOT NULL UNIQUE,    -- 用户名（唯一）
    password_hash VARCHAR(100) NOT NULL,     -- 密码哈希
    password_salt VARCHAR(50) NOT NULL,      -- 密码盐值
    role VARCHAR(20) NOT NULL DEFAULT 'User', -- 角色: Admin | User
    display_name VARCHAR(100)                -- 显示名称
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_users_role ON users(role);
CREATE INDEX IF NOT EXISTS idx_users_is_valid ON users(is_valid);

-- =============================================
-- 初始用户数据
-- 注意：密码哈希需要通过应用程序生成后替换
-- 初始密码均为: Admin@123
-- =============================================

-- admin01 用户 (管理员)
INSERT INTO users (id, is_valid, creation_date, username, password_hash, password_salt, role, display_name)
VALUES (
    'a0000000-0000-0000-0000-000000000001',
    TRUE,
    NOW(),
    'admin01',
    'dCi7+fxuYWLH+eBvxGzj8UkJlNvkYZyXz+A3Ax/HXKY=',
    '8Oo1SxdM9EsGQX5QhxXvJXwlMyZsYZ2EfGQl/YMQMW0=',
    'Admin',
    '管理员'
) ON CONFLICT (username) DO NOTHING;

-- user01 用户 (普通用户)
INSERT INTO users (id, is_valid, creation_date, username, password_hash, password_salt, role, display_name)
VALUES (
    'a0000000-0000-0000-0000-000000000002',
    TRUE,
    NOW(),
    'user01',
    '6j+kxcN9JYe+VhJ6P1kN0Y8cH3bXJxU3YLGV8YQJnKM=',
    'ZK9yXM7n5LsVQh2RvTwPmJ8cN4dA6eFgHiBjKlM0O1w=',
    'User',
    '操作员'
) ON CONFLICT (username) DO NOTHING;

-- =============================================
-- 脚本执行完成
-- =============================================
