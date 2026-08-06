# Graph Report - D:\EventBride  (2026-08-06)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 1169 nodes · 2030 edges · 104 communities (88 shown, 16 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 64 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- AbstractValidator
- RefreshToken
- NotificationDbContext
- InitialBooking
- GlobalExceptionMiddleware
- EventsController
- .Register
- Event
- Order
- compilerOptions
- TicketType
- ApiResponse
- EventBride.slnx
- .Handle
- dependencies
- Events.Application.Common.Models
- ApiResponse
- compilerOptions
- IEventRepository
- Identity.Application.DTOs
- IOrderRepository
- ApiResponse
- devDependencies
- CacheService
- Events.Domain.Interfaces
- EventSummaryResponse
- Identity.Domain.Entities
- .Handle
- Events.API
- IRequest
- .Handle
- Booking.Domain.Interfaces
- Identity.Application
- router.tsx
- ICacheService
- GlobalExceptionMiddleware
- GlobalExceptionMiddleware
- TicketInventoryTests
- net10.0
- http
- http
- http
- Microsoft.NET.Sdk
- http
- .GenerateTokensAsync
- http
- plugins
- package.json
- ValidationBehavior
- TicketInventoryService
- Identity.API
- GetUserByIdQueryHandler
- react
- event.types.ts
- ConfirmPaymentCommandHandler
- .Handle
- booking.types.ts
- Identity.Application/DependencyInjection.cs
- auth.types.ts
- Identity.Application/Common/Mappings/MappingProfile.cs
- Events.Application/DependencyInjection.cs
- ValidationBehavior
- LogoutCommandHandler
- ValidationBehavior
- CheckoutPage.tsx
- ErrorBoundary.tsx
- Toast.tsx
- Events.API/Program.cs
- .AddCommonCaching
- RegisterPage.tsx
- SeatSelectionPage.tsx
- TicketsPage.tsx
- Common.Logging
- LoginPage.tsx
- authStore.ts
- EventsPage.tsx
- authApi.ts
- bookingApi.ts
- eventsApi.ts
- mockData.ts
- EventCard.tsx
- axios.ts
- MainLayout.tsx
- EmptyState.tsx
- Input.tsx
- tsconfig.json
- date-fns-jalali
- react-hook-form
- queryClient.ts
- tokenStorage.ts

## God Nodes (most connected - your core abstractions)
1. `ApiResponse` - 34 edges
2. `Event` - 26 edges
3. `Order` - 25 edges
4. `net10.0` - 19 edges
5. `ApiResponse` - 19 edges
6. `OrderResponse` - 19 edges
7. `ApiResponse` - 19 edges
8. `compilerOptions` - 19 edges
9. `IEventRepository` - 18 edges
10. `Booking.API` - 16 edges

## Surprising Connections (you probably didn't know these)
- `Common.Caching` --references--> `net10.0`  [EXTRACTED]
  EventBride/src/BuildingBlocks/Common.Caching/Common.Caching.csproj → EventBride/EventBride/EventBride.csproj
- `Common.Caching` --references--> `Microsoft.NET.Sdk`  [EXTRACTED]
  EventBride/src/BuildingBlocks/Common.Caching/Common.Caching.csproj → EventBride/tests/Events.UnitTests/Events.UnitTests.csproj
- `Common.Logging` --references--> `net10.0`  [EXTRACTED]
  EventBride/src/BuildingBlocks/Common.Logging/Common.Logging.csproj → EventBride/EventBride/EventBride.csproj
- `Common.Logging` --references--> `Microsoft.NET.Sdk`  [EXTRACTED]
  EventBride/src/BuildingBlocks/Common.Logging/Common.Logging.csproj → EventBride/tests/Events.UnitTests/Events.UnitTests.csproj
- `EventBus.RabbitMQ` --references--> `net10.0`  [EXTRACTED]
  EventBride/src/BuildingBlocks/EventBus.RabbitMQ/EventBus.RabbitMQ.csproj → EventBride/EventBride/EventBride.csproj

## Import Cycles
- None detected.

## Communities (104 total, 16 thin omitted)

### Community 0 - "AbstractValidator"
Cohesion: 0.08
Nodes (35): AbstractValidator, Events.Application.Validators, Booking.Application.Validators, Identity.Application.Validators, OrdersController, CancellationToken, HttpGet, HttpPost (+27 more)

### Community 1 - "RefreshToken"
Cohesion: 0.09
Nodes (26): JwtSettings, DateTime, RefreshToken, DateTime, User, CancellationToken, Task, IRefreshTokenRepository (+18 more)

### Community 2 - "NotificationDbContext"
Cohesion: 0.06
Nodes (27): ConsumeContext, Notification.API.Persistence.Migrations, Notification.API.Entities, Notification.API.Persistence, Notification.API.Consumers, EventBus.RabbitMQ.Events, DbContext, DateTime (+19 more)

### Community 3 - "InitialBooking"
Cohesion: 0.06
Nodes (21): Booking.Infrastructure.Persistence.Migrations, Identity.Infrastructure.Persistence.Migrations, Booking.Infrastructure.Persistence, Events.Infrastructure.Persistence.Migrations, MigrationBuilder, InitialBooking, ModelBuilder, BookingDbContextModelSnapshot (+13 more)

### Community 4 - "GlobalExceptionMiddleware"
Cohesion: 0.08
Nodes (21): Assembly, Common.Logging, Booking.Infrastructure, Booking.API.Middleware, EventBus.RabbitMQ, Booking.Application, Booking.Application.BackgroundJobs, SerilogExtensions (+13 more)

### Community 5 - "EventsController"
Cohesion: 0.15
Nodes (22): AllowAnonymous, Authorize, CancellationToken, HttpGet, HttpPost, IActionResult, IMediator, ProducesResponseType (+14 more)

### Community 6 - ".Register"
Cohesion: 0.11
Nodes (20): ControllerBase, AllowAnonymous, Authorize, CancellationToken, HttpPost, IActionResult, IMediator, ProducesResponseType (+12 more)

### Community 7 - "Event"
Cohesion: 0.13
Nodes (17): DateTime, ICollection, Event, EventStatus, DateTime, ICollection, EventCategory, DateTime (+9 more)

### Community 8 - "Order"
Cohesion: 0.17
Nodes (16): Order, OrderItem, OrderStatusHistory, Payment, PaymentStatus, DateTime, ICollection, OrderStatus (+8 more)

### Community 9 - "compilerOptions"
Cohesion: 0.08
Nodes (25): compilerOptions, allowArbitraryExtensions, allowImportingTsExtensions, erasableSyntaxOnly, jsx, lib, module, moduleDetection (+17 more)

### Community 10 - "TicketType"
Cohesion: 0.19
Nodes (10): DateTime, TicketType, CancellationToken, IReadOnlyList, Task, ITicketTypeRepository, CancellationToken, IReadOnlyList (+2 more)

### Community 11 - "ApiResponse"
Cohesion: 0.15
Nodes (18): CancellationToken, Task, UserManager, LoginCommand, LoginCommandHandler, CancellationToken, Task, RefreshTokenCommand (+10 more)

### Community 12 - "EventBride.slnx"
Cohesion: 0.13
Nodes (21): EventBus.RabbitMQ, Booking.API, AutoMapper (16.2.0), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.0), Microsoft.EntityFrameworkCore.Design (10.0.0), Microsoft.EntityFrameworkCore.SqlServer (10.0.0), Swashbuckle.AspNetCore.SwaggerGen (8.0.0), Swashbuckle.AspNetCore.SwaggerUI (8.0.0) (+13 more)

### Community 13 - ".Handle"
Cohesion: 0.17
Nodes (12): Availability, CreateOrderCommand, CreateOrderCommandHandler, CancellationToken, List, Task, TimeSpan, ITicketInventoryService (+4 more)

### Community 14 - "dependencies"
Cohesion: 0.10
Nodes (21): axios, date-fns, dependencies, axios, date-fns, @fontsource/vazirmatn, @hookform/resolvers, react (+13 more)

### Community 15 - "Events.Application.Common.Models"
Cohesion: 0.13
Nodes (14): Events.API.Controllers, Events.Application.Commands.Tickets, Events.Application.Queries.Events, Events.Application.Commands.Events, Events.Application.Common.Models, Events.Application.DTOs, Events.Application.Queries.Tickets, Events.Application.Common.Mappings (+6 more)

### Community 16 - "ApiResponse"
Cohesion: 0.19
Nodes (14): Booking.Application.Common.Models, ApiResponse, List, PagedResult, IReadOnlyList, OrderResponse, GetMyOrdersQuery, GetMyOrdersQueryHandler (+6 more)

### Community 17 - "compilerOptions"
Cohesion: 0.10
Nodes (19): compilerOptions, allowImportingTsExtensions, erasableSyntaxOnly, lib, module, moduleDetection, noEmit, noFallthroughCasesInSwitch (+11 more)

### Community 18 - "IEventRepository"
Cohesion: 0.22
Nodes (8): CancellationToken, Task, DeleteEventCommand, DeleteEventCommandHandler, CancellationToken, IReadOnlyList, Task, IEventRepository

### Community 19 - "Identity.Application.DTOs"
Cohesion: 0.22
Nodes (8): Identity.Infrastructure, Identity.Application.Common.Models, Identity.API.Middleware, Identity.Application.Queries.Users, Identity.Application.Common.Interfaces, Identity.Application.Commands.Auth, Identity.Application.DTOs, Identity.API.Controllers

### Community 20 - "IOrderRepository"
Cohesion: 0.24
Nodes (8): ReservationCleanupJob, ILogger, Task, IOrderRepository, CancellationToken, DateTime, IReadOnlyList, Task

### Community 21 - "ApiResponse"
Cohesion: 0.24
Nodes (11): CancellationToken, Task, CreateEventCommand, CreateEventCommandHandler, List, ApiResponse, EventResponse, CancellationToken (+3 more)

### Community 22 - "devDependencies"
Cohesion: 0.12
Nodes (17): devDependencies, oxlint, tailwindcss, @tailwindcss/vite, @types/node, @types/react, @types/react-dom, vite (+9 more)

### Community 23 - "CacheService"
Cohesion: 0.23
Nodes (10): bool, CacheService, CancellationToken, Func, ILogger, Task, TimeSpan, IConnectionMultiplexer (+2 more)

### Community 24 - "Events.Domain.Interfaces"
Cohesion: 0.21
Nodes (7): Events.Infrastructure.Persistence, Events.Domain.Entities, Events.Infrastructure.Repositories, Events.Domain.Interfaces, IConfiguration, IServiceCollection, DependencyInjection

### Community 25 - "EventSummaryResponse"
Cohesion: 0.25
Nodes (11): EventSummaryResponse, CancellationToken, List, Task, GetEventsByOrganizerQuery, GetEventsByOrganizerQueryHandler, CancellationToken, List (+3 more)

### Community 26 - "Identity.Domain.Entities"
Cohesion: 0.22
Nodes (8): Identity.Infrastructure.Persistence, Identity.Infrastructure.Services, Identity.Domain.Interfaces, Identity.Domain.Entities, Identity.Infrastructure.Repositories, IConfiguration, IServiceCollection, DependencyInjection

### Community 27 - ".Handle"
Cohesion: 0.19
Nodes (9): Booking.API.Controllers, Booking.Application.Common.Mappings, Booking.Application.DTOs, Booking.Application.Queries.Orders, Booking.Application.Commands.Orders, CancelOrderCommand, CancelOrderCommandHandler, CancellationToken (+1 more)

### Community 28 - "Events.API"
Cohesion: 0.15
Nodes (14): Common.Caching, Events.API, Microsoft.AspNetCore.Authentication.JwtBearer (10.0.0), Microsoft.EntityFrameworkCore.Design (10.0.0), Microsoft.EntityFrameworkCore.SqlServer (10.0.0), Swashbuckle.AspNetCore.SwaggerGen (8.0.0), Swashbuckle.AspNetCore.SwaggerUI (8.0.0), Events.Application (+6 more)

### Community 29 - "IRequest"
Cohesion: 0.20
Nodes (10): CancellationToken, Task, ReleaseTicketsCommand, ReleaseTicketsCommandHandler, CancellationToken, Task, ReserveTicketsCommand, ReserveTicketsCommandHandler (+2 more)

### Community 30 - ".Handle"
Cohesion: 0.21
Nodes (9): Common.Caching, CacheKeys, IReadOnlyList, PagedResult, CancellationToken, Task, GetEventsQuery, GetEventsQueryHandler (+1 more)

### Community 31 - "Booking.Domain.Interfaces"
Cohesion: 0.19
Nodes (7): Booking.Infrastructure.Repositories, Booking.Domain.Entities, Booking.Domain.Interfaces, Booking.Infrastructure.Services, DependencyInjection, IConfiguration, IServiceCollection

### Community 32 - "Identity.Application"
Cohesion: 0.17
Nodes (13): Identity.Application, AutoMapper (14.0.1), AutoMapper.Extensions.Microsoft.DependencyInjection (12.0.1), FluentValidation (12.0.0), FluentValidation.DependencyInjectionExtensions (11.11.0), MediatR (14.2.0), Microsoft.Extensions.Identity.Stores (10.0.0), Identity.Domain (+5 more)

### Community 33 - "router.tsx"
Cohesion: 0.15
Nodes (10): CheckoutPage, EventDetailPage, EventsPage, LandingPage, LoginPage, NotFoundPage, RegisterPage, router (+2 more)

### Community 34 - "ICacheService"
Cohesion: 0.38
Nodes (5): ICacheService, CancellationToken, Func, Task, TimeSpan

### Community 35 - "GlobalExceptionMiddleware"
Cohesion: 0.36
Nodes (7): Exception, HttpContext, ILogger, RequestDelegate, Task, ValidationException, GlobalExceptionMiddleware

### Community 36 - "GlobalExceptionMiddleware"
Cohesion: 0.36
Nodes (7): Exception, HttpContext, ILogger, RequestDelegate, Task, ValidationException, GlobalExceptionMiddleware

### Community 37 - "TicketInventoryTests"
Cohesion: 0.31
Nodes (3): Events.UnitTests, TicketInventoryTests, Fact

### Community 38 - "net10.0"
Cohesion: 0.24
Nodes (9): net10.0, Microsoft.NET.Sdk.Web, ApiGateway, Notification.API, Microsoft.EntityFrameworkCore.Design (10.0.0), Microsoft.EntityFrameworkCore.SqlServer (10.0.0), Swashbuckle.AspNetCore.SwaggerGen (8.0.0), Swashbuckle.AspNetCore.SwaggerUI (8.0.0) (+1 more)

### Community 39 - "http"
Cohesion: 0.20
Nodes (9): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, profiles, http (+1 more)

### Community 40 - "http"
Cohesion: 0.20
Nodes (9): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, profiles, http (+1 more)

### Community 41 - "http"
Cohesion: 0.20
Nodes (9): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, profiles, http (+1 more)

### Community 42 - "Microsoft.NET.Sdk"
Cohesion: 0.24
Nodes (10): Events.Domain, Events.Infrastructure, Microsoft.EntityFrameworkCore.SqlServer (10.0.0), Microsoft.Extensions.Configuration.Abstractions (10.0.0), Events.UnitTests, Microsoft.NET.Sdk, coverlet.collector (6.0.4), Microsoft.NET.Test.Sdk (17.14.1) (+2 more)

### Community 43 - "http"
Cohesion: 0.20
Nodes (9): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, profiles, http (+1 more)

### Community 44 - ".GenerateTokensAsync"
Cohesion: 0.29
Nodes (7): AccessToken, CancellationToken, DateTime, ExpiresAt, RefreshExpiresAt, Task, ITokenService

### Community 45 - "http"
Cohesion: 0.20
Nodes (9): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, profiles, http (+1 more)

### Community 46 - "plugins"
Cohesion: 0.20
Nodes (9): plugins, rules, react/only-export-components, react/rules-of-hooks, $schema, typescript, oxc, warn (+1 more)

### Community 47 - "package.json"
Cohesion: 0.20
Nodes (9): name, private, scripts, build, dev, lint, preview, type (+1 more)

### Community 48 - "ValidationBehavior"
Cohesion: 0.22
Nodes (7): Booking.Application.Common.Behaviours, ValidationBehavior, CancellationToken, IEnumerable, RequestHandlerDelegate, Task, IPipelineBehavior

### Community 49 - "TicketInventoryService"
Cohesion: 0.33
Nodes (6): EventTicketDto, TicketInventoryService, CancellationToken, ILogger, Task, HttpClient

### Community 50 - "Identity.API"
Cohesion: 0.22
Nodes (9): Identity.API, AutoMapper (16.2.0), MediatR (14.2.0), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.0), Microsoft.AspNetCore.Identity.EntityFrameworkCore (10.0.0), Microsoft.EntityFrameworkCore.Design (10.0.0), Microsoft.EntityFrameworkCore.SqlServer (10.0.0), Swashbuckle.AspNetCore.SwaggerGen (8.0.0) (+1 more)

### Community 51 - "GetUserByIdQueryHandler"
Cohesion: 0.31
Nodes (7): UserResponse, CancellationToken, Task, UserManager, GetUserByIdQuery, GetUserByIdQueryHandler, IMapper

### Community 52 - "react"
Cohesion: 0.22
Nodes (3): Button, ButtonProps, react

### Community 53 - "event.types.ts"
Cohesion: 0.25
Nodes (7): EventCategory, EventDetail, EventsQueryParams, EventStatus, EventSummary, PagedResult, TicketType

### Community 54 - "ConfirmPaymentCommandHandler"
Cohesion: 0.36
Nodes (5): ConfirmPaymentCommand, ConfirmPaymentCommandHandler, CancellationToken, Task, IPublishEndpoint

### Community 55 - ".Handle"
Cohesion: 0.46
Nodes (4): CancellationToken, Task, UpdateEventCommand, UpdateEventCommandHandler

### Community 56 - "booking.types.ts"
Cohesion: 0.25
Nodes (6): ConfirmPaymentRequest, CreateOrderRequest, OrderItemRequest, OrderItemResponse, OrderResponse, PaymentResponse

### Community 57 - "Identity.Application/DependencyInjection.cs"
Cohesion: 0.29
Nodes (4): Identity.Application.Common.Behaviours, Identity.Application, IServiceCollection, DependencyInjection

### Community 58 - "auth.types.ts"
Cohesion: 0.29
Nodes (5): ApiResponse, AuthResponse, LoginPayload, RegisterPayload, User

### Community 60 - "Identity.Application/Common/Mappings/MappingProfile.cs"
Cohesion: 0.33
Nodes (5): Identity.Application.Common.Mappings, MappingProfile, MappingProfile, MappingProfile, Profile

### Community 61 - "Events.Application/DependencyInjection.cs"
Cohesion: 0.33
Nodes (3): Events.Application.Common.Behaviours, IServiceCollection, DependencyInjection

### Community 62 - "ValidationBehavior"
Cohesion: 0.33
Nodes (5): CancellationToken, IEnumerable, RequestHandlerDelegate, Task, ValidationBehavior

### Community 63 - "LogoutCommandHandler"
Cohesion: 0.47
Nodes (4): CancellationToken, Task, LogoutCommand, LogoutCommandHandler

### Community 64 - "ValidationBehavior"
Cohesion: 0.33
Nodes (5): CancellationToken, IEnumerable, RequestHandlerDelegate, Task, ValidationBehavior

### Community 65 - "CheckoutPage.tsx"
Cohesion: 0.33
Nodes (4): CheckoutForm, checkoutSchema, paymentMethods, trustBadges

### Community 66 - "ErrorBoundary.tsx"
Cohesion: 0.33
Nodes (3): ErrorBoundary, Props, State

### Community 67 - "Toast.tsx"
Cohesion: 0.33
Nodes (3): Toast, ToastContext, ToastContextValue

### Community 68 - "Events.API/Program.cs"
Cohesion: 0.40
Nodes (3): Events.API.Middleware, Events.Application, Events.Infrastructure

### Community 69 - ".AddCommonCaching"
Cohesion: 0.40
Nodes (3): DependencyInjection, IConfiguration, IServiceCollection

### Community 71 - "RegisterPage.tsx"
Cohesion: 0.50
Nodes (4): passwordStrength(), RegisterForm, RegisterPage(), registerSchema

### Community 72 - "SeatSelectionPage.tsx"
Cohesion: 0.50
Nodes (4): generateSeats(), seatColors, SeatSelectionPage(), SeatState

### Community 74 - "TicketsPage.tsx"
Cohesion: 0.40
Nodes (3): mockTickets, statusLabels, Ticket

### Community 75 - "Common.Logging"
Cohesion: 0.50
Nodes (4): Common.Logging, Serilog.AspNetCore (9.0.0), Serilog.Sinks.Console (6.0.0), Serilog.Sinks.Seq (9.0.0)

### Community 78 - "EventsPage.tsx"
Cohesion: 0.67
Nodes (3): EventsPage(), sortOptions, useDebouncedValue()

## Knowledge Gaps
- **231 isolated node(s):** `Microsoft.Extensions.Caching.StackExchangeRedis (10.0.0)`, `Serilog.AspNetCore (9.0.0)`, `Serilog.Sinks.Seq (9.0.0)`, `Serilog.Sinks.Console (6.0.0)`, `MassTransit.RabbitMQ (8.3.4)` (+226 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **16 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Identity.Application.Common.Models` connect `Identity.Application.DTOs` to `RefreshToken`, `Identity.Domain.Entities`, `ApiResponse`, `LogoutCommandHandler`?**
  _High betweenness centrality (0.055) - this node is a cross-community bridge._
- **Why does `Common.Logging` connect `GlobalExceptionMiddleware` to `NotificationDbContext`, `Identity.Application.DTOs`, `Events.API/Program.cs`?**
  _High betweenness centrality (0.036) - this node is a cross-community bridge._
- **Why does `ApiResponse` connect `ApiResponse` to `EventsController`, `Events.Application.Common.Models`, `IEventRepository`, `.Handle`, `EventSummaryResponse`, `IRequest`, `.Handle`?**
  _High betweenness centrality (0.033) - this node is a cross-community bridge._
- **What connects `Microsoft.Extensions.Caching.StackExchangeRedis (10.0.0)`, `Serilog.AspNetCore (9.0.0)`, `Serilog.Sinks.Seq (9.0.0)` to the rest of the system?**
  _231 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `AbstractValidator` be split into smaller, more focused modules?**
  _Cohesion score 0.07653061224489796 - nodes in this community are weakly interconnected._
- **Should `RefreshToken` be split into smaller, more focused modules?**
  _Cohesion score 0.08668076109936575 - nodes in this community are weakly interconnected._
- **Should `NotificationDbContext` be split into smaller, more focused modules?**
  _Cohesion score 0.06201550387596899 - nodes in this community are weakly interconnected._