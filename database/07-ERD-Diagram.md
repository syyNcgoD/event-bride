# ERD Diagram - EventBride

## نمای کلی سیستم

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Identity Service DB                                 │
│  ┌──────────────┐        ┌──────────────────┐                              │
│  │    Users     │        │  RefreshTokens   │                              │
│  │──────────────│        │──────────────────│                              │
│  │ Id (PK)     │───┐    │ Id (PK)         │                              │
│  │ UserName    │   │    │ UserId (FK) ─────│──→ Users.Id                  │
│  │ Email       │   │    │ Token           │                              │
│  │ PasswordHash│   │    │ JwtId           │                              │
│  │ FirstName   │   │    │ IsUsed          │                              │
│  │ LastName    │   │    │ ExpiresAt       │                              │
│  │ CreatedAt   │   │    └──────────────────┘                              │
│  └──────────────┘   │                                                     │
└─────────────────────┼─────────────────────────────────────────────────────┘
                      │
                      │ (Network Call - REST API)
                      │
┌─────────────────────┼─────────────────────────────────────────────────────┐
│                     ▼    Events Service DB                                 │
│  ┌──────────────┐  ┌──────────────────┐  ┌──────────────────┐            │
│  │   Venues     │  │     Events       │  │  EventCategories │            │
│  │──────────────│  │──────────────────│  │──────────────────│            │
│  │ Id (PK)     │←─│ VenueId (FK)     │  │ Id (PK)         │←─┐         │
│  │ Name        │  │ Id (PK)         │──│ CategoryId (FK)  │  │         │
│  │ Address     │  │ Title           │  │ Name            │  │         │
│  │ City        │  │ OrganizerId     │  │ ParentId (FK) ──│──┘ (self) │
│  │ Capacity    │  │ StartDate       │  └──────────────────┘            │
│  └──────────────┘  │ EndDate         │                                  │
│                    │ Status          │  ┌──────────────────┐            │
│                    └─────────────────┘  │   TicketTypes    │            │
│                           │             │──────────────────│            │
│                           │             │ Id (PK)         │            │
│                           └─────────────│ EventId (FK)    │            │
│                                         │ Name            │            │
│                                         │ Price           │            │
│                                         │ Quantity        │            │
│                                         │ SoldCount       │            │
│                                         └──────────────────┘            │
└──────────────────────────────────────────────────────────────────────────┘
                      │
                      │ (gRPC - for fast seat check)
                      │
┌─────────────────────┼─────────────────────────────────────────────────────┐
│                     ▼    Booking Service DB                                │
│  ┌──────────────────────────────────────────────────────────┐            │
│  │                        Orders                            │            │
│  │──────────────────────────────────────────────────────────│            │
│  │ Id (PK)                                                 │            │
│  │ OrderNumber (UNIQUE)                                    │            │
│  │ UserId ──────────────────────────────────────────────→ Users.Id     │
│  │ Status (Pending/Confirmed/Cancelled/Expired/Refunded)  │            │
│  │ TotalAmount                                             │            │
│  │ RowVersion (Optimistic Concurrency)                     │            │
│  └──────────────────────────────────────────────────────────┘            │
│           │                                         │                    │
│           │ 1:N                                     │ 1:N                │
│           ▼                                         ▼                    │
│  ┌──────────────────────┐              ┌──────────────────────┐         │
│  │     OrderItems       │              │      Payments        │         │
│  │──────────────────────│              │──────────────────────│         │
│  │ Id (PK)             │              │ Id (PK)             │         │
│  │ OrderId (FK) ───────│──→ Orders.Id │ OrderId (FK) ───────│──→ Orders│
│  │ TicketTypeId        │              │ PaymentMethod       │         │
│  │ EventId             │              │ TransactionId       │         │
│  │ EventTitle          │              │ Amount              │         │
│  │ Quantity            │              │ Status              │         │
│  │ UnitPrice           │              │ PaidAt              │         │
│  │ TotalPrice          │              └──────────────────────┘         │
│  │ RowVersion          │                                              │
│  └──────────────────────┘              ┌──────────────────────┐         │
│                                        │ OrderStatusHistory   │         │
│                                        │──────────────────────│         │
│                                        │ Id (PK)             │         │
│                                        │ OrderId (FK) ───────│──→ Orders│
│                                        │ OldStatus           │         │
│                                        │ NewStatus           │         │
│                                        │ ChangedBy           │         │
│                                        │ CreatedAt           │         │
│                                        └──────────────────────┘         │
└──────────────────────────────────────────────────────────────────────────┘
                      │
                      │ (RabbitMQ - Async Events)
                      │
┌─────────────────────┼─────────────────────────────────────────────────────┐
│                     ▼    Notification Service DB                           │
│  ┌──────────────────────────────────┐  ┌──────────────────────┐         │
│  │         Notifications            │  │    EmailTemplates    │         │
│  │──────────────────────────────────│  │──────────────────────│         │
│  │ Id (PK)                         │  │ Id (PK)             │         │
│  │ UserId ───────────────────────→ Users.Id  │ Name (UNIQUE)        │         │
│  │ Type (Email/SMS/Push)           │  │ Subject             │         │
│  │ Channel (BookingConfirmation...)│  │ BodyTemplate        │         │
│  │ Subject                         │  │ IsActive            │         │
│  │ Body                            │  └──────────────────────┘         │
│  │ IsRead                          │                                   │
│  │ IsSent                          │                                   │
│  └──────────────────────────────────┘                                   │
└──────────────────────────────────────────────────────────────────────────┘
```

## Relationship Summary

### Identity Service
- `Users` 1:N `RefreshTokens` (هر کاربر چندین Refresh Token دارد)

### Events Service
- `Venues` 1:N `Events` (هر مکان چندین رویداد دارد)
- `EventCategories` 1:N `Events` (هر دسته چندین رویداد دارد)
- `EventCategories` 1:N `EventCategories` (دسته‌بندی سلسله‌مراتبی)
- `Events` 1:N `TicketTypes` (هر رویداد چندین نوع بلیط دارد)

### Booking Service
- `Orders` 1:N `OrderItems` (هر سفارش چندین آیتم دارد)
- `Orders` 1:N `Payments` (هر سفارش یک یا چند پرداخت دارد)
- `Orders` 1:N `OrderStatusHistory` (تاریخچه تغییرات سفارش)

### Cross-Service Relationships (via API)
- `Orders.UserId` → `Users.Id` (Identity Service)
- `OrderItems.TicketTypeId` → `TicketTypes.Id` (Events Service)
- `OrderItems.EventId` → `Events.Id` (Events Service)
- `Notifications.UserId` → `Users.Id` (Identity Service)

---

## Key Constraints

### Domain Rules
1. **Orders.TotalAmount** = SUM(OrderItems.TotalPrice)
2. **TicketTypes.SoldCount** ≤ **TicketTypes.Quantity**
3. **Orders.ExpiresAt** = CreatedAt + 10 minutes (for pending orders)
4. **OrderItems.Quantity** ≤ **TicketTypes.MaxPerOrder**

### Concurrency Rules
1. **Optimistic:** `Orders.RowVersion` و `OrderItems.RowVersion`
2. **Pessimistic:** `UPDLOCK` در Stored Procedure برای رزرو صندلی
3. **Isolation Level:** Serializable برای عملیات رزرو

---

## تمرین: خودتان ERD را بکشید!

ابزارهای پیشنهادی:
1. **dbdiagram.io** - آنلاین و رایگان
2. **Draw.io** - رایگان و آفلاین
3. **SSMS** - Database Diagram

### چالش:
1. ERD بالا را در یکی از ابزارها بکشید
2. روابط 1:1، 1:N، N:M را مشخص کنید
3. فیلدهای Index شده را رنگ کنید
4. فیلدهای foreign key را با فلش نشان دهید
