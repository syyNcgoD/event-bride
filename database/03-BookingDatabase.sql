-- =============================================
-- Booking Service Database (مهم‌ترین بخش!)
-- =============================================

CREATE DATABASE EventBride_Booking;
GO

USE EventBride_Booking;
GO

-- جدول سفارشات/رزروها
CREATE TABLE [Orders] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [OrderNumber]       NVARCHAR(50) NOT NULL UNIQUE,  -- شماره یکتای سفارش
    [UserId]            NVARCHAR(450) NOT NULL,
    [Status]            NVARCHAR(50) NOT NULL DEFAULT 'Pending',  -- Pending, Confirmed, Cancelled, Expired, Refunded
    [TotalAmount]       DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Currency]          NVARCHAR(3) NOT NULL DEFAULT 'IRR',
    [Notes]             NVARCHAR(1000) NULL,

    -- آدرس ایمیل برای ارسال بلیط
    [Email]             NVARCHAR(256) NOT NULL,
    [PhoneNumber]       NVARCHAR(20) NULL,

    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ExpiresAt]         DATETIME2 NULL,  -- تاریخ انقضا برای رزروهای موقت (10 دقیقه)

    -- برای Optimistic Concurrency
    [RowVersion]        ROWVERSION NOT NULL,

    CONSTRAINT CK_Orders_Status CHECK ([Status] IN ('Pending', 'Confirmed', 'Cancelled', 'Expired', 'Refunded')),
    CONSTRAINT CK_Orders_TotalAmount CHECK ([TotalAmount] >= 0)
);

-- ایندکس‌ها (خیلی مهم برای performance)
CREATE UNIQUE INDEX IX_Orders_OrderNumber ON [Orders]([OrderNumber]);
CREATE INDEX IX_Orders_UserId ON [Orders]([UserId]);
CREATE INDEX IX_Orders_Status ON [Orders]([Status]);
CREATE INDEX IX_Orders_CreatedAt ON [Orders]([CreatedAt]);
CREATE INDEX IX_Orders_ExpiresAt ON [Orders]([ExpiresAt]);

-- ایندکس ترکیبی برای queryهای پرتکرار
CREATE INDEX IX_Orders_UserId_Status ON [Orders]([UserId], [Status]);
CREATE INDEX IX_Orders_Status_CreatedAt ON [Orders]([Status], [CreatedAt]);

-- جدول آیتم‌های سفارش (هر بلیط خریداری شده)
CREATE TABLE [OrderItems] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [OrderId]           INT NOT NULL,
    [TicketTypeId]      INT NOT NULL,  -- از Events Service
    [EventId]           INT NOT NULL,  -- کپی برای query سریع‌تر
    [EventTitle]        NVARCHAR(200) NOT NULL,  -- کپی برای نمایش
    [TicketTypeName]    NVARCHAR(100) NOT NULL,  -- کپی برای نمایش
    [SeatNumber]        NVARCHAR(20) NULL,  -- شماره صندلی (اختیاری)
    [Quantity]          INT NOT NULL DEFAULT 1,
    [UnitPrice]         DECIMAL(18,2) NOT NULL,
    [TotalPrice]        DECIMAL(18,2) NOT NULL,

    -- برای Optimistic Concurrency
    [RowVersion]        ROWVERSION NOT NULL,

    CONSTRAINT FK_OrderItems_Order FOREIGN KEY ([OrderId])
        REFERENCES [Orders]([Id]) ON DELETE CASCADE,
    CONSTRAINT CK_OrderItems_Quantity CHECK ([Quantity] > 0),
    CONSTRAINT CK_OrderItems_UnitPrice CHECK ([UnitPrice] >= 0)
);

-- ایندکس‌ها
CREATE INDEX IX_OrderItems_OrderId ON [OrderItems]([OrderId]);
CREATE INDEX IX_OrderItems_TicketTypeId ON [OrderItems]([TicketTypeId]);
CREATE INDEX IX_OrderItems_EventId ON [OrderItems]([EventId]);

-- جدول پرداخت‌ها
CREATE TABLE [Payments] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [OrderId]           INT NOT NULL,
    [PaymentMethod]     NVARCHAR(50) NOT NULL,  -- CreditCard, OnlineGateway, Wallet
    [TransactionId]     NVARCHAR(100) NULL,  -- شناسه تراکنش درگاه پرداخت
    [Amount]            DECIMAL(18,2) NOT NULL,
    [Currency]          NVARCHAR(3) NOT NULL DEFAULT 'IRR',
    [Status]            NVARCHAR(50) NOT NULL DEFAULT 'Pending',  -- Pending, Success, Failed, Refunded
    [FailureReason]     NVARCHAR(500) NULL,
    [PaidAt]            DATETIME2 NULL,
    [RefundedAt]        DATETIME2 NULL,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Payments_Order FOREIGN KEY ([OrderId])
        REFERENCES [Orders]([Id]),
    CONSTRAINT CK_Payments_Status CHECK ([Status] IN ('Pending', 'Success', 'Failed', 'Refunded')),
    CONSTRAINT CK_Payments_Amount CHECK ([Amount] > 0)
);

-- ایندکس‌ها
CREATE INDEX IX_Payments_OrderId ON [Payments]([OrderId]);
CREATE INDEX IX_Payments_Status ON [Payments]([Status]);
CREATE INDEX IX_Payments_TransactionId ON [Payments]([TransactionId]);
CREATE INDEX IX_Payments_PaidAt ON [Payments]([PaidAt]);

-- جدول لاگ تغییرات سفارش (برای audit trail)
CREATE TABLE [OrderStatusHistory] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [OrderId]           INT NOT NULL,
    [OldStatus]         NVARCHAR(50) NULL,
    [NewStatus]         NVARCHAR(50) NOT NULL,
    [ChangedBy]         NVARCHAR(450) NULL,  -- UserId یا System
    [Reason]            NVARCHAR(500) NULL,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_OrderStatusHistory_Order FOREIGN KEY ([OrderId])
        REFERENCES [Orders]([Id]) ON DELETE CASCADE
);

-- ایندکس‌ها
CREATE INDEX IX_OrderStatusHistory_OrderId ON [OrderStatusHistory]([OrderId]);
CREATE INDEX IX_OrderStatusHistory_CreatedAt ON [OrderStatusHistory]([CreatedAt]);

GO
