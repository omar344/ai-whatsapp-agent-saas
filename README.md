# AI WhatsApp Agent SaaS

A professional, multi-tenant SaaS platform that empowers store owners to deploy AI-powered WhatsApp agents. The system features a **Hybrid Intelligence** engine that routes customer queries between structured data (SQL Path) and unstructured knowledge (RAG Path), with responses generated in Arabic.

---

## Architecture Overview

The project follows **Clean Architecture** with strict layer dependency rules and full multi-tenant data isolation at the database level.

| Layer | Technology |
|---|---|
| Frontend | Next.js 16, React 19, Tailwind CSS v4, TypeScript |
| Backend API | .NET 9, ASP.NET Core Minimal API |
| ORM | Entity Framework Core 9 + Npgsql |
| Database | PostgreSQL 18 + pgvector extension |
| Cache / Session | Redis |
| AI Orchestrator | OpenAI GPT-4o *(Milestone 3)* |
| WhatsApp | Meta WhatsApp Cloud API v22.0 |
| Encryption | AES-256-GCM |
| Auth | JWT Bearer |

---

## Project Structure

```
ai-whatsapp-agent-saas/
├── dashboard/                          # Next.js 16 client dashboard (Milestone 5)
├── src/
│   ├── AiAgent.Api/                    # Composition root & HTTP layer
│   │   ├── Auth/
│   │   │   └── AuthEndpoints.cs        # POST /auth/token → tenant-scoped JWT
│   │   ├── Webhooks/
│   │   │   ├── WebhookEndpoints.cs     # GET + POST /webhook (WhatsApp Cloud API)
│   │   │   └── WhatsAppPayload.cs      # Inbound message DTOs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   ├── AiAgent.Application/            # Business logic, interfaces, MediatR
│   │   ├── Interfaces/
│   │   │   ├── IConversationSessionStore.cs
│   │   │   ├── ISecretEncryptionService.cs
│   │   │   ├── ITenantProvider.cs
│   │   │   └── IWhatsAppSender.cs
│   │   └── WhatsApp/
│   │       ├── ConversationTurn.cs
│   │       ├── ProcessInboundMessageCommand.cs
│   │       └── ProcessInboundMessageHandler.cs
│   │
│   ├── AiAgent.Infrastructure/         # Concrete implementations
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs         # EF Core + global tenant query filters
│   │   │   └── Migrations/
│   │   ├── Security/
│   │   │   └── AesGcmEncryptionService.cs
│   │   ├── Tenancy/
│   │   │   └── RequestTenantContext.cs # Scoped per-request tenant holder
│   │   └── WhatsApp/
│   │       ├── ConversationSessionStore.cs  # Redis-backed, 24h sliding TTL
│   │       └── WhatsAppSender.cs            # Cloud API v22.0 HTTP client
│   │
│   └── AiAgent.Domain/                 # Entities, no external dependencies
│       ├── Common/
│       │   └── IMustHaveTenant.cs
│       └── Entities/
│           └── Tenant.cs
│
├── docker-compose.yml                  # PostgreSQL + Redis
└── assets/                             # Architecture diagram
```

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/health` | None | Service status check |
| `GET` | `/webhook` | None | Meta webhook verification challenge |
| `POST` | `/webhook` | HMAC-SHA256 | Inbound WhatsApp messages |
| `POST` | `/auth/token` | None | Issue tenant-scoped JWT from API key |

Protected dashboard routes (Milestone 5) will require `Authorization: Bearer <token>`.

---

## Inbound Message Flow

```
Customer → Meta WhatsApp Cloud API → POST /webhook
    │
    ├─ HMAC-SHA256 signature validation  (X-Hub-Signature-256)
    ├─ Tenant resolution by phone_number_id
    ├─ RequestTenantContext.SetTenantId()
    ├─ MediatR → ProcessInboundMessageCommand
    │       ├─ Load conversation history from Redis
    │       ├─ Append user turn
    │       └─ [AI Orchestrator - Milestone 3]
    └─ 200 OK to Meta (within 5 seconds required)
```

---

## Running Locally

### Prerequisites

- .NET 9 SDK
- Docker

### 1. Start infrastructure

```bash
docker-compose up -d
```

This starts PostgreSQL on `5432` and Redis on `6379`.

### 2. Configure environment

The following must be set as environment variables (or in `appsettings.Development.json` for local dev):

| Variable | Description |
|---|---|
| `ENCRYPTION__MASTERKEY` | Base64-encoded 32-byte AES key. Generate: `openssl rand -base64 32` |
| `WHATSAPP__APPSECRET` | App Secret from Meta Developer Console |
| `WHATSAPP__VERIFYTOKEN` | Any string you set in the Meta webhook configuration |
| `JWT__KEY` | Signing key for dashboard JWTs (min 32 characters) |

`appsettings.Development.json` already has placeholder values for `ConnectionStrings`, `Redis`, and a dev-only `Jwt:Key` so the app starts locally without environment variables.

### 3. Apply database migrations

```bash
dotnet ef database update \
  --project src/AiAgent.Infrastructure \
  --startup-project src/AiAgent.Api
```

### 4. Run the API

```bash
dotnet run --project src/AiAgent.Api
```

---

## Multi-Tenant Isolation

All tenant-scoped entities implement `IMustHaveTenant`. `AppDbContext` applies a global EF Core query filter at model level:

```csharp
WHERE "TenantId" = '<current-tenant-id>'
```

When `ITenantProvider.TenantId` is `null`, the filter resolves to `1=0` — returning zero rows for all tenant-scoped tables. The `Tenant` entity itself is not tenant-scoped and is queryable without a filter for initial resolution.

---

## Security Notes

- **WhatsApp webhook** is authenticated via HMAC-SHA256 over the raw request body using the Meta App Secret. Comparison uses `CryptographicOperations.FixedTimeEquals` to prevent timing attacks.
- **Tenant secrets** (`StoreConnectionString`, `WhatsAppAccessToken`) are AES-256-GCM encrypted at rest. The master key is never stored in the database.
- **API keys** are stored as SHA-256 hex digests — the raw key is never persisted.
- **JWTs** carry `tenantId` and `tenantName` claims, signed with HMAC-SHA256.

---

## Milestone Roadmap

| Milestone | Status | Description |
|---|---|---|
| 1 | Done | Clean Architecture skeleton, EF Core, pgvector, Docker |
| 2 | Done | Tenant security layer, WhatsApp webhook, AES encryption, Redis session, JWT auth |
| 3 | Pending | AI Orchestrator — intent classification, RAG path (PDF ingestion + pgvector), Arabic response synthesizer |
| 4 | Pending | SQL Path — dynamic PostgreSQL connector, field mapping, GPT-4o query generation |
| 5 | Pending | Client Dashboard — PDF uploads, field config UI, analytics, tenant onboarding |
