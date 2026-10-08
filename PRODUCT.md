# Modernize with Aspire

A small application for exploring a migration from Docker Compose to Aspire. The React frontend calls a .NET API backed by SQL Server and Redis output caching. The example demonstrates local orchestration, resource configuration and readiness, OpenTelemetry, Entity Framework commands, and integration/end-to-end tests.

The `before` branch provides the Compose baseline. The `after` branch provides the completed Aspire version. The `live` branch starts from the baseline for hands-on exploration. Keep the business feature and code structure simple so the wiring changes are easy to compare.

The frontend follows the tarkan.dev visual identity: pine and ochre colors, Barlow Condensed headings, Public Sans body text, rectangular controls, and open list rows. All application services run locally; no cloud account is needed.
