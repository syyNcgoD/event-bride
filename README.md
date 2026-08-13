# EventBride Microservices Platform

EventBride is a production-grade event ticketing and booking platform. It was built to demonstrate how to handle complex distributed system challenges—specifically concurrent transactions, distributed caching, and event-driven communication—using the .NET 10 ecosystem. 

Rather than just another CRUD application, EventBride focuses heavily on tackling the "double-booking" problem under high concurrency and ensuring data consistency across multiple microservices.

## Architecture & Core Design

The system relies on a Microservices architecture, where each service strictly adheres to Clean Architecture principles (Domain -> Application -> Infrastructure -> API).

- **Identity Service**: Handles authentication and JWT generation (ASP.NET Core Identity).
- **Events Service**: Manages events, ticket types, and inventory. Implements caching and pessimistic locking.
- **Booking Service**: Manages reservations, payments, and distributed transactions.
- **Notification Service**: A decoupled worker that consumes events and handles outbound communications.

### Key Engineering Decisions

1. **Concurrency & Locking**: 
   To prevent overselling tickets during high-traffic surges, the Events Service uses SQL Server pessimistic locking (`WITH (UPDLOCK, ROWLOCK)`) at the row level. This ensures atomic ticket decrements. We also use optimistic concurrency (RowVersion) for order state management.

2. **Distributed Transactions & Outbox Pattern**:
   When a booking is confirmed, we need to update the local database and publish an event to RabbitMQ. To avoid dual-write issues (e.g. if RabbitMQ is down), we implemented the Transactional Outbox pattern using MassTransit and EF Core. 

3. **Resilience & Fault Tolerance**:
   Service-to-service HTTP calls are wrapped with Polly v8 resilience pipelines, implementing strict timeouts, exponential backoffs, and circuit breakers to prevent cascading failures.

4. **Distributed Caching (Cache-Aside)**:
   High-traffic endpoints (like the events catalog) are cached in Redis. We built a custom fallback mechanism so that if Redis is temporarily unreachable, it seamlessly degrades to in-memory caching.

5. **Background Processing**:
   We use Hangfire for scheduled and background jobs. For example, a `ReservationCleanupJob` runs periodically to identify pending orders older than 10 minutes, cancel them, and release the locked tickets back to the inventory pool.

6. **Distributed Locking (RedLock)**:
   In cases where multiple Hangfire instances might try to clean up the same resources, we utilize a Redis-based distributed lock to ensure only one instance processes the cleanup at a time.

## Tech Stack

- **Framework**: .NET 10.0
- **Database**: SQL Server & Entity Framework Core
- **Message Broker**: RabbitMQ (via MassTransit)
- **Caching & Locking**: Redis
- **Background Jobs**: Hangfire
- **Resilience**: Polly v8
- **Observability**: Serilog & Seq

## Getting Started

To run the platform locally, you'll need the .NET 10 SDK and Docker Desktop installed.

1. **Spin up the infrastructure**:
   ```bash
   docker run -d --name eventbride-redis -p 6379:6379 redis:7-alpine
   docker run -d --name eventbride-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   docker run -d --name eventbride-seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
   ```

2. **Run the microservices**:
   Open separate terminals and start each service from the root directory:
   ```bash
   dotnet run --project src/Services/Identity/Identity.API
   dotnet run --project src/Services/Events/Events.API
   dotnet run --project src/Services/Booking/Booking.API
   dotnet run --project src/Services/Notification/Notification.API
   ```
   *Note: EF Core migrations will run automatically on startup to seed the databases.*

3. **Access APIs**:
   Each service exposes a Swagger UI:
   - Identity: `http://localhost:5001/swagger`
   - Events: `http://localhost:5002/swagger`
   - Booking: `http://localhost:5003/swagger`
   - Hangfire Dashboard: `http://localhost:5003/hangfire`

## Project Knowledge Graph

If you want to explore the architecture interactively, we've indexed the codebase with Graphify. You can view the full dependency graph and community boundaries [here](docs/graph/graph.html), or read the [graph report](docs/graph/GRAPH_REPORT.md).