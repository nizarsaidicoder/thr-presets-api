﻿# THR Preset Hub — Project Guidelines

> This file is a living document. Update it progressively as the project evolves.

---

## 🎯 Project Overview

A web platform for sharing, downloading, and rating Yamaha THR amplifier presets (`.thrl6p` files).

**Key features:**
- Upload / download `.thrl6p` preset files (stored on S3)
- Browse, search, and filter presets
- Star rating system (Wilson score for ranking)
- Favorite presets (personal library)
- Collections (organize presets into folders)
- Tags (amp model, amp version, genre, style)
- Guest-friendly (no forced login)
- Bulk download as ZIP
- Preset reports (community moderation)
- SEO-optimized frontend

---

## 🗂️ Repositories

```
ThrPresetsApi/        ← ASP.NET Core 9 backend (this repo)
thr-presets-frontend/ ← React Router v7 frontend (to be created)
```

---

## 🛠️ Stack

### Backend
| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 — Minimal API |
| ORM | Entity Framework Core 10 + Npgsql |
| Database | PostgreSQL 16 (Docker) |
| Cache | Redis 7 (Docker) |
| Auth | JWT (access token) + httpOnly cookie (refresh token) |
| File storage | AWS S3 (configured, not yet implemented) |
| Validation | DataAnnotations (implemented), FluentValidation (package added, not configured) |
| API docs | Scalar (`/docs`) |
| Password hashing | BCrypt.Net-Next |

### Frontend
| Layer | Technology |
|---|---|
| Framework | TBD (planned: React Router v7 or Next.js) |
| Language | TBD (TypeScript recommended) |
| Styling | TBD (Tailwind CSS recommended) |
| SEO | TBD (SSR/SSG required) |

---

## 🗃️ Database Schema

### Models

**User**
```
Id           string (GUID)   PK
Email        string          UNIQUE (stored lowercase)
Username     string          UNIQUE
PasswordHash string
AvatarUrl    string?
CreatedAt    DateTime
UpdatedAt    DateTime
```

**Preset**
```
Id           string (GUID)   PK
Slug         string          UNIQUE INDEX
Name         string
Description  string?
Source       string?         e.g. "Factory", "Reddit", "toneshare.io"
S3Key        string          S3 object key
FileSize     int
IsPublic     bool            INDEX
Downloads    int             INDEX
WilsonScore  double          INDEX
AuthorId     string?         FK → User (SetNull on delete) INDEX
CreatedAt    DateTime        INDEX
UpdatedAt    DateTime
```

**Tag**
```
Id     string   PK
Name   string   UNIQUE
Type   TagType  stored as string
```

**TagType enum:** `AmpModel | AmpVersion | Genre | Style`

**PresetTag** (join table)
```
PresetId   string   FK → Preset (Cascade)
TagId      string   FK → Tag (Cascade)
PK: (PresetId, TagId)
```

**Rating**
```
Id        string   PK
Stars     int      1–5
CreatedAt DateTime
UpdatedAt DateTime
UserId    string   FK → User (Cascade)
PresetId  string   FK → Preset (Cascade) INDEX
UNIQUE: (UserId, PresetId)
```

**Favorite** (join table)
```
UserId    string   FK → User (Cascade)   INDEX
PresetId  string   FK → Preset (Cascade)
CreatedAt DateTime
PK: (UserId, PresetId)
```

**Collection**
```
Id          string   PK
Name        string
Description string?
IsPublic    bool
CreatedAt   DateTime
UpdatedAt   DateTime
UserId      string   FK → User (Cascade) INDEX
```

**CollectionItem**
```
Position     int
Note         string?
AddedAt      DateTime
CollectionId string   FK → Collection (Cascade)
PresetId     string   FK → Preset (Cascade)
PK: (CollectionId, PresetId)
```

**PresetReport**
```
Id         string        PK
Reason     ReportReason  stored as string
Details    string?
Resolved   bool          INDEX
CreatedAt  DateTime
ReporterId string?       FK → User (SetNull on delete)
PresetId   string        FK → Preset (Cascade) INDEX
```

**ReportReason enum:** `BrokenFile | IncorrectInfo | Duplicate | Inappropriate | Other`

---

## 🔗 REST API Endpoints

### Auth
```
POST   /api/auth/signup    — register (guest or new user)
POST   /api/auth/signin    — login
POST   /api/auth/refresh   — refresh access token (uses httpOnly cookie)
POST   /api/auth/logout    — clear refresh token cookie
```

### Users
```
GET    /api/users/me                — my profile (authenticated)
PATCH  /api/users/me                — update my profile
DELETE /api/users/me                — delete my account
GET    /api/users/:username         — public profile + their presets
```

### Presets
```
GET    /api/presets                 — browse public presets (search, filter, sort)
GET    /api/presets/:slug           — preset detail
POST   /api/presets                 — upload preset (guest or authenticated)
PATCH  /api/presets/:slug           — update my preset
DELETE /api/presets/:slug           — delete my preset
GET    /api/presets/:slug/download  — download .thrl6p (S3 signed URL redirect)
POST   /api/presets/bulk-download   — download multiple as ZIP
POST   /api/presets/:slug/report    — report a preset
```

### Ratings
```
POST   /api/presets/:slug/ratings     — rate a preset 1–5 (upsert)
GET    /api/presets/:slug/ratings/me  — get my rating
DELETE /api/presets/:slug/ratings/me  — remove my rating
```

### Favorites
```
GET    /api/favorites          — my favorite presets
POST   /api/favorites/:slug    — add to favorites
DELETE /api/favorites/:slug    — remove from favorites
```

### Collections
```
GET    /api/collections                        — my collections
POST   /api/collections                        — create collection
GET    /api/collections/:id                    — collection detail
PATCH  /api/collections/:id                    — update collection
DELETE /api/collections/:id                    — delete collection
POST   /api/collections/:id/presets/:slug      — add preset
PATCH  /api/collections/:id/presets/:slug      — update note/position
DELETE /api/collections/:id/presets/:slug      — remove preset
```

### Tags
```
GET    /api/tags    — list all tags (filterable by type)
```

---

## 📁 Project Structure

```
ThrPresetsApi/
├── Features/                    — Vertical slice architecture
│   ├── Auth/
│   │   ├── DTOs/
│   │   │   ├── SignUpDto.cs
│   │   │   ├── SignInDto.cs
│   │   │   ├── AuthResponseDto.cs
│   │   │   └── RefreshResponseDto.cs
│   │   ├── AuthService.cs       — Business logic
│   │   ├── TokenService.cs      — JWT handling
│   │   └── AuthEndpoints.cs     — Minimal API endpoints
│   ├── Users/
│   │   └── DTOs/
│   │       └── UserDto.cs
│   ├── Presets/                 (planned)
│   ├── Ratings/                 (planned)
│   ├── Favorites/               (planned)
│   ├── Collections/             (planned)
│   └── Tags/                    (planned)
├── Common/                      — Shared utilities
│   ├── Exceptions/
│   │   ├── ValidationException.cs
│   │   ├── ConflictException.cs
│   │   └── NotFoundException.cs
│   └── Extensions/
│       └── EndpointExtensions.cs — .IsPublic(), .RequireAuth()
├── Configuration/               — Startup configuration
│   ├── AuthConfiguration.cs
│   ├── CorsConfiguration.cs
│   ├── DatabaseConfiguration.cs
│   ├── JsonConfiguration.cs
│   ├── ServicesConfiguration.cs
│   └── SwaggerConfiguration.cs
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Models/                      — EF Core entities (shared)
│   ├── User.cs
│   ├── Preset.cs
│   ├── Rating.cs
│   ├── Favorite.cs
│   ├── Collection.cs
│   ├── CollectionItem.cs
│   ├── Tag.cs
│   ├── PresetTag.cs
│   └── PresetReport.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json (gitignored)
├── docker-compose.yml
├── nuget.config
├── Program.cs
├── requests.http                (planned)
└── ThrPresetsApi.csproj
```

---

## 🔐 Authentication

- **Access token** — JWT, 15 min expiry, returned in response body
- **Refresh token** — JWT, 7 day expiry, stored in `httpOnly` cookie (`SameSite=Lax`, `Secure=false` in dev)
- **Guest support** — most read endpoints accessible without auth via `OptionalJwtGuard` pattern
- Claim type mapping is disabled (`JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear()`) so `sub` stays `sub`
- Email stored lowercased on signup and compared lowercased on signin

### JWT Config (`appsettings.json`)
```json
"Jwt": {
  "Secret": "...",
  "AccessTokenExpiresInMinutes": 15,
  "RefreshTokenExpiresInDays": 7,
  "Issuer": "ThrPresetsApi",
  "Audience": "ThrPresetsApi"
}
```

---

## ⚙️ Key Conventions

- **Architecture**: Vertical slice pattern — each feature is self-contained in `Features/[Feature]/`
- **Endpoints** use `TypedResults` for fully typed, Swagger-friendly responses
- **All endpoints** should have `.WithName()`, `.WithSummary()`, `.WithDescription()`, `.Produces<T>()` decorators
- **Validation** using DataAnnotations on DTOs (FluentValidation package added but not configured)
- **Error handling** via `ExceptionHandlingMiddleware` mapping exceptions to HTTP status codes
- **Enums** stored as strings in the DB (configured in AppDbContext)
- **JSON** serialized as camelCase via `ConfigureHttpJsonOptions`
- **CORS** configured with `AllowCredentials()` for cookie support
- **Slugs** will be used in URLs instead of IDs for SEO (`/presets/early-vh`)
- **WilsonScore** will be recalculated on every new rating, stored on Preset for fast sorting
- **Preset settings** will be parsed on the fly from S3, cached in Redis (not yet implemented)
- **IDs** are GUID strings generated via `Guid.NewGuid().ToString()`
- **Primary constructors** used throughout (e.g., `public class MyService(AppDbContext db)`)

---

## 🐳 Docker

`docker-compose.yml` at project root:
```yaml
services:
  postgres:  port 5432, image postgres:16-alpine
  redis:     port 6379, image redis:7-alpine
```

```powershell
docker compose up -d          # start
docker compose down -v        # full reset (drops volumes)
dotnet ef database update     # apply migrations
```

---

## 🧪 Testing

Using JetBrains HTTP client (`.http` files) with inline assertions.

**Auth tests** (`requests.http`) — 21 tests covering:
- Signup: valid, duplicate email, duplicate username, invalid email, short/long password, short/long username, invalid username chars, empty body
- Signin: valid, wrong password, unknown email, empty email, empty password, empty body
- Refresh: valid cookie, tampered token
- Logout: valid, already logged out
- Refresh after logout

---

## 📝 TODO

### ✅ Done
- [x] PostgreSQL + Redis via Docker
- [x] EF Core DbContext + all models
- [x] Initial migration
- [x] JWT auth (access token + httpOnly refresh token cookie)
- [x] Signup / Signin / Refresh / Logout endpoints (AuthEndpoints.cs)
- [x] Auth services (AuthService, TokenService)
- [x] DataAnnotations validation on DTOs
- [x] Global exception handler (ExceptionHandlingMiddleware)
- [x] Custom exceptions (ValidationException, ConflictException, NotFoundException)
- [x] camelCase JSON serialization
- [x] CORS with `AllowCredentials()`
- [x] Scalar API docs (`/docs` endpoint)
- [x] Configuration extensions (Auth, CORS, Database, JSON, Swagger, Services)
- [x] Endpoint extensions (.IsPublic(), .RequireAuth())
- [x] Vertical slices architecture setup

### 🚨 Critical Fixes Needed
- [ ] **Implement AddValidation() extension method** or remove the call from Program.cs
- [ ] **Map AuthEndpoints in Program.cs** — add `app.MapAuthEndpoints();` before `app.RunAsync()`
- [ ] **Configure FluentValidation** or remove the package if using DataAnnotations only
- [ ] **Create appsettings.Development.json** with proper configuration (JWT secret, DB connection, etc.)

### 🔜 Up Next
- [ ] Auth `.http` test suite (21 tests as mentioned in docs)
- [ ] Users module (service + endpoints + DTOs)
- [ ] Presets module (service + endpoints + DTOs + S3 integration)
- [ ] Tags module (service + endpoints + seed data)
- [ ] Ratings module (service + endpoints + Wilson score)
- [ ] Favorites module (service + endpoints)
- [ ] Collections module (service + endpoints)
- [ ] S3Service implementation
- [ ] WilsonScoreService implementation
- [ ] Redis caching service for preset settings
- [ ] Frontend (React Router v7 or similar)

### 🎯 Future Enhancements
- [ ] Email verification
- [ ] Password reset flow
- [ ] Admin panel for moderating reports
- [ ] Preset analytics (view counts, download stats)
- [ ] User profiles with avatar upload
- [ ] Social features (follow users, comments)
- [ ] API rate limiting
- [ ] Comprehensive logging and monitoring
- [ ] CI/CD pipeline
- [ ] Production deployment guide
