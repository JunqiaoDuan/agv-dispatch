-- =============================================
-- 脚本名称: 260106-01_CreateTable_Users.sql
-- 说明: 创建用户表及初始数据
-- 数据库: SQL Server
-- 创建日期: 2026-01-06
-- =============================================

-- =============================================
-- 用户表 (users)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[users] (
        -- BaseEntity 字段
        id UNIQUEIDENTIFIER PRIMARY KEY,                     -- 用户ID (Guid)
        is_valid BIT NOT NULL DEFAULT 1,  -- 是否有效
        reason_of_invalid NVARCHAR(MAX),                  -- 失效原因
        created_by UNIQUEIDENTIFIER,                         -- 创建人ID
        created_by_name NVARCHAR(255),            -- 创建人姓名
        creation_date DATETIMEOFFSET(7),               -- 创建时间
        modified_by UNIQUEIDENTIFIER,                        -- 修改人ID
        modified_by_name NVARCHAR(255),           -- 修改人姓名
        modified_date DATETIMEOFFSET(7),               -- 修改时间

        -- User 实体字段
        username NVARCHAR(50) NOT NULL UNIQUE,    -- 用户名（唯一）
        password_hash NVARCHAR(100) NOT NULL,     -- 密码哈希
        password_salt NVARCHAR(50) NOT NULL,      -- 密码盐值
        role NVARCHAR(20) NOT NULL DEFAULT 'User', -- 角色: Admin | User
        display_name NVARCHAR(100)                -- 显示名称
    );
END;
GO

-- 创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_users_username' AND object_id = OBJECT_ID('users'))
    CREATE INDEX idx_users_username ON users(username);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_users_role' AND object_id = OBJECT_ID('users'))
    CREATE INDEX idx_users_role ON users(role);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_users_is_valid' AND object_id = OBJECT_ID('users'))
    CREATE INDEX idx_users_is_valid ON users(is_valid);
GO

-- =============================================
-- 初始用户数据
-- 注意：密码哈希需要通过应用程序生成后替换
-- 初始密码均为: Admin@123
-- =============================================

-- admin01 用户 (管理员)
IF NOT EXISTS (SELECT 1 FROM users WHERE username = 'admin01')
BEGIN
    INSERT INTO users (id, is_valid, creation_date, username, password_hash, password_salt, role, display_name)
    VALUES (
        'a0000000-0000-0000-0000-000000000001',
        1,
        SYSDATETIMEOFFSET(),
        'admin01',
        'dCi7+fxuYWLH+eBvxGzj8UkJlNvkYZyXz+A3Ax/HXKY=',
        '8Oo1SxdM9EsGQX5QhxXvJXwlMyZsYZ2EfGQl/YMQMW0=',
        'Admin',
        N'管理员'
    );
END;
GO

-- user01 用户 (普通用户)
IF NOT EXISTS (SELECT 1 FROM users WHERE username = 'user01')
BEGIN
    INSERT INTO users (id, is_valid, creation_date, username, password_hash, password_salt, role, display_name)
    VALUES (
        'a0000000-0000-0000-0000-000000000002',
        1,
        SYSDATETIMEOFFSET(),
        'user01',
        '6j+kxcN9JYe+VhJ6P1kN0Y8cH3bXJxU3YLGV8YQJnKM=',
        'ZK9yXM7n5LsVQh2RvTwPmJ8cN4dA6eFgHiBjKlM0O1w=',
        'User',
        N'操作员'
    );
END;
GO

-- =============================================
-- 脚本执行完成
-- =============================================
