# Modernize with Aspire

A deliberately small users application: frontend → .NET API → SQL Server, with Redis caching `GET /users` for one minute. Creating a user invalidates the cached response. `X-Database-Read` changes only on an actual database read, making cache hits visible.

## Talk branches

- `before`: Docker Compose and manually supplied configuration.
- `after`: Aspire 13.6.1 AppHost, service defaults, and isolated TUnit integration/end-to-end tests.
- `live`: starts at `before`; use it for the live migration.

The original folders are preserved outside this repository at `../ModernizeWithAspire-originals-20261007`. Both originals already contained Aspire and Angular; `before` is an explicit Compose baseline derived from their users feature.

## Run with Aspire

```sh
aspire start --apphost AppHost/AppHost.csproj --non-interactive
aspire wait frontend --apphost AppHost/AppHost.csproj --non-interactive
aspire describe --apphost AppHost/AppHost.csproj --non-interactive
```

Open the frontend URL printed by Aspire. Its port is allocated by the AppHost; the dashboard shows API logs, SQL/Redis health, and traces. `aspire stop --apphost AppHost/AppHost.csproj --non-interactive` stops this app. Development SQL data stays in the `modernize-demo-sql-data` volume across restarts.

## Test a fresh environment

```sh
dotnet build
pwsh Tests/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet run --project Tests
```

TUnit creates a separate AppHost for each test run, waits for resources, and disposes it afterward. `--UseVolumes=false` prevents tests from reading or modifying your persistent development database. Aspire allocates ports and injects connection strings into the API. Tests use `CreateHttpClient` and `GetConnectionStringAsync` instead of parsing fixed addresses or credentials themselves.

The tests cover a React browser journey, Redis response reuse/invalidation, and connections to the actual SQL/Redis resources. Run them while your development environment is running to demonstrate their isolation.

## Rehearse the migration

Start on `live` (initially identical to `before`). Run Compose and show the users page, cache reuse, and configuration. Stop Compose before moving to Aspire. Compare with `git diff before..after -- AppHost Backend Tests` as your reference. Add the AppHost resources, replace the backend's manual registrations with Aspire integrations, then replace the test fixture. The UI and business endpoints stay the same.

Switch branches only after stopping the current environment. On `before`/`live`, use the Compose commands documented in that branch's README. On `after`, use the commands above.

## Navigate the demo

- `Backend/Program.cs`: configuration, endpoints, cache policy, invalidation.
- `Backend/DbContexts` and `Backend/Entities`: the small SQL model.
- `frontend/src`: the single-page user workflow.
- `Tests`: API/cache test, browser journey, and environment fixture.
- `compose.yaml` on `before`; `AppHost/AppHost.cs` on `after`.

This is a local teaching app: no authentication or production deployment, and `EnsureCreated` instead of migrations. SQL Server requires an x64-capable container runtime; Apple Silicon uses Docker's amd64 emulation. Download packages, browser binaries, and images before presenting. Once those are cached, running and testing the app needs no cloud service.
