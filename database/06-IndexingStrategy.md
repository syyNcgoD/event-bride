# Indexing Strategy for EventBride

## فلسفه ایندکس‌گذاری

### 1. ایندکس‌های Single Column
روی فیلدهایی که در `WHERE`، `JOIN` یا `ORDER BY` استفاده می‌شوند.

```sql
-- مثال: جستجوی کاربر با ایمیل
CREATE INDEX IX_Users_Email ON [Users]([NormalizedEmail]);
```

### 2. ایندکس‌های Composite (ترکیبی)
روی فیلدهایی که همیشه با هم در query استفاده می‌شوند.

```sql
-- مثال: سفارشات یک کاربر با وضعیت خاص
CREATE INDEX IX_Orders_UserId_Status ON [Orders]([UserId], [Status]);
```

**نکته مهم:** ترتیب فیلدها مهم است!
- فیلدهایی با `Equality` (=) اول بیایند
- فیلدهایی با `Range` (>, <, BETWEEN) آخر بیایند

### 3. ایندکس‌های Covering
شامل تمام فیلدهایی باشد که query نیاز دارد تا از Key Lookup جلوگیری کند.

```sql
-- مثال: دریافت اطلاعات پایه رویداد
CREATE INDEX IX_Events_Covering
ON [Events]([Status], [StartDate])
INCLUDE ([Title], [VenueId], [CategoryId]);
```

---

## ایندکس‌های هر جدول

### Users Table
| ایندکس | نوع | دلیل |
|--------|------|-------|
| `PK_Users` | Clustered | کلید اصلی |
| `IX_Users_Email` | Unique | جستجوی کاربر با ایمیل (Login) |
| `IX_Users_CreatedAt` | Non-Clustered | گزارش کاربران جدید |

### RefreshTokens Table
| ایندکس | نوع | دلیل |
|--------|------|-------|
| `PK_RefreshTokens` | Clustered | کلید اصلی |
| `IX_RefreshTokens_Token` | Non-Clustered | اعتبارسنجی توکن (خیلی پرتکرار) |
| `IX_RefreshTokens_UserId` | Non-Clustered | دریافت توکن‌های یک کاربر |
| `IX_RefreshTokens_IsUsed_IsRevoked` | Composite | فیلتر توکن‌های معتبر |

### Events Table
| ایندکس | نوع | دلیل |
|--------|------|-------|
| `PK_Events` | Clustered | کلید اصلی |
| `IX_Events_Status_StartDate` | Composite | لیست رویدادهای فعال (پرتکرارترین query) |
| `IX_Events_VenueId` | Non-Clustered | رویدادهای یک مکان |
| `IX_Events_CategoryId` | Non-Clustered | فیلتر بر اساس دسته‌بندی |
| `IX_Events_OrganizerId_Status` | Composite | رویدادهای یک برگزارکننده |

### TicketTypes Table
| ایندکس | نوع | دلیل |
|--------|------|-------|
| `PK_TicketTypes` | Clustered | کلید اصلی |
| `IX_TicketTypes_EventId` | Non-Clustered | انواع بلیط یک رویداد |
| `IX_TicketTypes_Price` | Non-Clustered | فیلتر بر اساس قیمت |

### Orders Table
| ایندکس | نوع | دلیل |
|--------|------|-------|
| `PK_Orders` | Clustered | کلید اصلی |
| `IX_Orders_OrderNumber` | Unique | جستجو با شماره سفارش |
| `IX_Orders_UserId_Status` | Composite | سفارشات کاربر (پرتکرار) |
| `IX_Orders_Status_CreatedAt` | Composite | گزارش و جستجوی تاریخ |
| `IX_Orders_ExpiresAt` | Non-Clustered | پیدا کردن رزروهای منقضی (Hangfire) |

### OrderItems Table
| ایندکس | نوع | دلیل |
|--------|------|-------|
| `PK_OrderItems` | Clustered | کلید اصلی |
| `IX_OrderItems_OrderId` | Non-Clustered | آیتم‌های یک سفارش |
| `IX_OrderItems_TicketTypeId` | Non-Clustered | بررسی فروش هر نوع بلیط |
| `IX_OrderItems_EventId` | Non-Clustered | گزارش فروش رویداد |

---

## نحوه بررسی Execution Plan

### در SSMS:
1. query را بنویسید
2. کلید `Ctrl + M` را بزنید (Include Actual Execution Plan)
3. query را اجرا کنید
4. تب `Execution Plan` را ببینید

### نکات مهم در Execution Plan:

#### 1. Table Scan vs Index Scan vs Index Seek
```
Table Scan     → بد! ایندکس ندارد
Index Scan     → قابل قبول (خیلی از ردیف‌ها را می‌خواند)
Index Seek      → بهترین (فقط ردیف‌های مورد نیاز)
```

#### 2. Key Lookup
```
Key Lookup     → ایندکس کامل نیست (Covering نیست)
Solution       → ایندکس را با INCLUDE کامل کنید
```

#### 3. Cost Percentage
```
بالاترین درصد  → گلوگاه اصلی query
اینجا را بهینه کنید
```

---

## تمرین عملی

### Step 1: اجرای SP گزارش
```sql
-- ابتدا چند داده نمونه اضافه کنید
-- سپس SP را اجرا کنید
EXEC [dbo].[usp_GetTopSellingEvents] @Top = 10;
```

### Step 2: بررسی Execution Plan
```sql
-- با Include Actual Execution Plan اجرا کنید
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

EXEC [dbo].[usp_GetTopSellingEvents] @Top = 10;
```

### Step 3: تحلیل
سوالات زیر را جواب دهید:
1. آیا از ایندکس `IX_OrderItems_EventId` استفاده می‌کند؟
2. آیا Key Lookup دارد؟
3. Cost اصلی کجاست؟
4. اگر `@Top = 1000` کنیم چه تغییری می‌کند؟

### Step 4: بهینه‌سازی
اگر Index Scan دارید، یک Covering Index بسازید:
```sql
CREATE NONCLUSTERED INDEX [IX_OrderItems_EventId_Covering]
ON [OrderItems]([EventId])
INCLUDE ([OrderId], [Quantity], [UnitPrice], [TotalPrice], [EventTitle], [TicketTypeName]);
```

دوباره اجرا کنید و Execution Plan را مقایسه کنید!
