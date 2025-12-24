# 📘 Week 03 — Catalog Domain & Business Structure

**Goal:** Establish the core business structure of the barber booking platform.
This week focuses on modeling the real-world catalog (shops, services, barbers)
with strict ownership rules, soft deletion, and clean architectural boundaries.
No scheduling or booking logic is introduced.

## Day 1 — Catalog Domain Foundation

- Introduced core catalog domain entities:
  - `Shop`
  - `Service`
  - `Barber`
- Enforced domain invariants directly in entities:
  - Valid owner and shop relationships
  - Name and length constraints
  - Duration rules for services (5–480 minutes, multiple of 5)
  - Safe, normalized slugs for public shop discovery
- Added cross-cutting domain abstractions:
  - `AuditableEntity` for created/updated metadata
  - `ISoftDeletable` for logical deletion
- Introduced `Money` value object for service pricing:
  - Immutable
  - Centralized validation and rounding
  - No persistence or framework dependencies
- Implemented EF Core configurations for catalog entities:
  - Explicit table mappings (`shops`, `services`, `barbers`)
  - Required fields and column constraints
  - Global query filters to exclude soft-deleted records
- Enforced database-level uniqueness:
  - One shop per owner
  - Unique shop slug for public discovery
  - Unique service names per shop (case-insensitive)
  - Unique barber display names per shop (case-insensitive)
- Implemented normalization strategy for case-insensitive uniqueness:
  - Shadow properties (`normalized_name`, `normalized_display_name`)
  - Centralized normalization in `AppDbContext` during SaveChanges
- Added DbSets for catalog entities and wired configurations via assembly scanning
- Applied and verified PostgreSQL migrations for all catalog tables
- Confirmed clean Onion/Clean Architecture boundaries:
  - No EF Core or ASP.NET dependencies in Domain
  - No business logic in controllers
  - Infrastructure concerns isolated to persistence layer

---

## Day 2 — Ownership Write Path (Create My Shop)

- Introduced ownership-aware write use case:
  - `CreateMyShopUseCase` in Application layer
- Enforced business rule:
  - A single owner can create **only one shop**
- Implemented Application-layer abstractions:
  - `IShopRepository`
  - `IUnitOfWork`
  - `ICurrentUser` (identity abstraction, no HTTP/JWT dependency)
- Implemented Infrastructure persistence:
  - `ShopRepository` using EF Core
  - `UnitOfWork` for transactional consistency
- Wired ownership resolution:
  - Owner identity resolved via `ICurrentUser`
  - No direct access to `HttpContext` outside WebApi
- Added Owner-scoped API endpoint:
  - `POST /api/owner/shop`
  - Protected with `OwnerOnly` authorization policy
- Ensured correct authorization behavior:
  - Owner role → allowed
  - Non-owner role → `403 Forbidden`
  - Unauthenticated → `401 Unauthorized`
- Added conflict handling:
  - Attempting to create a second shop → `409 Conflict`
- Verified full end-to-end behavior via Postman:
  - JWT authentication
  - Role-based authorization
  - Correct persistence and response DTO mapping
- Preserved strict Clean/Onion Architecture boundaries:
  - Controllers delegate to use cases only
  - No EF Core or Identity leakage into Application or Domain

---

## Day 3 — Owner Service Management (Create Service)

- Introduced owner-managed service creation flow scoped to a shop
- Implemented service creation use case in the Application layer:
  - `CreateServiceUseCase`
  - Explicit request/response contracts (`CreateServiceRequest`, `ServiceDto`)
- Enforced critical business rules:
  - Services must belong to the owner’s shop
  - Service names must be unique per shop (case-insensitive)
  - Duration and pricing validated via domain invariants
- Added persistence abstraction:
  - `IServiceRepository` in Application
  - EF Core–based `ServiceRepository` in Infrastructure
- Implemented normalized-name uniqueness checks prior to persistence
- Exposed owner-only API endpoint:
  - `POST /api/owner/services`
  - Protected via `OwnerOnly` authorization policy
- Ensured correct HTTP semantics:
  - `200 OK` on success
  - `409 Conflict` on duplicate service name
  - `403 Forbidden` for non-owner access
  - `401 Unauthorized` for unauthenticated requests
- Verified full end-to-end behavior via Postman:
  - JWT authentication
  - Role-based authorization
  - Correct persistence and DTO mapping
- Preserved strict Clean/Onion Architecture boundaries:
  - No EF Core or Identity dependencies in Application
  - Controllers delegate exclusively to use cases

---

## Day 4 — Owner Service Management (Read & Update)

- Implemented owner-scoped service management endpoints
- Added read path for listing services belonging to the owner’s shop:
  - `GET /api/owner/services`
- Implemented update flow for existing services:
  - `PUT /api/owner/services/{id}`
  - Supports updating name, duration, and price
- Enforced strict ownership rules in Application layer:
  - Services must belong to the owner’s shop
  - Cross-tenant access returns `404` without leaking data
- Preserved service name uniqueness per shop (case-insensitive):
  - Duplicate updates return `409 Conflict`
- Ensured correct HTTP semantics:
  - `200 OK` on success
  - `404 NotFound` for missing shop or service
  - `409 Conflict` for duplicate service names
- Verified full end-to-end behavior via Postman using JWT authentication
- Maintained strict Clean/Onion Architecture boundaries:
  - Controllers delegate to use cases only
  - No EF Core or Identity leakage into Application layer



