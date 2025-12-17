# 📘 Week 1 - Foundation & Skeleton

## Overview
Week 1 focuses on establishing a **production-grade foundation** for the BarberBooking application.  
The goal is to set up a clean architecture, properly separated layers, infrastructure, observability, and an automated CI pipeline - **before implementing any business features**.

No functional features are implemented during this week. Everything built in subsequent weeks relies on this foundation.

---

## 🎯 Goals for Week 1
- Establish **Onion Architecture** skeleton
- Configure **PostgreSQL + EF Core + migrations**
- Basic observability (**logging, health checks**)
- **Global exception handling**
- **OpenAPI (Swagger)**
- **GitHub CI pipeline** (build, test, format)

---

## 🧱 Solution Structure (Onion Architecture)

```text
BarberBooking
├── BarberBooking.Domain
├── BarberBooking.Application
├── BarberBooking.Infrastructure
├── BarberBooking.WebApi
└── BarberBooking.sln
```

---

## 1️⃣ BarberBooking.Domain
Pure domain layer, free of external dependencies.

Contains:
- Entities (e.g. Appointment)
- ValueObjects (e.g. TimeRange)
- Domain exceptions

---

## 2️⃣ BarberBooking.Application
Application / use-case layer.

Contains:
- Application services / use-cases
- Interfaces (repositories, services)
- DTOs

---

## 3️⃣ BarberBooking.Infrastructure
Technical infrastructure implementations.

Contains:
- EF Core AppDbContext
- PostgreSQL configuration
- EF Core migrations
- Dependency Injection extensions

---

## 4️⃣ BarberBooking.WebApi
Delivery layer (ASP.NET Core Web API).

Contains:
- Controllers
- Middleware
- Swagger / OpenAPI
- Health checks
- Program.cs (composition root)

---

## 🗄️ Database & Migrations

- PostgreSQL is used as the primary database
- EF Core is used as the ORM
- Migrations are stored in the Infrastructure layer

Commands:
```bash
dotnet ef migrations add InitialCreate \
  --project BarberBooking.Infrastructure \
  --startup-project BarberBooking.WebApi

dotnet ef database update \
  --project BarberBooking.Infrastructure \
  --startup-project BarberBooking.WebApi
```

---

## 🪵 Logging (Serilog)
 - Serilog is configured as a host-level logger
 - Console sink for development
 - Automatic HTTP request logging

---

## 🚨 Global Exception Handling

A custom exception middleware is implemented that:
 - Catches all unhandled exceptions
 - Returns a standardized JSON error response
 - Generates a traceId for debugging

---

## ❤️ Health Checks

Implemented endpoints:
 - /health/live - liveness check
 - /health/ready - readiness + database check

---

## 📄 OpenAPI (Swagger)

 - Implemented using Swashbuckle.AspNetCore
 - Swagger UI available in development
 - Endpoint: /swagger

---

## 🔁 Continuous Integration (GitHub Actions)

A CI pipeline is configured with the following steps:
 - dotnet restore
 - dotnet build
 - dotnet test
 - dotnet format --verifiy-no-changes
