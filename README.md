# Modernize with Aspire

Explore the sample from [Wiring Modern .NET Together with Aspire](https://tarkan.dev/presentations/wiring-modern-dotnet-with-aspire/): a React frontend, a .NET API, SQL Server, and Redis. Compare a working Docker Compose application with the same application wired through Aspire.

`GET /users` is cached in Redis for one minute. Adding a person invalidates the cached response. The `X-Database-Read` response header changes when the API queries SQL, so you can observe cache reuse in the frontend.

## Get started

Install these tools:

- .NET 10 SDK matching `global.json`.
- Node.js 22.12 or newer.
- Docker with Docker Compose, or a compatible container runtime.
- [Aspire CLI](https://aspire.dev/get-started/install-cli/).
- PowerShell (`pwsh`) for the Playwright browser installation below.

SQL Server uses an x64 container image. On Apple Silicon, enable amd64 emulation in your container runtime. The first run downloads packages and container images; the application itself needs no cloud services.

```sh
git clone --branch after https://github.com/Puharesource/presentations-wiring-modern-dotnet-with-aspire.git
cd presentations-wiring-modern-dotnet-with-aspire
```

| Branch | What to explore |
| --- | --- |
| `after` | Completed Aspire AppHost, ServiceDefaults, and isolated integration/end-to-end tests. |
| `before` | The same feature running with Docker Compose and explicit connection settings. |
| `live` | The Compose baseline, ready for you to try the migration yourself. |

## Run the Aspire version

On `after`, run:

```sh
aspire start --apphost AppHost/AppHost.csproj
aspire wait frontend --apphost AppHost/AppHost.csproj
aspire describe --apphost AppHost/AppHost.csproj
```

Open the dashboard address printed by Aspire and launch the `frontend` resource from its URL. The frontend port is allocated at startup. The API also has an **OpenAPI** dashboard link to `/openapi/v1.json`.

Try this workflow:

1. Add John Doe or Jane Doe in the frontend.
2. Refresh the list to see Redis reuse the response.
3. Add another person to invalidate the cache and trigger a new SQL read.
4. Open **Traces**, select **All** or **api**, and inspect `POST /users`: the request includes SQL work and Redis cache invalidation. React does not emit its own browser spans in this sample.
5. Explore Resources, Graph, logs, metrics, and the `api-migrations` Entity Framework commands.

Stop Aspire before changing branches:

```sh
aspire stop --apphost AppHost/AppHost.csproj
```

The development SQL volume persists across restarts. Each test run uses its own fresh environment.

## Run the Compose baseline

After stopping Aspire, switch to `before` (or `live`):

```sh
git switch before
cp .env.example .env
docker compose up --build --wait
```

Open [localhost:5173](http://localhost:5173). The API is at [localhost:5244](http://localhost:5244), with OpenAPI at `/openapi/v1.json`. The example password is only for this local sample.

Before switching back to Aspire:

```sh
docker compose down
git switch after
```

Compose and Aspire share the API address `localhost:5244`. Run one development version at a time: otherwise the frontend can reach the Compose API and its requests will not appear in Aspire's traces. If that happens, stop Compose and restart Aspire.

`docker compose down` preserves the SQL volume. The Compose and Aspire development databases have separate volumes; their records do not transfer between branches.

## Run the tests

Build the selected branch, install the browser once, then run the tests:

```sh
dotnet build
pwsh Tests/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet run --project Tests
```

On `before` and `live`, start Compose first: the fixture expects the application to be running at its local addresses. You can override them with `API_URL` and `FRONTEND_URL` environment variables.

On `after`, TUnit creates a separate Aspire environment for each test run, waits for readiness, and disposes it afterward. `--UseVolumes=false` keeps it separate from your development data. You can leave the development Aspire environment running while tests execute. Tests obtain endpoints and connection strings from the running AppHost.

Tests cover API cache reuse/invalidation and a React browser journey. The Aspire version also verifies connections to SQL and Redis. Isolation is per test run, rather than per test method.

## Understand the migration

Compare the branches:

```sh
git diff before..after -- Backend/Program.cs
git diff before..after -- AppHost ServiceDefaults Tests
```

Both versions use the same SQL/Redis client packages, connection names, request handlers, and EF migration. Aspire changes who supplies configuration and owns startup:

- `AppHost/AppHost.cs` declares SQL, Redis, the API, React, resource references, and readiness waits.
- `ServiceDefaults/Extensions.cs` configures shared health endpoints and OpenTelemetry.
- `Backend/Program.cs` adds shared defaults and delegates migration startup to AppHost; the SQL and Redis registrations stay the same.
- `Tests/DemoFixture.cs` changes from external endpoints to an Aspire-owned environment.

For an exercise, switch to `live` and use `after` as the reference implementation. Preserve your edits before switching branches.

## Entity Framework commands

AppHost runs pending migrations through `RunDatabaseUpdateOnStart()`. The API waits for `api-migrations` before accepting requests. The dashboard exposes **Get Database Status**, **Add Migration…**, **Update Database**, and other EF actions.

For example, on `after` with Aspire running:

```sh
aspire resource api-migrations ef-database-status --apphost AppHost/AppHost.csproj
```

The first EF command may download its tool. Adding or removing a migration changes backend source; rebuild before the next migration action. This sample's automatic migration policy is for local development and tests. Production migration policy depends on your deployment.

Core Aspire packages are pinned to `13.6.1`; the EF hosting integration is the preview package `13.6.1-preview.1.26506.6`.

## Find your way around

- `Backend/Program.cs`: endpoints, cache policy, and invalidation.
- `Backend/DbContexts` and `Backend/Entities`: the small SQL model.
- `Backend/Migrations`: the initial database schema.
- `frontend/src`: the single-page React feature.
- `Tests`: the fixture and API/browser tests.
- `compose.yaml` on `before`/`live`; `AppHost` and `ServiceDefaults` on `after`.

This is a small local learning application, without authentication or a production deployment configuration.
