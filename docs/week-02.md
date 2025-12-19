# 📘 Week 02 — Identity & Security Baseline

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

