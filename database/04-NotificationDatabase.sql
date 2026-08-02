-- =============================================
-- Notification Service Database
-- =============================================

CREATE DATABASE EventBride_Notification;
GO

USE EventBride_Notification;
GO

-- جدول اعلان‌ها
CREATE TABLE [Notifications] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [UserId]            NVARCHAR(450) NOT NULL,
    [Type]              NVARCHAR(50) NOT NULL,  -- Email, SMS, Push
    [Channel]           NVARCHAR(50) NOT NULL,  -- BookingConfirmation, PaymentReceipt, EventReminder, etc.
    [Subject]           NVARCHAR(200) NOT NULL,
    [Body]              NVARCHAR(MAX) NOT NULL,
    [IsRead]            BIT NOT NULL DEFAULT 0,
    [IsSent]            BIT NOT NULL DEFAULT 0,
    [SentAt]            DATETIME2 NULL,
    [ReadAt]            DATETIME2 NULL,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT CK_Notifications_Type CHECK ([Type] IN ('Email', 'SMS', 'Push')),
    CONSTRAINT CK_Notifications_Channel CHECK ([Channel] IN (
        'BookingConfirmation', 'BookingCancellation', 'PaymentReceipt',
        'PaymentFailed', 'EventReminder', 'EventUpdate', 'EventCancellation'
    ))
);

-- ایندکس‌ها
CREATE INDEX IX_Notifications_UserId ON [Notifications]([UserId]);
CREATE INDEX IX_Notifications_IsRead ON [Notifications]([IsRead]);
CREATE INDEX IX_Notifications_IsSent ON [Notifications]([IsSent]);
CREATE INDEX IX_Notifications_CreatedAt ON [Notifications]([CreatedAt]);
CREATE INDEX IX_Notifications_UserId_IsRead ON [Notifications]([UserId], [IsRead]);
CREATE INDEX IX_Notifications_Type_CreatedAt ON [Notifications]([Type], [CreatedAt]);

-- جدول templateهای ایمیل
CREATE TABLE [EmailTemplates] (
    [Id]                INT IDENTITY(1,1) PRIMARY KEY,
    [Name]              NVARCHAR(100) NOT NULL UNIQUE,
    [Subject]           NVARCHAR(200) NOT NULL,
    [BodyTemplate]      NVARCHAR(MAX) NOT NULL,  -- HTML template با Placeholders
    [IsActive]          BIT NOT NULL DEFAULT 1,
    [CreatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt]         DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE UNIQUE INDEX IX_EmailTemplates_Name ON [EmailTemplates]([Name]);

GO
