# 🌍 IP Blocker API

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/Architecture-Clean-green?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Storage-In--Memory-orange?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Swagger-Enabled-85EA2D?style=for-the-badge&logo=swagger&logoColor=black" />
  <img src="https://img.shields.io/badge/Build-Passing-brightgreen?style=for-the-badge" />
</p>

> A **production-quality .NET 8 Web API** for managing blocked countries and validating IP addresses using the [ipapi.co](https://ipapi.co) geolocation service.  
> No database required — all storage is **thread-safe in-memory** using `ConcurrentDictionary` and `ConcurrentBag`.

---

## 📑 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
  - [Countries](#countries)
  - [IP](#ip)
  - [Logs](#logs)
- [Error Handling](#-error-handling)
- [Background Service](#-background-service)
- [Input Validation](#-input-validation)
- [Tech Stack](#-tech-stack)

---

## ✨ Features

| Feature | Description |
|---|---|
| 🚫 **Country Blocking** | Permanently block/unblock countries by ISO 3166-1 alpha-2 code |
| ⏱️ **Temporary Blocks** | Time-limited blocks (1–1440 min) that auto-expire |
| 🔍 **IP Geolocation** | Resolve any IP to country, ISP via ipapi.co |
| 🛡️ **Block Check** | Instantly check if a caller's IP is from a blocked country |
| 📋 **Audit Logging** | Every block-check attempt is logged with IP, country, timestamp, user-agent |
| 📄 **Pagination** | All list endpoints support `page`, `pageSize`, and `search` |
| 🔄 **Auto Cleanup** | Background service purges expired blocks every 5 minutes |
| 🌐 **Swagger UI** | Full interactive API documentation at `/` |
| ⚙️ **Global Error Handling** | Unified JSON error responses with proper HTTP status codes |
| 🔒 **Thread Safety** | All in-memory stores use concurrent collections |

---

## 🏗️ Architecture

This project follows **Clean Architecture** with strict separation of concerns across 4 projects:

```
IPBlocker.sln
├── IPBlocke.Api            ← Presentation   (Controllers, Middleware, DI, Swagger)
├── IPBlocker.Application   ← Business Logic (Services, Interfaces, DTOs, Exceptions)
├── IPBlocker.Infrastructure← Infrastructure (Repositories, ipapi.co Client, Hosted Service)
└── IPBlocker.Domain        ← Core Domain    (Entities only, no dependencies)
```

### Dependency Flow

```
IPBlocke.Api
    │
    ├──► IPBlocker.Application ◄── IPBlocker.Infrastructure
    │           │
    └───────────┴──► IPBlocker.Domain
```

- **Domain** has zero dependencies — pure entities.
- **Application** depends only on Domain — defines interfaces, not implementations.
- **Infrastructure** implements Application interfaces — repositories, external APIs, hosted services.
- **API** wires everything together — controllers call services, DI registered in `ServiceRegistration.cs`.

### In-Memory Storage Design

| Store | Type | Notes |
|---|---|---|
| Blocked countries | `ConcurrentDictionary<string, Country>` | Case-insensitive key (country code) |
| Temporary blocks | `ConcurrentDictionary<string, TemporaryBlock>` | Lazy eviction on read + scheduled cleanup |
| Audit logs | `ConcurrentBag<BlockedAttemptLog>` | Append-only, newest-first on retrieval |

> **Repositories are registered as Singletons** so they hold state for the lifetime of the app.  
> **Services are Scoped** — resolved per HTTP request.

---

## 🗂️ Project Structure

```
IPBlocker/
│
├── 📄 README.md
├── 📄 .gitignore
├── 📄 IPBlocker.sln
│
├── 📁 IPBlocker.Domain/
│   └── Entities/
│       ├── Country.cs                         ← string CountryCode, CountryName, DateTime CreatedAt
│       ├── TemporaryBlock.cs                  ← + ExpiresAt, DurationMinutes, IsExpired (computed)
│       └── BlockedAttemptLog.cs               ← IpAddress, Timestamp, CountryCode, IsBlocked, UserAgent
│
├── 📁 IPBlocker.Application/
│   ├── DTOs/
│   │   └── Dtos.cs                            ← All request/response DTOs + PaginatedResult<T>
│   ├── Exceptions/
│   │   └── CustomExceptions.cs                ← DuplicateException, NotFoundException, etc.
│   ├── Interfaces/
│   │   ├── ICountryRepository.cs
│   │   ├── ITemporaryBlockRepository.cs
│   │   ├── ILogRepository.cs
│   │   ├── IIpGeolocationService.cs
│   │   ├── ICountryService.cs
│   │   ├── IIpService.cs
│   │   ├── ITemporaryBlockService.cs
│   │   └── ILogService.cs
│   └── Services/
│       ├── CountryService.cs
│       ├── IpService.cs
│       ├── TemporaryBlockService.cs
│       └── LogService.cs
│
├── 📁 IPBlocker.Infrastructure/
│   ├── Repositories/
│   │   ├── InMemoryCountryRepository.cs
│   │   ├── InMemoryTemporaryBlockRepository.cs
│   │   └── InMemoryLogRepository.cs
│   ├── External/
│   │   └── IpApiGeolocationService.cs         ← ipapi.co HTTP client with retry/rate-limit handling
│   └── BackgroundServices/
│       └── TemporaryBlockCleanupService.cs     ← Runs every 5 minutes
│
└── 📁 IPBlocke.Api/
    ├── Controllers/
    │   ├── CountriesController.cs             ← /api/countries/*
    │   ├── IpController.cs                    ← /api/ip/*
    │   └── LogsController.cs                  ← /api/logs/*
    ├── Extensions/
    │   └── ServiceRegistration.cs             ← AddApplicationServices(), AddInfrastructureServices()
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs      ← Global exception → JSON error response
    ├── Program.cs
    └── appsettings.json
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Clone & Run

```bash
git clone https://github.com/your-username/IPBlocker.git
cd IPBlocker
dotnet restore
dotnet run --project IPBlocke.Api --urls "http://localhost:5100"
```

Open **Swagger UI** at 👉 [http://localhost:5100](http://localhost:5100)

### Configure API Key (Optional)

`ipapi.co` works **for free** with up to 1,000 requests/day without an API key.  
To use a paid key, edit `IPBlocke.Api/appsettings.json`:

```json
{
  "IpApi": {
    "ApiKey": "YOUR_IPAPI_CO_KEY_HERE"
  }
}
```

---

## 📋 API Endpoints

### Countries

---

#### `POST /api/countries/block`
Add a country to the **permanent** blocked list.

**Request Body:**
```json
{
  "countryCode": "EG",
  "countryName": "Egypt"
}
```

| Field | Type | Rules |
|---|---|---|
| `countryCode` | `string` | Required. Exactly 2 uppercase letters (ISO 3166-1 alpha-2) |
| `countryName` | `string` | Required. 2–100 characters |

**Responses:**

`201 Created`
```json
{
  "countryCode": "EG",
  "countryName": "Egypt",
  "createdAt": "2026-04-09T13:25:06.260Z"
}
```

`409 Conflict` — country already blocked
```json
{
  "statusCode": 409,
  "message": "Country 'EG' is already blocked.",
  "timestamp": "2026-04-09T13:25:10.000Z",
  "details": null
}
```

---

#### `DELETE /api/countries/block/{countryCode}`
Remove a country from the permanent block list.

| Parameter | Type | Example |
|---|---|---|
| `countryCode` | route `string` | `EG` |

**Responses:**

`204 No Content` — successfully unblocked  
`404 Not Found` — country was not in the blocked list

---

#### `GET /api/countries/blocked`
Get a **paginated, searchable** list of all permanently blocked countries.

| Query Param | Type | Default | Description |
|---|---|---|---|
| `page` | `int` | `1` | Page number (1-based) |
| `pageSize` | `int` | `10` | Items per page (max 100) |
| `search` | `string` | — | Filter by country code or name (case-insensitive) |

**Example:** `GET /api/countries/blocked?page=1&pageSize=5&search=eg`

`200 OK`
```json
{
  "items": [
    {
      "countryCode": "EG",
      "countryName": "Egypt",
      "createdAt": "2026-04-09T13:25:06.260Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 5,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

---

#### `POST /api/countries/temporal-block`
Temporarily block a country for a set duration. **Expires automatically.**

**Request Body:**
```json
{
  "countryCode": "RU",
  "durationMinutes": 60
}
```

| Field | Type | Rules |
|---|---|---|
| `countryCode` | `string` | Required. 2 uppercase letters |
| `durationMinutes` | `int` | Required. Between **1** and **1440** (24 hours) |

**Responses:**

`201 Created`
```json
{
  "countryCode": "RU",
  "createdAt": "2026-04-09T13:25:49.359Z",
  "expiresAt": "2026-04-09T14:25:49.359Z",
  "durationMinutes": 60
}
```

`409 Conflict` — an active temporary block already exists for this country

---

### IP

---

#### `GET /api/ip/lookup`
Resolve an IP address to its country and ISP using ipapi.co.  
If `ipAddress` is omitted, **the caller's own IP** is used.

| Query Param | Type | Required | Description |
|---|---|---|---|
| `ipAddress` | `string` | No | IPv4 or IPv6 address to look up |

**Example:** `GET /api/ip/lookup?ipAddress=8.8.8.8`

`200 OK`
```json
{
  "ipAddress": "8.8.8.8",
  "countryCode": "US",
  "countryName": "United States",
  "isp": "Google LLC"
}
```

`400 Bad Request` — invalid IP address format  
`502 Bad Gateway` — geolocation API unavailable or rate-limited

---

#### `GET /api/ip/check-block`
Check if the **caller's IP** is from a blocked country (permanently or temporarily).  
Every call is automatically **logged** to the audit log.

> Caller IP is resolved from `X-Forwarded-For` header first (proxy-aware), then from `RemoteIpAddress`.

`200 OK`
```json
{
  "ipAddress": "8.8.8.8",
  "countryCode": "US",
  "countryName": "United States",
  "isBlocked": true
}
```

---

### Logs

---

#### `GET /api/logs/blocked-attempts`
Get a **paginated** audit log of all `/api/ip/check-block` calls.

| Query Param | Type | Default | Description |
|---|---|---|---|
| `page` | `int` | `1` | Page number |
| `pageSize` | `int` | `10` | Items per page (max 100) |

Results are sorted **newest first**.

`200 OK`
```json
{
  "items": [
    {
      "ipAddress": "8.8.8.8",
      "timestamp": "2026-04-09T13:26:55.924Z",
      "countryCode": "US",
      "countryName": "United States",
      "isBlocked": true,
      "userAgent": "Mozilla/5.0 ..."
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

---

## ❌ Error Handling

All errors are returned as a uniform JSON structure via the global `ExceptionHandlingMiddleware`:

```json
{
  "statusCode": 404,
  "message": "Country 'XX' is not in the blocked list.",
  "timestamp": "2026-04-09T13:00:00.000Z",
  "details": null
}
```

> In **Development** environment, `details` includes the full exception stack trace.

### Exception → HTTP Status Code Mapping

| Exception | Status Code | When |
|---|---|---|
| `DuplicateException` | `409 Conflict` | Blocking an already-blocked country |
| `NotFoundException` | `404 Not Found` | Unblocking a non-existent country |
| `ValidationException` | `400 Bad Request` | Invalid IP format |
| `ExternalApiException` | `502 Bad Gateway` | ipapi.co unreachable or rate-limited |
| Model validation failure | `400 Bad Request` | Invalid request body (DataAnnotations) |
| Unhandled exception | `500 Internal Server Error` | Unexpected errors |

---

## ⏱️ Background Service

`TemporaryBlockCleanupService` is a `BackgroundService` (`IHostedService`) that:

- Starts automatically when the application starts
- Runs a cleanup loop **every 5 minutes**
- Calls `ITemporaryBlockRepository.RemoveExpiredAsync()` to purge expired blocks
- Logs how many records were removed each cycle
- Handles graceful shutdown via `CancellationToken`

Additionally, `InMemoryTemporaryBlockRepository` performs **lazy eviction** — when you query whether a country is blocked, expired entries are removed on-the-spot, giving instant accuracy without waiting for the scheduled job.

---

## ✅ Input Validation

All request DTOs use **DataAnnotations** for validation, enforced automatically by ASP.NET Core model binding:

| Rule | Applied To |
|---|---|
| `[Required]` | `countryCode`, `countryName`, `durationMinutes` |
| `[StringLength(2, MinimumLength = 2)]` | `countryCode` |
| `[RegularExpression("^[A-Z]{2}$")]` | `countryCode` — must be 2 uppercase letters |
| `[StringLength(100, MinimumLength = 2)]` | `countryName` |
| `[Range(1, 1440)]` | `durationMinutes` |

Invalid IP addresses are also validated programmatically using `IPAddress.TryParse()`.

---

## 🛠️ Tech Stack

| Component | Technology | Version |
|---|---|---|
| Framework | ASP.NET Core Web API | .NET 8 |
| API Documentation | Swashbuckle / Swagger UI | 6.6.2 |
| HTTP Client | `IHttpClientFactory` (typed client) | 8.0.1 |
| Geolocation Provider | [ipapi.co](https://ipapi.co) REST API | — |
| Storage | `ConcurrentDictionary`, `ConcurrentBag` | BCL |
| Background Jobs | `BackgroundService` / `IHostedService` | .NET 8 |
| Logging | `Microsoft.Extensions.Logging` | .NET 8 |
| JSON | `System.Text.Json` | .NET 8 |
| Dependency Injection | `Microsoft.Extensions.DependencyInjection` | .NET 8 |

---

## 📜 License

This project is intended for educational and assignment purposes.
