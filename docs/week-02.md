# 📘 Week 02 — Identity & Security Baseline

**Goal:** Establish a production-grade authentication and authorization baseline.
This week focuses on secure identity management, token-based authentication,
defense-in-depth security mechanisms, and strict separation of concerns
using Onion/Clean Architecture principles.

## Day 1 — Identity Wiring
- Integrated ASP.NET Core Identity with EF Core and PostgreSQL
- Introduced ApplicationUser with GUID-based keys
- Updated AppDbContext to use IdentityDbContext and applied migrations
- Configured Identity and Data Protection through the Infrastructure layer
- Verified functionality via basic register/login endpoints (no JWT)

---

## Day 2 — JWT Authentication & Refresh Tokens
- Implemented short-lived JWT access tokens with issuer, audience, and signing key validation
- Added secure refresh token system with hashed storage and server-side pepper
- Implemented refresh token rotation with reuse detection and automatic revocation
- Modeled refresh token families with replacement chains to support incident-wide revocation
- Treated refresh token reuse as a security incident with full family invalidation
- Introduced clean Application-layer auth use cases (Login, Refresh) with DTOs
- Abstracted Identity behind Application interfaces (no Identity/EF leakage)
- Added AuthController with /login and /refresh endpoints
- Configured JWT authentication and authorization in WebApi pipeline
- Ensured authentication errors avoid user enumeration (uniform 401 responses)
- Applied database migration for RefreshTokens and verified behavior via Postman
- Moved all secrets (JWT key, refresh pepper, connection string) to User Secrets

---

## Day 3 — Refresh Token Rotation, Reuse Detection & Integration Tests
- Finalized refresh token rotation with atomic database transactions
- Implemented refresh token “family” concept for session-level revocation
- Added reuse detection:
  - Reusing a revoked or replaced refresh token is treated as a security incident
  - Entire token family is revoked immediately
  - Generic 401 response returned (no user or token leakage)
- Ensured all refresh tokens are stored hashed with a server-side pepper
- Added PostgreSQL-backed integration tests using Testcontainers:
  - Verified refresh rotation revokes old tokens and links replacements
  - Verified reuse attacks revoke the entire token family
  - Verified expired/invalid refresh tokens return generic 401 responses
- Ensured test isolation via database reset between security-sensitive test cases
- Proved end-to-end behavior through real HTTP calls and real PostgreSQL (no InMemory providers)

---

## Day 4 — Rate Limiting & Brute Force Protection

- Added IP-based rate limiting for authentication endpoints using ASP.NET Core built-in RateLimiter middleware
- Applied limits only to auth endpoints (no global rate limiting):
  - `/api/auth/login` → 5 requests per minute per IP
  - `/api/auth/refresh` → 20 requests per minute per IP
- Returned generic `429 Too Many Requests` responses without leaking security details
- Verified that health checks, Swagger, and other endpoints are unaffected
- Implemented per-user brute force protection via ASP.NET Identity lockout:
  - Account lockout after 5 failed login attempts
  - Lockout duration set to 15 minutes
- Ensured uniform authentication errors to prevent account enumeration
- Combined IP rate limiting and user lockout for layered (defense-in-depth) security

---

## Day 5 — Email Confirmation & Password Reset Flows

- Implemented production-ready email confirmation flow using ASP.NET Core Identity tokens
- Added non-enumerable email confirmation request endpoint
- Added email confirmation endpoint (userId + token)
- Simulated email delivery via logging abstraction (no real SMTP)
- Implemented secure password reset flow:
  - Forgot password endpoint with non-enumerable responses
  - Password reset endpoint using Identity reset tokens
- Enforced Identity password policy during reset
- Returned generic error responses to avoid leaking token or password validity
- Preserved strict Onion/Clean Architecture boundaries:
  - Infrastructure: Identity token generation, validation, and email simulation
  - Application: identity flow interfaces and DTOs
  - WebApi: endpoint wiring only
- Verified all flows end-to-end via Postman and application logs

---

## Day 6 — Authorization Policies & Least Privilege

- Defined role-based authorization policies using ASP.NET Core policy system
- Introduced clear, capability-driven policies:
  - `AdminOnly`, `OwnerOnly`, `BarberOnly`, `CustomerOnly`
  - `CanManageShops`, `CanManageAppointments`, `CanBook`
- Enforced least-privilege access by mapping roles to responsibilities
- Applied `[Authorize(Policy = "...")]` instead of raw role checks
- Kept all authorization logic confined to the WebApi layer
- Verified policy behavior end-to-end via Postman:
  - 401 for unauthenticated requests
  - 403 for insufficient roles
  - 200 for valid role-policy combinations
- Confirmed JWT role claims and Identity role assignments are correctly enforced
- Established a scalable foundation for future resource-based and claims-based authorization

---

### Scope Notes
Week 2 intentionally focuses only on identity, authentication, and authorization.
Business-domain logic such as scheduling, availability, resource ownership,
and concurrency control are addressed in subsequent weeks.

