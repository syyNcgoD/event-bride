-- =============================================
-- Stored Procedures مهم
-- =============================================

USE EventBride_Booking;
GO

-- =============================================
-- SP برای بررسی موجودی صندلی با Pessimistic Lock
-- این SP زمانی استفاده می‌شود که کاربر می‌خواهد بلیط رزرو کند
-- =============================================
CREATE PROCEDURE [dbo].[usp_CheckAndReserveSeats]
    @EventId INT,
    @TicketTypeId INT,
    @Quantity INT,
    @OrderId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    -- قفل Pessimistic روی ردیف مربوطه (UPDLOCK)
    -- این باعث می‌شود کاربر دیگری نتواند هم‌زمان همین ردیف را تغییر دهد
    DECLARE @AvailableSeats INT;
    DECLARE @MaxPerOrder INT;
    DECLARE @SaleStart DATETIME2;
    DECLARE @SaleEnd DATETIME2;

    SELECT
        @AvailableSeats = [Quantity] - [SoldCount],
        @MaxPerOrder = [MaxPerOrder],
        @SaleStart = [SaleStart],
        @SaleEnd = [SaleEnd]
    FROM [EventBride_Events].[dbo].[TicketTypes]
    WITH (UPDLOCK, ROWLOCK)  -- قفل سطح ردیف با حالت Update
    WHERE [Id] = @TicketTypeId AND [EventId] = @EventId;

    -- بررسی‌ها
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

    -- افزایش SoldCount
    UPDATE [EventBride_Events].[dbo].[TicketTypes]
    SET [SoldCount] = [SoldCount] + @Quantity
    WHERE [Id] = @TicketTypeId;

    -- ساخت شماره سفارش یکتا
    DECLARE @OrderNumber NVARCHAR(50);
    SET @OrderNumber = 'ORD-' + FORMAT(GETUTCDATE(), 'yyyyMMdd') + '-' + CAST(NEXT VALUE FOR [dbo].[OrderNumberSequence] AS NVARCHAR(10));

    -- ایجاد سفارش
    INSERT INTO [Orders] ([OrderNumber], [UserId], [Status], [Email], [ExpiresAt])
    VALUES (@OrderNumber, @UserId, 'Pending', @Email, DATEADD(MINUTE, 10, GETUTCDATE()));

    SET @OrderId = SCOPE_IDENTITY();

    COMMIT TRANSACTION;
END
GO

-- Sequence برای شماره سفارش یکتا
CREATE SEQUENCE [dbo].[OrderNumberSequence]
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 999999
    NO CYCLE;

GO

-- =============================================
-- Stored Procedure برای گزارش پرفروش‌ترین رویدادها
-- اینجا Execution Plan را بررسی کنید!
-- =============================================

USE EventBride_Booking;
GO

CREATE PROCEDURE [dbo].[usp_GetTopSellingEvents]
    @Top INT = 10,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- مقداردهی اولیه پیش‌فرض
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(MONTH, -6, GETUTCDATE());
    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();

    -- این Query را در SSMS اجرا کنید و Execution Plan را ببینید
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

-- =============================================
-- SP برای آزادسازی رزروهای منقضی شده
-- توسط Hangfire هر دقیقه اجرا می‌شود
-- =============================================

CREATE PROCEDURE [dbo].[usp_ReleaseExpiredReservations]
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    -- پیدا کردن سفارشات منقضی شده
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

    -- برگرداندن SoldCount برای بلیط‌های منقضی شده
    UPDATE tt
    SET tt.[SoldCount] = tt.[SoldCount] - oi.[Quantity]
    FROM [EventBride_Events].[dbo].[TicketTypes] tt
    INNER JOIN [OrderItems] oi ON tt.[Id] = oi.[TicketTypeId]
    INNER JOIN @ExpiredOrders eo ON oi.[OrderId] = eo.[Id];

    -- به‌روزرسانی وضعیت سفارشات
    UPDATE [Orders]
    SET [Status] = 'Expired',
        [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM @ExpiredOrders);

    COMMIT TRANSACTION;

    -- برگرداندن تعداد سفارشات آزاد شده
    SELECT COUNT(*) AS ReleasedCount FROM @ExpiredOrders;
END
GO
