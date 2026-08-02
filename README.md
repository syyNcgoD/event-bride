# EventBride - Online Ticketing Platform

## Architecture

Microservices-based architecture with Clean Architecture pattern per service.

## Solution Structure

```
EventBride/
├── src/
│   ├── Services/
│   │   ├── Identity/          (JWT + ASP.NET Identity)
│   │   │   ├── Identity.API
│   │   │   ├── Identity.Application  (CQRS + MediatR)
│   │   │   ├── Identity.Domain
│   │   │   └── Identity.Infrastructure (EF Core)
│   │   ├── Events/            (Catalog management)
│   │   │   ├── Events.API
│   │   │   ├── Events.Application
│   │   │   ├── Events.Domain
│   │   │   └── Events.Infrastructure
│   │   ├── Booking/           (Reservations + Payments)
│   │   │   ├── Booking.API
│   │   │   ├── Booking.Application
│   │   │   ├── Booking.Domain
│   │   │   └── Booking.Infrastructure
│   │   └── Notification/      (Email + SMS)
│   │       ├── Notification.API
│   │       └── Notification.Worker
│   ├── Gateway/
│   │   └── ApiGateway/        (Ocelot/YARP)
│   └── BuildingBlocks/
│       ├── EventBus.RabbitMQ/ (Shared messaging)
│       ├── Common.Logging/    (Serilog config)
│       └── Common.Caching/    (Redis wrapper)
├── tests/
│   ├── Booking.UnitTests/
│   ├── Booking.IntegrationTests/
│   └── Events.UnitTests/
└── docker-compose.yml
```

## Tech Stack

- **Backend:** .NET 10, ASP.NET Core, CQRS, MediatR
- **Database:** SQL Server, EF Core, Redis
- **Messaging:** RabbitMQ
- **Auth:** JWT + Refresh Token
- **API Gateway:** Ocelot/YARP
- **Monitoring:** Serilog, Seq, OpenTelemetry, Grafana
- **CI/CD:** GitHub Actions → Azure

## Quick Start

```bash
docker-compose up
```

## Development

- `main` - Production-ready code
- `develop` - Integration branch
- `feature/*` - Feature branches
