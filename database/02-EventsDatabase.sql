-- =============================================
-- Events Service Database
-- =============================================

CREATE DATABASE EventBride_Events;
GO

USE EventBride_Events;
GO

-- جدول مکان‌ها/سالن‌ها
CREATE TABLE [Venues] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [Name]              NVARCHAR(200) NOT NULL,
    [Address]           NVARCHAR(500) NOT NULL,
    [City]              NVARCHAR(100) NOT NULL,
    [Country]           NVARCHAR(100) NOT NULL,
    [Capacity]          INT NOT NULL,
    [Description]       NVARCHAR(2000) NULL,
    [ImageUrl]          NVARCHAR(500) NULL,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE()),
    [IsActive]          BIT NOT NULL DEFAULT 1
);

-- ایندکس‌ها
CREATE INDEX IX_Venues_City ON [Venues]([City]);
CREATE INDEX IX_Venues_IsActive ON [Venues]([IsActive]);

-- جدول دسته‌بندی رویدادها
CREATE TABLE [EventCategories] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [Name]              NVARCHAR(100) NOT NULL,
    [Description]       NVARCHAR(500) NULL,
    [ParentId]          INT NULL,  -- برای دسته‌بندی‌های سلسله‌مراتبی
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_EventCategories_Parent FOREIGN KEY ([ParentId])
        REFERENCES [EventCategories]([Id])
);

CREATE UNIQUE INDEX IX_EventCategories_Name ON [EventCategories]([Name]);

-- جدول رویدادها
CREATE TABLE [Events] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [Title]             NVARCHAR(200) NOT NULL,
    [Description]       NVARCHAR(MAX) NULL,
    [ImageUrl]          NVARCHAR(500) NULL,
    [VenueId]           INT NOT NULL,
    [CategoryId]        INT NOT NULL,
    [OrganizerId]       NVARCHAR(450) NOT NULL,  -- UserId از Identity Service
    [StartDate]         DATETIME2 NOT NULL,
    [EndDate]           DATETIME2 NOT NULL,
    [DoorsOpen]         DATETIME2 NULL,  -- ساعت باز شدن درها
    [Status]            NVARCHAR(50) NOT NULL DEFAULT 'Draft',  -- Draft, Published, Cancelled, Completed
    [IsFeatured]        BIT NOT NULL DEFAULT 0,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE()),

    -- محدودیت‌ها
    CONSTRAINT FK_Events_Venue FOREIGN KEY ([VenueId])
        REFERENCES [Venues]([Id]),
    CONSTRAINT FK_Events_Category FOREIGN KEY ([CategoryId])
        REFERENCES [EventCategories]([Id]),
    CONSTRAINT CK_Events_Dates CHECK ([EndDate] > [StartDate]),
    CONSTRAINT CK_Events_Status CHECK ([Status] IN ('Draft', 'Published', 'Cancelled', 'Completed'))
);

-- ایندکس‌ها (خیلی مهم برای queryها)
CREATE INDEX IX_Events_VenueId ON [Events]([VenueId]);
CREATE INDEX IX_Events_CategoryId ON [Events]([CategoryId]);
CREATE INDEX IX_Events_OrganizerId ON [Events]([OrganizerId]);
CREATE INDEX IX_Events_StartDate ON [Events]([StartDate]);
CREATE INDEX IX_Events_Status ON [Events]([Status]);
CREATE INDEX IX_Events_IsFeatured ON [Events]([IsFeatured]);

-- ایندکس ترکیبی برای پرکاربردترین query
CREATE INDEX IX_Events_Status_StartDate ON [Events]([Status], [StartDate]);
CREATE INDEX IX_Events_OrganizerId_Status ON [Events]([OrganizerId], [Status]);

-- جدول نوع بلیط‌ها (مثلاً VIP، معمولی، ...)
CREATE TABLE [TicketTypes] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [EventId]           INT NOT NULL,
    [Name]              NVARCHAR(100) NOT NULL,
    [Description]       NVARCHAR(500) NULL,
    [Price]             DECIMAL(18,2) NOT NULL,
    [Quantity]          INT NOT NULL,  -- تعداد کل بلیط از این نوع
    [SoldCount]         INT NOT NULL DEFAULT 0,
    [MaxPerOrder]       INT NOT NULL DEFAULT 10,  -- حداکثر بلیط در هر سفارش
    [SaleStart]         DATETIME2 NOT NULL,  -- شروع فروش
    [SaleEnd]           DATETIME2 NOT NULL,  -- پایان فروش
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_TicketTypes_Event FOREIGN KEY ([EventId])
        REFERENCES [Events]([Id]) ON DELETE CASCADE,
    CONSTRAINT CK_TicketTypes_Price CHECK ([Price] >= 0),
    CONSTRAINT CK_TicketTypes_Quantity CHECK ([Quantity] >= 0),
    CONSTRAINT CK_TicketTypes_SoldCount CHECK ([SoldCount] >= 0),
    CONSTRAINT CK_TicketTypes_MaxPerOrder CHECK ([MaxPerOrder] > 0),
    CONSTRAINT CK_TicketTypes_SaleDates CHECK ([SaleEnd] > [SaleStart])
);

-- ایندکس‌ها
CREATE INDEX IX_TicketTypes_EventId ON [TicketTypes]([EventId]);
CREATE INDEX IX_TicketTypes_SaleStart_SaleEnd ON [TicketTypes]([SaleStart], [SaleEnd]);
CREATE INDEX IX_TicketTypes_Price ON [TicketTypes]([Price]);

GO
