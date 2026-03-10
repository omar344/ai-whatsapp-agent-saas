# AI WhatsApp Agent SaaS

A professional, multi-tenant SaaS platform that empowers store owners to deploy AI WhatsApp agents. The system features a **Hybrid Intelligence** engine that routes queries between structured data (SQL Path) and unstructured knowledge (RAG Path).

---

## Architecture Overview

The project follows **Clean Architecture** principles to ensure strict data isolation and scalability.

- **Frontend:** Next.js 16 (App Router, Tailwind CSS, TypeScript)
- **Backend:** .NET 9 (C#, EF Core, Web API)
- **Database:** PostgreSQL with `pgvector` (Metadata & Embeddings)
- **Cache/Session:** Redis (Chat history & Tenant resolution)
- **AI Orchestrator:** OpenAI GPT-4o (Intent Routing & RAG)

---

## 📁 Project Structure

```text
.
├── dashboard/                 # Next.js Client Ecosystem
├── src/
│   ├── AiAgent.Api            # Entry point & Middleware
│   ├── AiAgent.Application    # Business Logic & AI Prompting
│   ├── AiAgent.Infrastructure # Data Access, Encryption, Vector Search
│   └── AiAgent.Domain         # Entities (Tenant, Map, Chunk)
├── docker-compose.yml         # Infrastructure Orchestration
└── .env                       # SaaS Master Encryption Keys (Local only)