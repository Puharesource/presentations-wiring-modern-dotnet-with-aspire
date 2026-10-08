# Modernize with Aspire

A deliberately small users application: frontend → .NET API → SQL Server, with Redis caching `GET /users` for one minute. Creating a user invalidates the cached response. `X-Database-Read` changes only on an actual database read, making cache hits visible.

## Talk branches

- `before`: Docker Compose and manually supplied configuration.
- `after`: Aspire 13.6.1 AppHost, service defaults, and isolated TUnit integration/end-to-end tests.
- `live`: starts at `before`; use it for the live migration.

The original folders are preserved outside this repository at `../ModernizeWithAspire-originals-20261007`. Both originals already contained Aspire and Angular; `before` is an explicit Compose baseline derived from their users feature.

## Run the Compose baseline

```sh
cp .env.example .env
docker compose up --build --wait
```

Open http://localhost:5173. The API is http://localhost:5244; OpenAPI is `/openapi/v1.json`.

```sh
dotnet build
pwsh Tests/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet run --project Tests
```

Tests on this branch expect the app to be running. The migration-based SQL volume keeps development data. Older EnsureCreated volumes are preserved separately. `docker compose down` stops the environment without deleting it.

## Navigate the demo

- `Backend/Program.cs`: configuration, endpoints, cache policy, invalidation.
- `Backend/DbContexts` and `Backend/Entities`: the small SQL model.
- `frontend/src`: the single-page user workflow.
- `Tests`: API/cache test, browser journey, and environment fixture.
- `compose.yaml` on `before`; `AppHost/AppHost.cs` on `after`.

This is a local teaching app: no authentication or production deployment, and a single EF migration applied by the API on startup. The SQL and Redis client registrations are identical on both branches: Compose supplies their connection strings before migration, and AppHost supplies them afterwards. SQL Server requires an x64-capable container runtime; Apple Silicon uses Docker's amd64 emulation. Download packages, browser binaries, and images before presenting. Once those are cached, running and testing the app needs no cloud service.
