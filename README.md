# CourseHub

ASP.NET Core 10 Web API for a small course catalog with enrollments. PostgreSQL through EF Core. This repo is your Training Plan 2 workspace.

## Requirements

- .NET SDK 10 (see `global.json`)
- Docker (PostgreSQL 17 and Redis 7 run in containers)

## Run

The database password is not in the repo. Copy `.env.example` to `.env`, set `POSTGRES_PASSWORD`, then store the same value in user secrets for the API.

```bash
cp .env.example .env
docker compose up -d postgres
dotnet build
dotnet test
dotnet user-secrets set "ConnectionStrings:CourseHub" "Host=localhost;Port=5432;Database=coursehub;Username=coursehub;Password=<POSTGRES_PASSWORD from .env>" --project src/CourseHub.Api
dotnet run --project src/CourseHub.Api
```

On PowerShell, `cp .env.example .env` is `Copy-Item .env.example .env`.

The API listens on `http://localhost:5090`. On first start it creates the database and seeds 200 courses. Calls that change data send `Authorization: Bearer`. The signing key stays in user secrets; create tokens with `dotnet user-jwts` as described in `docs/api.md`. Sample calls are in `requests.http`. Enrollment rules are in `docs/enrollment-rules.md`. Where secrets live, including the Plan 1 repo, is in `docs/secrets.md`.

If Postgres was already created with an older password, either put that same value in `.env` and user secrets, or run `docker compose down -v` and start again.

Redis is in `docker-compose.yml` for later work: `docker compose up -d redis`.

To reset the data: `docker compose down -v`, then start again.

## Layout

| Path | Contents |
|---|---|
| `src/CourseHub.Api` | Controllers, startup |
| `src/CourseHub.Core` | Domain, `CourseHubDbContext`, seed data, enrollment policy |
| `tests/CourseHub.Core.Tests` | xUnit tests |
| `.github/workflows/ci.yml` | Build and test on every PR and on `main` |
