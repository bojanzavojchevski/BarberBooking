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
