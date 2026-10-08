# Modernize with Aspire

A deliberately small users application: frontend → .NET API → SQL Server, with Redis caching `GET /users` for one minute. Creating a user invalidates the cached response. `X-Database-Read` changes only on an actual database read, making cache hits visible.

## Talk branches

- `before`: Docker Compose and manually supplied configuration.
- `after`: Aspire 13.6.1 AppHost, service defaults, and isolated TUnit integration/end-to-end tests.
- `live`: starts at `before`; available for private coding practice. The talk uses prepared code slides and switches from `before` to `after`.

The original folders are preserved outside this repository at `../ModernizeWithAspire-originals-20261007`. Both originals already contained Aspire and Angular; `before` is an explicit Compose baseline derived from their users feature.

## Run with Aspire

```sh
aspire start --apphost AppHost/AppHost.csproj --non-interactive
aspire wait frontend --apphost AppHost/AppHost.csproj --non-interactive
aspire describe --apphost AppHost/AppHost.csproj --non-interactive
```

The API resource has an **OpenAPI** dashboard link to `/openapi/v1.json`, configured with `WithUrl`. This adds a discoverable link to an existing route; the API still owns that route.

Open the frontend URL printed by Aspire. Its port is allocated by the AppHost; the dashboard shows API logs, SQL/Redis health, and traces. `aspire stop --apphost AppHost/AppHost.csproj --non-interactive` stops this app. Development SQL data stays in the `modernize-demo-sql-migrations-data` volume across restarts.

## Test a fresh environment

```sh
dotnet build
pwsh Tests/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet run --project Tests
```

TUnit creates a separate AppHost for each test run, waits for resources, and disposes it afterward. `--UseVolumes=false` prevents tests from reading or modifying your persistent development database. Aspire allocates ports and injects connection strings into the API. Tests use `CreateHttpClient` and `GetConnectionStringAsync` instead of parsing fixed addresses or credentials themselves.

The tests cover a React browser journey, Redis response reuse/invalidation, and connections to the actual SQL/Redis resources. Run them while your development environment is running to demonstrate their isolation.

## Present the migration

Run `before` with Compose and demonstrate add → reload and Redis reuse. Stop Compose, return to the prepared project-sidebar slides, and explain the changes without editing source. Then switch to `after`, start Aspire, and tour the running app, dashboard, and EF commands. The detailed talk guide lives in the website repo at `presentations/decks/wiring-modern-dotnet-with-aspire/rehearsal.md`.

Switch branches only after stopping the current environment and preserving any local edits. `before` stays the Compose baseline; `live` stays available for coding practice. `after` includes the working migration and test setup.

## Entity Framework commands

The API applies the initial Users migration with `MigrateAsync` on startup. The AppHost's `api-migrations` resource exposes **Get Database Status**, **Add Migration…**, **Update Database**, and other EF actions. The backend references `Microsoft.EntityFrameworkCore.Design`; EF can obtain the existing registered DbContext without a separate factory.

```sh
aspire resource api-migrations ef-database-status --apphost AppHost/AppHost.csproj --non-interactive
```

Run that command once before presenting to cache its on-demand EF tool. Adding/removing a migration edits backend source and requires a rebuild before the next migration action. Update Database is normally a no-op because startup already applied the initial migration.

`Aspire.Hosting.EntityFrameworkCore` is currently a preview integration, pinned to `13.6.1-preview.1.26506.6`; the core Aspire packages remain stable `13.6.1`.

The migration-based version uses the new `modernize-demo-sql-migrations-data` volume. The old `modernize-demo-sql-data` volume and Compose volume are preserved, but their EnsureCreated databases are not automatically migrated or copied. Fresh tests opt out of development persistence as before.

## Navigate the demo

- `Backend/Program.cs`: configuration, endpoints, cache policy, invalidation.
- `Backend/DbContexts` and `Backend/Entities`: the small SQL model.
- `frontend/src`: the single-page user workflow.
- `Tests`: API/cache test, browser journey, and environment fixture.
- `compose.yaml` on `before`; `AppHost/AppHost.cs` on `after`.

This is a local teaching app: no authentication or production deployment, and a single initial EF migration. SQL Server requires an x64-capable container runtime; Apple Silicon uses Docker's amd64 emulation. Download packages, browser binaries, and images before presenting. Once those are cached, running and testing the app needs no cloud service.
