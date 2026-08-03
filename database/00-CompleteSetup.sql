-- =============================================
-- EventBride - Complete Database Setup
-- اجرای این اسکریپت تمام جداول را می‌سازد
-- =============================================

-- استفاده از دیتابیس EventBride (که قبلاً ساختید)
USE [EventBride];
GO

-- =============================================
-- حذف جداول قبلی (اگر وجود داشته باشد)
-- =============================================
IF OBJECT_ID('dbo.OrderStatusHistory', 'U') IS NOT NULL DROP TABLE [dbo].[OrderStatusHistory];
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE [dbo].[Payments];
IF OBJECT_ID('dbo.OrderItems', 'U') IS NOT NULL DROP TABLE [dbo].[OrderItems];
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE [dbo].[Orders];
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DROP TABLE [dbo].[Notifications];
IF OBJECT_ID('dbo.EmailTemplates', 'U') IS NOT NULL DROP TABLE [dbo].[EmailTemplates];
IF OBJECT_ID('dbo.TicketTypes', 'U') IS NOT NULL DROP TABLE [dbo].[TicketTypes];
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE [dbo].[Events];
IF OBJECT_ID('dbo.EventCategories', 'U') IS NOT NULL DROP TABLE [dbo].[EventCategories];
IF OBJECT_ID('dbo.Venues', 'U') IS NOT NULL DROP TABLE [dbo].[Venues];
IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NOT NULL DROP TABLE [dbo].[RefreshTokens];
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE [dbo].[Users];
IF OBJECT_ID('dbo.OrderNumberSequence', 'SO') IS NOT NULL DROP SEQUENCE [dbo].[OrderNumberSequence];
GO

PRINT '=============================================';
PRINT 'Creating EventBride Database Schema';
PRINT '=============================================';
GO

-- =============================================
-- 1. Identity Tables
-- =============================================
PRINT 'Creating Identity Tables...';

CREATE TABLE [Users] (
    [Id]                    NVARCHAR(450) PRIMARY KEY,
    [UserName]              NVARCHAR(256) NOT NULL,
    [NormalizedUserName]    NVARCHAR(256) NOT NULL,
    [Email]                 NVARCHAR(256) NOT NULL,
    [NormalizedEmail]       NVARCHAR(256) NOT NULL,
    [EmailConfirmed]        BIT NOT NULL DEFAULT 0,
    [PasswordHash]          NVARCHAR(MAX) NOT NULL,
    [SecurityStamp]         NVARCHAR(MAX) NULL,
    [ConcurrencyStamp]      NVARCHAR(MAX) NULL,
    [PhoneNumber]           NVARCHAR(MAX) NULL,
    [PhoneNumberConfirmed]  BIT NOT NULL DEFAULT 0,
    [TwoFactorEnabled]      BIT NOT NULL DEFAULT 0,
    [LockoutEnd]            DATETIMEOFFSET NULL,
    [LockoutEnabled]        BIT NOT NULL DEFAULT 0,
    [AccessFailedCount]     INT NOT NULL DEFAULT 0,
    [FirstName]             NVARCHAR(100) NOT NULL,
    [LastName]              NVARCHAR(100) NOT NULL,
    [CreatedAt]             DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]             DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsActive]              BIT NOT NULL DEFAULT 1
);
GO

CREATE UNIQUE INDEX IX_Users_UserName ON [Users]([UserName]);
CREATE UNIQUE INDEX IX_Users_Email ON [Users]([NormalizedEmail]);
CREATE INDEX IX_Users_CreatedAt ON [Users]([CreatedAt]);
GO

CREATE TABLE [RefreshTokens] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [UserId]            NVARCHAR(450) NOT NULL,
    [Token]             NVARCHAR(500) NOT NULL,
    [JwtId]             NVARCHAR(100) NOT NULL,
    [IsUsed]            BIT NOT NULL DEFAULT 0,
    [IsRevoked]         BIT NOT NULL DEFAULT 0,
    [ExpiresAt]         DATETIME2 NOT NULL,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedByIp]       NVARCHAR(50) NULL,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY ([UserId])
        REFERENCES [Users]([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX IX_RefreshTokens_UserId ON [RefreshTokens]([UserId]);
CREATE INDEX IX_RefreshTokens_Token ON [RefreshTokens]([Token]);
CREATE INDEX IX_RefreshTokens_ExpiresAt ON [RefreshTokens]([ExpiresAt]);
CREATE INDEX IX_RefreshTokens_IsUsed_IsRevoked ON [RefreshTokens]([IsUsed], [IsRevoked]);
GO

PRINT '  ✓ Users table created';
PRINT '  ✓ RefreshTokens table created';
GO

-- =============================================
-- 2. Events Tables
-- =============================================
PRINT 'Creating Events Tables...';

CREATE TABLE [Venues] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [Name]          NVARCHAR(200) NOT NULL,
    [Address]       NVARCHAR(500) NOT NULL,
    [City]          NVARCHAR(100) NOT NULL,
    [Country]       NVARCHAR(100) NOT NULL,
    [Capacity]      INT NOT NULL,
    [Description]   NVARCHAR(2000) NULL,
    [ImageUrl]      NVARCHAR(500) NULL,
    [CreatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsActive]      BIT NOT NULL DEFAULT 1
);
GO

CREATE INDEX IX_Venues_City ON [Venues]([City]);
CREATE INDEX IX_Venues_IsActive ON [Venues]([IsActive]);
GO

CREATE TABLE [EventCategories] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [Name]          NVARCHAR(100) NOT NULL,
    [Description]   NVARCHAR(500) NULL,
    [ParentId]      INT NULL,
    [CreatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_EventCategories_Parent FOREIGN KEY ([ParentId])
        REFERENCES [EventCategories]([Id])
);
GO

CREATE UNIQUE INDEX IX_EventCategories_Name ON [EventCategories]([Name]);
GO

CREATE TABLE [Events] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [Title]         NVARCHAR(200) NOT NULL,
    [Description]   NVARCHAR(MAX) NULL,
    [ImageUrl]      NVARCHAR(500) NULL,
    [VenueId]       INT NOT NULL,
    [CategoryId]    INT NOT NULL,
    [OrganizerId]   NVARCHAR(450) NOT NULL,
    [StartDate]     DATETIME2 NOT NULL,
    [EndDate]       DATETIME2 NOT NULL,
    [DoorsOpen]     DATETIME2 NULL,
    [Status]        NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [IsFeatured]    BIT NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Events_Venue FOREIGN KEY ([VenueId])
        REFERENCES [Venues]([Id]),
    CONSTRAINT FK_Events_Category FOREIGN KEY ([CategoryId])
        REFERENCES [EventCategories]([Id]),
    CONSTRAINT CK_Events_Dates CHECK ([EndDate] > [StartDate]),
    CONSTRAINT CK_Events_Status CHECK ([Status] IN ('Draft', 'Published', 'Cancelled', 'Completed'))
);
GO

CREATE INDEX IX_Events_VenueId ON [Events]([VenueId]);
CREATE INDEX IX_Events_CategoryId ON [Events]([CategoryId]);
CREATE INDEX IX_Events_OrganizerId ON [Events]([OrganizerId]);
CREATE INDEX IX_Events_StartDate ON [Events]([StartDate]);
CREATE INDEX IX_Events_Status ON [Events]([Status]);
CREATE INDEX IX_Events_IsFeatured ON [Events]([IsFeatured]);
CREATE INDEX IX_Events_Status_StartDate ON [Events]([Status], [StartDate]);
CREATE INDEX IX_Events_OrganizerId_Status ON [Events]([OrganizerId], [Status]);
GO

CREATE TABLE [TicketTypes] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [EventId]       INT NOT NULL,
    [Name]          NVARCHAR(100) NOT NULL,
    [Description]   NVARCHAR(500) NULL,
    [Price]         DECIMAL(18,2) NOT NULL,
    [Quantity]      INT NOT NULL,
    [SoldCount]     INT NOT NULL DEFAULT 0,
    [MaxPerOrder]   INT NOT NULL DEFAULT 10,
    [SaleStart]     DATETIME2 NOT NULL,
    [SaleEnd]       DATETIME2 NOT NULL,
    [CreatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TicketTypes_Event FOREIGN KEY ([EventId])
        REFERENCES [Events]([Id]) ON DELETE CASCADE,
    CONSTRAINT CK_TicketTypes_Price CHECK ([Price] >= 0),
    CONSTRAINT CK_TicketTypes_Quantity CHECK ([Quantity] >= 0),
    CONSTRAINT CK_TicketTypes_SoldCount CHECK ([SoldCount] >= 0),
    CONSTRAINT CK_TicketTypes_MaxPerOrder CHECK ([MaxPerOrder] > 0),
    CONSTRAINT CK_TicketTypes_SaleDates CHECK ([SaleEnd] > [SaleStart])
);
GO

CREATE INDEX IX_TicketTypes_EventId ON [TicketTypes]([EventId]);
CREATE INDEX IX_TicketTypes_SaleStart_SaleEnd ON [TicketTypes]([SaleStart], [SaleEnd]);
CREATE INDEX IX_TicketTypes_Price ON [TicketTypes]([Price]);
GO

PRINT '  ✓ Venues table created';
PRINT '  ✓ EventCategories table created';
PRINT '  ✓ Events table created';
PRINT '  ✓ TicketTypes table created';
GO

-- =============================================
-- 3. Booking Tables
-- =============================================
PRINT 'Creating Booking Tables...';

CREATE TABLE [Orders] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [OrderNumber]   NVARCHAR(50) NOT NULL,
    [UserId]        NVARCHAR(450) NOT NULL,
    [Status]        NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [TotalAmount]   DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Currency]      NVARCHAR(3) NOT NULL DEFAULT 'IRR',
    [Notes]         NVARCHAR(1000) NULL,
    [Email]         NVARCHAR(256) NOT NULL,
    [PhoneNumber]   NVARCHAR(20) NULL,
    [CreatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ExpiresAt]     DATETIME2 NULL,
    [RowVersion]    ROWVERSION NOT NULL,
    CONSTRAINT CK_Orders_Status CHECK ([Status] IN ('Pending', 'Confirmed', 'Cancelled', 'Expired', 'Refunded')),
    CONSTRAINT CK_Orders_TotalAmount CHECK ([TotalAmount] >= 0)
);
GO

CREATE UNIQUE INDEX IX_Orders_OrderNumber ON [Orders]([OrderNumber]);
CREATE INDEX IX_Orders_UserId ON [Orders]([UserId]);
CREATE INDEX IX_Orders_Status ON [Orders]([Status]);
CREATE INDEX IX_Orders_CreatedAt ON [Orders]([CreatedAt]);
CREATE INDEX IX_Orders_ExpiresAt ON [Orders]([ExpiresAt]);
CREATE INDEX IX_Orders_UserId_Status ON [Orders]([UserId], [Status]);
CREATE INDEX IX_Orders_Status_CreatedAt ON [Orders]([Status], [CreatedAt]);
GO

CREATE TABLE [OrderItems] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [OrderId]           INT NOT NULL,
    [TicketTypeId]      INT NOT NULL,
    [EventId]           INT NOT NULL,
    [EventTitle]        NVARCHAR(200) NOT NULL,
    [TicketTypeName]    NVARCHAR(100) NOT NULL,
    [SeatNumber]        NVARCHAR(20) NULL,
    [Quantity]          INT NOT NULL DEFAULT 1,
    [UnitPrice]         DECIMAL(18,2) NOT NULL,
    [TotalPrice]        DECIMAL(18,2) NOT NULL,
    [RowVersion]        ROWVERSION NOT NULL,
    CONSTRAINT FK_OrderItems_Order FOREIGN KEY ([OrderId])
        REFERENCES [Orders]([Id]) ON DELETE CASCADE,
    CONSTRAINT CK_OrderItems_Quantity CHECK ([Quantity] > 0),
    CONSTRAINT CK_OrderItems_UnitPrice CHECK ([UnitPrice] >= 0)
);
GO

CREATE INDEX IX_OrderItems_OrderId ON [OrderItems]([OrderId]);
CREATE INDEX IX_OrderItems_TicketTypeId ON [OrderItems]([TicketTypeId]);
CREATE INDEX IX_OrderItems_EventId ON [OrderItems]([EventId]);
GO

CREATE TABLE [Payments] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [OrderId]           INT NOT NULL,
    [PaymentMethod]     NVARCHAR(50) NOT NULL,
    [TransactionId]     NVARCHAR(100) NULL,
    [Amount]            DECIMAL(18,2) NOT NULL,
    [Currency]          NVARCHAR(3) NOT NULL DEFAULT 'IRR',
    [Status]            NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [FailureReason]     NVARCHAR(500) NULL,
    [PaidAt]            DATETIME2 NULL,
    [RefundedAt]        DATETIME2 NULL,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Payments_Order FOREIGN KEY ([OrderId])
        REFERENCES [Orders]([Id]),
    CONSTRAINT CK_Payments_Status CHECK ([Status] IN ('Pending', 'Success', 'Failed', 'Refunded')),
    CONSTRAINT CK_Payments_Amount CHECK ([Amount] > 0)
);
GO

CREATE INDEX IX_Payments_OrderId ON [Payments]([OrderId]);
CREATE INDEX IX_Payments_Status ON [Payments]([Status]);
CREATE INDEX IX_Payments_TransactionId ON [Payments]([TransactionId]);
CREATE INDEX IX_Payments_PaidAt ON [Payments]([PaidAt]);
GO

CREATE TABLE [OrderStatusHistory] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [OrderId]       INT NOT NULL,
    [OldStatus]     NVARCHAR(50) NULL,
    [NewStatus]     NVARCHAR(50) NOT NULL,
    [ChangedBy]     NVARCHAR(450) NULL,
    [Reason]        NVARCHAR(500) NULL,
    [CreatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_OrderStatusHistory_Order FOREIGN KEY ([OrderId])
        REFERENCES [Orders]([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX IX_OrderStatusHistory_OrderId ON [OrderStatusHistory]([OrderId]);
CREATE INDEX IX_OrderStatusHistory_CreatedAt ON [OrderStatusHistory]([CreatedAt]);
GO

PRINT '  ✓ Orders table created';
PRINT '  ✓ OrderItems table created';
PRINT '  ✓ Payments table created';
PRINT '  ✓ OrderStatusHistory table created';
GO

-- =============================================
-- 4. Notification Tables
-- =============================================
PRINT 'Creating Notification Tables...';

CREATE TABLE [Notifications] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [UserId]        NVARCHAR(450) NOT NULL,
    [Type]          NVARCHAR(50) NOT NULL,
    [Channel]       NVARCHAR(50) NOT NULL,
    [Subject]       NVARCHAR(200) NOT NULL,
    [Body]          NVARCHAR(MAX) NOT NULL,
    [IsRead]        BIT NOT NULL DEFAULT 0,
    [IsSent]        BIT NOT NULL DEFAULT 0,
    [SentAt]        DATETIME2 NULL,
    [ReadAt]        DATETIME2 NULL,
    [CreatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Notifications_Type CHECK ([Type] IN ('Email', 'SMS', 'Push')),
    CONSTRAINT CK_Notifications_Channel CHECK ([Channel] IN (
        'BookingConfirmation', 'BookingCancellation', 'PaymentReceipt',
        'PaymentFailed', 'EventReminder', 'EventUpdate', 'EventCancellation'
    ))
);
GO

CREATE INDEX IX_Notifications_UserId ON [Notifications]([UserId]);
CREATE INDEX IX_Notifications_IsRead ON [Notifications]([IsRead]);
CREATE INDEX IX_Notifications_IsSent ON [Notifications]([IsSent]);
CREATE INDEX IX_Notifications_CreatedAt ON [Notifications]([CreatedAt]);
CREATE INDEX IX_Notifications_UserId_IsRead ON [Notifications]([UserId], [IsRead]);
CREATE INDEX IX_Notifications_Type_CreatedAt ON [Notifications]([Type], [CreatedAt]);
GO

CREATE TABLE [EmailTemplates] (
    [Id]            INT IDENTITY(1,1) PRIMARY KEY,
    [Name]          NVARCHAR(100) NOT NULL,
    [Subject]       NVARCHAR(200) NOT NULL,
    [BodyTemplate]  NVARCHAR(MAX) NOT NULL,
    [IsActive]      BIT NOT NULL DEFAULT 1,
    [CreatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]     DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE UNIQUE INDEX IX_EmailTemplates_Name ON [EmailTemplates]([Name]);
GO

PRINT '  ✓ Notifications table created';
PRINT '  ✓ EmailTemplates table created';
GO

-- =============================================
-- 5. Sequences
-- =============================================
PRINT 'Creating Sequences...';

CREATE SEQUENCE [dbo].[OrderNumberSequence]
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 999999
    NO CYCLE;
GO

PRINT '  ✓ OrderNumberSequence created';
GO

-- =============================================
-- 6. Stored Procedures
-- =============================================
PRINT 'Creating Stored Procedures...';

-- SP 1: بررسی و رزرو صندلی با Pessimistic Lock
CREATE PROCEDURE [dbo].[usp_CheckAndReserveSeats]
    @EventId INT,
    @TicketTypeId INT,
    @Quantity INT,
    @UserId NVARCHAR(450),
    @Email NVARCHAR(256),
    @OrderId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @AvailableSeats INT;
    DECLARE @MaxPerOrder INT;
    DECLARE @SaleStart DATETIME2;
    DECLARE @SaleEnd DATETIME2;

    -- قفل Pessimistic با UPDLOCK
    SELECT
        @AvailableSeats = [Quantity] - [SoldCount],
        @MaxPerOrder = [MaxPerOrder],
        @SaleStart = [SaleStart],
        @SaleEnd = [SaleEnd]
    FROM [TicketTypes]
    WITH (UPDLOCK, ROWLOCK)
    WHERE [Id] = @TicketTypeId AND [EventId] = @EventId;

    IF @AvailableSeats IS NULL
    BEGIN
        RAISERROR('Ticket type not found', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF GETUTCDATE() < @SaleStart OR GETUTCDATE() > @SaleEnd
    BEGIN
        RAISERROR('Ticket sale is not active', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF @Quantity > @AvailableSeats
    BEGIN
        RAISERROR('Not enough seats available', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF @Quantity > @MaxPerOrder
    BEGIN
        RAISERROR('Quantity exceeds max per order limit', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE [TicketTypes]
    SET [SoldCount] = [SoldCount] + @Quantity
    WHERE [Id] = @TicketTypeId;

    DECLARE @OrderNumber NVARCHAR(50);
    SET @OrderNumber = 'ORD-' + FORMAT(GETUTCDATE(), 'yyyyMMdd') + '-' +
                       CAST(NEXT VALUE FOR [dbo].[OrderNumberSequence] AS NVARCHAR(10));

    INSERT INTO [Orders] ([OrderNumber], [UserId], [Status], [Email], [ExpiresAt])
    VALUES (@OrderNumber, @UserId, 'Pending', @Email, DATEADD(MINUTE, 10, GETUTCDATE()));

    SET @OrderId = SCOPE_IDENTITY();

    COMMIT TRANSACTION;
END
GO

PRINT '  ✓ usp_CheckAndReserveSeats created';
GO

-- SP 2: گزارش پرفروش‌ترین رویدادها
CREATE PROCEDURE [dbo].[usp_GetTopSellingEvents]
    @Top INT = 10,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @StartDate IS NULL
        SET @StartDate = DATEADD(MONTH, -6, GETUTCDATE());
    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();

    SELECT TOP (@Top)
        oi.[EventId],
        oi.[EventTitle],
        COUNT(DISTINCT o.[Id]) AS TotalOrders,
        SUM(oi.[Quantity]) AS TotalTicketsSold,
        SUM(oi.[TotalPrice]) AS TotalRevenue,
        AVG(oi.[UnitPrice]) AS AverageTicketPrice,
        MIN(o.[CreatedAt]) AS FirstOrderDate,
        MAX(o.[CreatedAt]) AS LastOrderDate
    FROM [OrderItems] oi
    INNER JOIN [Orders] o ON oi.[OrderId] = o.[Id]
    WHERE o.[Status] = 'Confirmed'
        AND o.[CreatedAt] BETWEEN @StartDate AND @EndDate
    GROUP BY oi.[EventId], oi.[EventTitle]
    ORDER BY TotalTicketsSold DESC;
END
GO

PRINT '  ✓ usp_GetTopSellingEvents created';
GO

-- SP 3: آزادسازی رزروهای منقضی شده
CREATE PROCEDURE [dbo].[usp_ReleaseExpiredReservations]
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    DECLARE @ExpiredOrders TABLE (
        [Id] INT,
        [OrderNumber] NVARCHAR(50)
    );

    INSERT INTO @ExpiredOrders ([Id], [OrderNumber])
    SELECT [Id], [OrderNumber]
    FROM [Orders]
    WHERE [Status] = 'Pending'
        AND [ExpiresAt] IS NOT NULL
        AND [ExpiresAt] < GETUTCDATE();

    UPDATE tt
    SET tt.[SoldCount] = tt.[SoldCount] - oi.[Quantity]
    FROM [TicketTypes] tt
    INNER JOIN [OrderItems] oi ON tt.[Id] = oi.[TicketTypeId]
    INNER JOIN @ExpiredOrders eo ON oi.[OrderId] = eo.[Id];

    UPDATE [Orders]
    SET [Status] = 'Expired',
        [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM @ExpiredOrders);

    COMMIT TRANSACTION;

    SELECT COUNT(*) AS ReleasedCount FROM @ExpiredOrders;
END
GO

PRINT '  ✓ usp_ReleaseExpiredReservations created';
GO

-- =============================================
-- Finish
-- =============================================
PRINT '=============================================';
PRINT '✓ EventBride database schema created successfully!';
PRINT '=============================================';
PRINT '';
PRINT 'Tables created:';
PRINT '  - Users (Identity)';
PRINT '  - RefreshTokens (Identity)';
PRINT '  - Venues (Events)';
PRINT '  - EventCategories (Events)';
PRINT '  - Events (Events)';
PRINT '  - TicketTypes (Events)';
PRINT '  - Orders (Booking)';
PRINT '  - OrderItems (Booking)';
PRINT '  - Payments (Booking)';
PRINT '  - OrderStatusHistory (Booking)';
PRINT '  - Notifications (Notification)';
PRINT '  - EmailTemplates (Notification)';
PRINT '';
PRINT 'Stored Procedures:';
PRINT '  - usp_CheckAndReserveSeats (Pessimistic Lock)';
PRINT '  - usp_GetTopSellingEvents (Reporting)';
PRINT '  - usp_ReleaseExpiredReservations (Hangfire)';
GO
