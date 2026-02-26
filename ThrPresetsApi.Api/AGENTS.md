# THR Preset Hub — Agent Instructions

> This file is intended for AI assistants working on this project.
> Read it fully before writing any code.

---

## 🚀 How to Launch the Project

### Prerequisites
- .NET 10 SDK
- Docker Desktop
- JetBrains Rider or Visual Studio (recommended for development)
- DataGrip (optional, for DB visualization)

### First time setup
```powershell
# Start PostgreSQL + Redis
docker compose up -d

# Apply migrations
dotnet ef database update

# Run the API
dotnet run
```

### Daily workflow
```powershell
docker compose up -d      # make sure DB + Redis are running
dotnet run                # start the API
# API available at http://localhost:5149 (or check launchSettings.json)
# Scalar docs at http://localhost:5149/docs
```

### After pulling changes
```powershell
dotnet ef database update  # apply any new migrations
dotnet run
```

### Database reset
```powershell
docker compose down -v     # wipe volumes
docker compose up -d
dotnet ef database update
```

### Adding a new migration
```powershell
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## 🛠️ Stack

### Backend
| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 — Minimal API |
| ORM | Entity Framework Core 10 + Npgsql |
| Database | PostgreSQL 16 (Docker) |
| Cache | Redis 7 (Docker) |
| Auth | JWT (access token) + httpOnly cookie (refresh token) |
| File storage | AWS S3 (planned) |
| Validation | DataAnnotations (simple), FluentValidation (planned for complex) |
| API docs | Scalar (`/docs`) |
| Password hashing | BCrypt.Net-Next |

---

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
│   │   ├── DTOs/
│   │   │   └── UserDto.cs
│   │   ├── UsersService.cs      (planned)
│   │   └── UsersEndpoints.cs    (planned)
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
│   ├── Extensions/
│   │   └── EndpointExtensions.cs — .IsPublic(), .RequireAuth()
│   └── Models/                  (for shared base classes if needed)
├── Configuration/               — Startup configuration
│   ├── AuthConfiguration.cs
│   ├── CorsConfiguration.cs
│   ├── DatabaseConfiguration.cs
│   ├── JsonConfiguration.cs
│   ├── ServicesConfiguration.cs
│   └── SwaggerConfiguration.cs
├── Data/
│   ├── AppDbContext.cs          — EF Core DbContext
│   └── Migrations/              — Auto-generated (do not edit manually)
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs — Global exception handler
├── Models/                      — EF Core entities (shared across features)
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
├── appsettings.Development.json
├── Program.cs                   — App entry point (stays lean)
└── requests.http                — HTTP client test suite
```

### Architecture Pattern: Vertical Slices

Each feature is self-contained with:
- **DTOs/** - Request/response objects
- **Validators/** - FluentValidation (if needed)
- **[Feature]Service.cs** - Business logic
- **[Feature]Endpoints.cs** - API endpoints

**Benefits:**
- High cohesion: related code lives together
- Easy to locate feature code
- Scales well as features are added
- Clear ownership boundaries

**Current Implementation:**
- ✅ Auth feature fully implemented (AuthService, TokenService, AuthEndpoints, DTOs)
- ⚠️ Users feature partially implemented (only DTOs exist)
- 📋 Other features planned but not started (Presets, Ratings, Favorites, Collections, Tags)

---

## ✍️ Code Conventions

### General
- Language: **C# 13**, target framework **net10.0**
- Use **primary constructors** wherever possible (e.g. `public class MyService(AppDbContext db)`)
- Use **file-scoped namespaces** (`namespace ThrPresetsApi.Api.Features.Auth;`)
- Prefer `async/await` for all DB and I/O operations
- Never use `var` when the type is not immediately obvious from the right-hand side
- Use `null!` for required navigation properties in models
- Use `[]` for empty collection initializers (C# 12+)

### Naming
- Classes, methods, properties: **PascalCase**
- Private fields: **_camelCase**
- DTOs suffix: `Dto` (e.g. `CreatePresetDto`, `PresetResponseDto`)
- Services suffix: `Service` (e.g. `PresetsService`)
- Endpoints suffix: `Endpoints` (e.g. `PresetsEndpoints`)
- Exceptions suffix: `Exception` (e.g. `NotFoundException`)

### Namespaces
Follow the folder structure:
- Features: `ThrPresetsApi.Features.[Feature]`
- Feature DTOs: `ThrPresetsApi.Features.[Feature].DTOs`
- Feature Validators: `ThrPresetsApi.Features.[Feature].Validators`
- Common: `ThrPresetsApi.Common.[Subfolder]`
- Configuration: `ThrPresetsApi.Configuration`
- Models: `ThrPresetsApi.Models`

### Models
- All models live in `Models/` (shared across features)
- All PKs are `string` using `Guid.NewGuid().ToString()`
- Always include `CreatedAt` and `UpdatedAt` where relevant
- Navigation properties use `null!` for required and `?` for optional
- Empty collections initialized with `[]`
- Enums stored as strings in the DB (configured in `AppDbContext`)

### DTOs
- Organized by feature under `Features/[Feature]/DTOs/`
- One DTO per file
- Use `System.ComponentModel.DataAnnotations` for simple validation
- Use `FluentValidation` for complex validation rules
- Never expose `PasswordHash` or sensitive fields in response DTOs
- Response DTOs should be flat and minimal — only what the client needs

### Services
- All services live in `Features/[Feature]/`
- Services only receive DTOs and primitives as parameters — never raw `HttpRequest`/`HttpResponse`
- Services throw custom exceptions (from `Common/Exceptions/`) — never return nulls or booleans for error states
- All DB operations use `AppDbContext` injected via primary constructor
- Always use `async/await` — no `.Result` or `.Wait()`
- Register services in `Configuration/ServicesConfiguration.cs`

### Endpoints
- All endpoints live in `Features/[Feature]/[Feature]Endpoints.cs` as static classes
- Each endpoint file maps one feature (e.g. `AuthEndpoints.cs`, `PresetsEndpoints.cs`)
- Extension method signature: `public static void Map[Feature]Endpoints(this WebApplication app)`
- Always use `TypedResults` (not `Results`) for type-safe responses
- Every endpoint must have:
    - `.WithName("EndpointName")`
    - `.WithSummary("...")`
    - `.WithDescription("...")`
    - `.Produces<T>(statusCode)` for every possible response
    - `.WithTags("Domain")`
- Use `.IsPublic()` for endpoints accessible without auth (from `Common/Extensions/EndpointExtensions.cs`)
- Use `.RequireAuth()` for endpoints requiring authentication
- Endpoint handlers should be thin — delegate all logic to services

### Exceptions
- All custom exceptions live in `Common/Exceptions/`
- The `ExceptionHandlingMiddleware` maps exception types to HTTP status codes
- Use the appropriate exception type:
    - `ValidationException` → 400
    - `ConflictException` → 409
    - `NotFoundException` → 404
    - `UnauthorizedAccessException` → 401 (built-in)
    - `BadHttpRequestException` → 400 (built-in, fallback for framework errors)

### Database
- All DB configuration lives in `Data/AppDbContext.OnModelCreating()`
- Always define explicit indexes for columns used in filtering/sorting
- Use `DeleteBehavior.Cascade` for owned entities
- Use `DeleteBehavior.SetNull` for optional relationships (e.g. author of a preset)
- Composite PKs defined with `e.HasKey(x => new { x.A, x.B })`
- Enums stored as strings: `e.Property(x => x.Enum).HasConversion<string>()`

### JSON
- All responses serialized as **camelCase**
- Configured globally via `Configuration/JsonConfiguration.cs`
- Do not use `[JsonPropertyName]` attributes — rely on the global policy

### Auth
- Access token: JWT, 15 min, returned in response body
- Refresh token: JWT, 7 days, stored in `HttpOnly` cookie (`SameSite=Lax`)
- `JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear()` is called so claim names stay as standard JWT names (`sub`, `jti`, etc.)
- Extract user ID from claims using `"sub"` after clearing the map
- Never store sensitive data in JWT claims

### Configuration
- All startup configuration lives in `Configuration/` folder
- Each configuration class is an extension method on `IServiceCollection` or `WebApplication`
- `Program.cs` should only call these extension methods and map endpoints
- Keep `Program.cs` lean and readable
- **IMPORTANT**: Endpoint mapping is missing from `Program.cs` - each feature's endpoints must be mapped (e.g., `app.MapAuthEndpoints();`)

### Middleware
- Custom middleware lives in `Middleware/`
- Use primary constructors: `public class MyMiddleware(RequestDelegate next)`
- Register in `Program.cs` with `app.UseMiddleware<MyMiddleware>()`

---

## 🧪 Testing

- Tests written in JetBrains HTTP client format (`.http` files)
- All tests live in `requests.http` at the project root
- Use `client.test()` + `client.assert()` for assertions
- Use `client.global.set()` to pass data between requests (e.g. access token)
- Run all tests with the **Run All** button in WebStorm/Rider
- Tests must cover: happy path, validation errors, auth errors, edge cases
- Clean the DB before running the full suite (`TRUNCATE "Users" CASCADE` in DataGrip)

---

## 🔑 Environment Variables

All secrets live in `appsettings.Development.json` (never committed):

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=thr_hub;Username=thr;Password=thr_password"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters",
    "AccessTokenExpiresInMinutes": 15,
    "RefreshTokenExpiresInDays": 7,
    "Issuer": "ThrPresetsApi",
    "Audience": "ThrPresetsApi"
  },
  "Cors": {
    "Origins": ["http://localhost:5149", "http://localhost:3000"]
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Aws": {
    "Region": "eu-west-3",
    "AccessKeyId": "...",
    "SecretAccessKey": "...",
    "BucketName": "thr-presets"
  }
}
```

---

## ⚠️ Things to Avoid

- Do NOT edit files in `Migrations/` manually
- Do NOT return `PasswordHash` or any sensitive field in response DTOs
- Do NOT use `.Result` or `.Wait()` on async methods
- Do NOT put business logic in endpoint handlers — delegate to services
- Do NOT use `Results` — always use `TypedResults`
- Do NOT add packages without checking for version compatibility with .NET 10
- Do NOT commit `appsettings.Development.json` (it contains secrets)
- Do NOT use `var` when the type is unclear
- Do NOT use `BadHttpRequestException` in business logic — use custom exceptions from `Common/Exceptions/`

---

## 🚨 Current Issues to Fix

1. **Missing AddValidation() extension method** — Program.cs calls `builder.Services.AddValidation()` but this method doesn't exist yet. Either implement it or use FluentValidation's built-in `AddValidatorsFromAssemblyContaining<T>()`.

2. **Missing endpoint mapping** — Program.cs doesn't call `app.MapAuthEndpoints()`, so the Auth endpoints are not exposed. Add this before `app.RunAsync()`.

3. **FluentValidation not configured** — FluentValidation.AspNetCore is referenced but not configured. Either use it or rely on DataAnnotations only.

---

## 🎯 When Adding a New Feature

1. Create `Features/[Feature]/` folder
2. Add DTOs in `Features/[Feature]/DTOs/`
3. Add service: `Features/[Feature]/[Feature]Service.cs`
4. Add endpoints: `Features/[Feature]/[Feature]Endpoints.cs`
5. Register service in `Configuration/ServicesConfiguration.cs`
6. Map endpoints in `Program.cs`: `app.Map[Feature]Endpoints();`
7. Add tests to `requests.http`

**Example:** Adding a Users feature
```
Features/Users/
├── DTOs/
│   ├── UserDto.cs
│   ├── UpdateUserDto.cs
│   └── UserProfileDto.cs
├── UsersService.cs
└── UsersEndpoints.cs
```

Then in `Configuration/ServicesConfiguration.cs`:
```csharp
services.AddScoped<UsersService>();
```

And in `Program.cs`:
```csharp
app.MapUsersEndpoints();
```
