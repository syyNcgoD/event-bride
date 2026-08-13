<div align="center">
  <h1>🎟️ EventBride</h1>
  <h3>Distributed Event Ticketing & Reservation Platform</h3>
  <p>Engineered for high-concurrency, data consistency, and fault tolerance in <strong>.NET 10</strong>.</p>

  <p>
    <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white" alt=".NET" />
    <img src="https://img.shields.io/badge/Architecture-Microservices-FF9900?logo=microgenetics&logoColor=white" alt="Microservices" />
    <img src="https://img.shields.io/badge/Message_Broker-RabbitMQ-FF6600?logo=rabbitmq&logoColor=white" alt="RabbitMQ" />
    <img src="https://img.shields.io/badge/Cache-Redis-DC382D?logo=redis&logoColor=white" alt="Redis" />
    <img src="https://img.shields.io/badge/Resilience-Polly_v8-00A98F?logo=polly&logoColor=white" alt="Polly" />
    <img src="https://img.shields.io/badge/Testing-Testcontainers-2496ED?logo=docker&logoColor=white" alt="Testcontainers" />
  </p>
</div>

---

## 🎯 The Engineering Context

Building a CRUD ticketing system is straightforward. Building one that survives a flash sale—where 50,000 users attempt to buy the last 100 tickets at the exact same millisecond—is a distributed systems nightmare. 

**EventBride** is not a basic tutorial project. It is a pragmatic implementation of enterprise-grade distributed systems patterns in the .NET ecosystem. It focuses entirely on the "hard parts" of backend engineering:
- Preventing race conditions and inventory overselling.
- Guaranteeing data consistency across microservices without distributed transactions (2PC).
- Designing resilient service boundaries that don't cascade failures.
- Managing state and background jobs across scaled-out instances.

---

## 🏗️ Architecture & Design Decisions

The solution enforces **Clean Architecture** (Domain ➔ Application ➔ Infrastructure ➔ API) and utilizes **CQRS** to physically separate read-heavy operations from write-heavy transactional flows.

```mermaid
graph TD
    Client[Client Applications] --> GW[YARP API Gateway]
    
    subgraph Services ["Core Services"]
        GW --> IS[Identity Service]
        GW --> ES[Events Service]
        GW --> BS[Booking Service]
        
        IS <--> IDB[(Identity DB)]
        ES <--> EDB[(Events DB)]
        BS <--> BDB[(Booking DB)]
    end
    
    subgraph Caching ["Caching Layer"]
        ES <== "Cache-Aside / Invalidate" ==> Redis[(Redis)]
    end

    subgraph Messaging ["Event Bus & Outbox"]
        BS -- "1. Atomic Commit (Order + Outbox)" --> BDB
        Outbox[MassTransit Outbox Worker] -- "2. Read Pending" --> BDB
        Outbox -- "3. Publish BookingConfirmed" --> RMQ((RabbitMQ))
        RMQ -- "4. Consume" --> NS[Notification Service]
        NS <--> NDB[(Notification DB)]
    end

    subgraph Background ["Background Processing"]
        Hangfire((Hangfire Scheduler)) -- "Acquire RedLock" --> Redis
        Hangfire -- "Release Expired Tickets" --> BS
    end
```