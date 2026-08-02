-- =============================================
-- EventBride Database Creation
-- =============================================

-- برای هر سرویس یک دیتابیس جداگانه داریم
-- این اسکریپت دیتابیس Identity Service را می‌سازد

CREATE DATABASE EventBride_Identity;
GO

USE EventBride_Identity;
GO

-- جدول کاربران (ASP.NET Identity)
CREATE TABLE [Users] (
    [Id]                NVARCHAR(450) PRIMARY KEY,
    [UserName]          NVARCHAR(256) NOT NULL,
    [NormalizedUserName] NVARCHAR(256) NOT NULL,
    [Email]             NVARCHAR(256) NOT NULL,
    [NormalizedEmail]   NVARCHAR(256) NOT NULL,
    [EmailConfirmed]    BIT NOT NULL DEFAULT 0,
    [PasswordHash]      NVARCHAR(MAX) NOT NULL,
    [SecurityStamp]     NVARCHAR(MAX) NULL,
    [ConcurrencyStamp]  NVARCHAR(MAX) NULL,
    [PhoneNumber]       NVARCHAR(MAX) NULL,
    [PhoneNumberConfirmed] BIT NOT NULL DEFAULT 0,
    [TwoFactorEnabled]  BIT NOT NULL DEFAULT 0,
    [LockoutEnd]        DATETIMEOFFSET NULL,
    [LockoutEnabled]    BIT NOT NULL DEFAULT 0,
    [AccessFailedCount] INT NOT NULL DEFAULT 0,

    -- فیلدهای سفارشی
    [FirstName]         NVARCHAR(100) NOT NULL,
    [LastName]          NVARCHAR(100) NOT NULL,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsActive]          BIT NOT NULL DEFAULT 1
);

-- ایندکس‌ها برای کاربران
CREATE UNIQUE INDEX IX_Users_UserName ON [Users]([UserName]);
CREATE UNIQUE INDEX IX_Users_Email ON [Users]([NormalizedEmail]);
CREATE INDEX IX_Users_CreatedAt ON [Users]([CreatedAt]);

-- جدول Refresh Tokenها (برای چرخش توکن)
CREATE TABLE [RefreshTokens] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [UserId]            NVARCHAR(450) NOT NULL,
    [Token]             NVARCHAR(500) NOT NULL,
    [JwtId]             NVARCHAR(100) NOT NULL,  -- شناسه منحصربفرد JWT
    [IsUsed]            BIT NOT NULL DEFAULT 0,
    [IsRevoked]         BIT NOT NULL DEFAULT 0,
    [ExpiresAt]         DATETIME2 NOT NULL,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE()),
    [CreatedByIp]       NVARCHAR(50) NULL,

    -- روابط
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY ([UserId])
        REFERENCES [Users]([Id]) ON DELETE CASCADE
);

-- ایندکس‌ها برای Refresh Token
CREATE INDEX IX_RefreshTokens_UserId ON [RefreshTokens]([UserId]);
CREATE INDEX IX_RefreshTokens_Token ON [RefreshTokens]([Token]);
CREATE INDEX IX_RefreshTokens_ExpiresAt ON [RefreshTokens]([ExpiresAt]);
CREATE INDEX IX_RefreshTokens_IsUsed_IsRevoked ON [RefreshTokens]([IsUsed], [IsRevoked]);

GO
