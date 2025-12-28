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

## Day 4 — Owner Service Management (Read, Update, Toggle, Delete)

- Completed owner-scoped service management capabilities
- Implemented service listing for owner’s shop:
  - `GET /api/owner/services`
- Added update flow for existing services:
  - `PUT /api/owner/services/{id}`
  - Supports updating name, duration, and price
- Enforced strict ownership boundaries in Application layer:
  - Services must belong to the owner’s shop
  - Cross-tenant access safely returns `404 Not Found`
- Preserved case-insensitive uniqueness during updates:
  - Duplicate service names return `409 Conflict`
- Implemented service activation lifecycle:
  - `PATCH /api/owner/services/{id}/active`
  - Enables enabling/disabling services without deletion
- Implemented soft deletion for services:
  - `DELETE /api/owner/services/{id}`
  - Deleted services are hidden via global query filters
- Ensured correct HTTP semantics across all endpoints:
  - `200 OK`, `204 No Content`, `404 Not Found`, `409 Conflict`
- Verified full end-to-end behavior via Postman using JWT authentication
- Maintained strict Clean/Onion Architecture boundaries:
  - No EF Core or HTTP dependencies in Application layer
  - Controllers remain thin and delegate to use cases only

---

## Day 5 — Barbers Catalog (CRUD + Ownership)

- Implemented Barber catalog domain:
  - `Barber` entity with invariants for display name and bio
  - Soft deletion via `ISoftDeletable`
  - Active/inactive lifecycle support
- Added EF Core persistence:
  - `BarberConfiguration` with table mapping and constraints
  - Case-insensitive uniqueness per shop using `normalized_display_name`
  - Global query filter excluding soft-deleted barbers
- Implemented Application use cases:
  - Create barber
  - List owner’s barbers
  - Update barber details and active status
- Enforced ownership and authorization:
  - All operations scoped to owner’s shop
  - Protected by `OwnerOnly` policy
- Implemented REST endpoints:
  - `POST /api/owner/barbers`
  - `GET /api/owner/barbers`
  - `PUT /api/owner/barbers/{id}`
- Verified behavior via Postman:
  - Correct CRUD behavior
  - Case-insensitive uniqueness enforcement
  - Proper HTTP status codes (`200`, `400`, `401`, `403`, `409`)
- Confirmed production readiness:
  - `dotnet build` and `dotnet test` passing
  - Clean Onion/Clean Architecture boundaries preserved

---

## Day 6 — Public Catalog (Read-only Browsing)

- Implemented public, anonymous read-only catalog browsing (no authentication required)
- Added Application read abstraction for projections:
  - `IPublicCatalogReadService` (read-only, DTO-based)
- Introduced public read DTOs (no EF entities exposed):
  - `ShopListItemDto`, `ShopPublicDto`, `ShopPublicDetailsDto`
  - `ServicePublicDto`, `BarberPublicDto`
- Added public query contract:
  - `GetPublicShopsQuery` (search + pagination)
- Implemented Infrastructure read projections using EF Core:
  - `AsNoTracking()` for read performance
  - `Select(...)` projections (no `Include`, no entity graph exposure)
  - Fixed query count for details view (no N+1)
- Enforced public visibility rules:
  - Only `IsActive = true` shops are returned
  - Only `IsActive = true` services and barbers are returned
  - Soft-deleted records excluded via global query filters (`ISoftDeletable`)
- Exposed public API endpoints:
  - `GET /api/public/shops` (paged list + optional `q` search)
  - `GET /api/public/shops/{slug}` (shop details + active services + active barbers)
- Verified behavior via Postman (no JWT required)
- Maintained strict Clean/Onion Architecture boundaries:
  - Application defines DTOs + contracts
  - Infrastructure implements projections
  - WebApi exposes endpoints only

---

## Day 7 — Catalog Integration Tests + Week Review (Definition of Done)

- Added integration test host for the API using Testcontainers + PostgreSQL:
  - Shared `TestAppFactory` based on `WebApplicationFactory<Program>`
  - Automatic migrations applied for test database
- Added deterministic database seeding for catalog scenarios:
  - Active vs inactive shops
  - Active vs inactive services/barbers
  - Soft-deleted entities to validate global query filters
- Implemented integration tests proving catalog correctness:
  - Public shop list returns only active, non-deleted shops
  - Public shop details by slug returns only active services and active/non-deleted barbers
  - Inactive shop returns `404 Not Found`
  - Public search (`q`) filters shops by name/slug
- Refactored existing Auth integration tests to reuse the shared test host (single containerized environment)
- Verified Definition of Done:
  - `dotnet build` succeeds
  - `dotnet test` succeeds with Docker/Testcontainers
  - No EF entities leak from API responses (DTO-only)
  - Soft delete filters enforced consistently
  - Clean architecture boundaries preserved



