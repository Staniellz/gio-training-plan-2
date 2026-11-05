# Secrets

Where credentials live for CourseHub (`gio-training-plan-2`) and the Plan 1 repo (`hello-logging-structured-logging`). Updated 2026-11-05.

## Where each secret lives

| Secret | Dev | CI | Production |
|---|---|---|---|
| CourseHub Postgres password | Gitignored `.env` key `POSTGRES_PASSWORD`. Compose loads that file with `env_file`. The API reads `ConnectionStrings:CourseHub` from `dotnet user-secrets`, not from `appsettings.json`. | GitHub Actions builds and runs unit tests only. No database and no secret. | Not deployed. When it is, set the connection string in App Service configuration or Key Vault. Not in the image and not in the repo. |
| CourseHub JWT signing key | `dotnet user-jwts` stores it in user secrets for `src/CourseHub.Api`. `appsettings.Development.json` has the issuer and audience only. | Tests do not call the API. No signing key in CI. | Not deployed. |
| HelloLogging Postgres password | Gitignored `.env` key `POSTGRES_PASSWORD`, loaded with `env_file`. The API appends it to `ConnectionStrings__Db` at startup when that string has no password yet. For `dotnet run` on the host, user-secrets `ConnectionStrings:Db` holds the full string. | No CI job uses this database. | Not deployed. Same rule: host settings or Key Vault. |
| HelloLogging Grafana admin password | Gitignored `.env` key `GF_SECURITY_ADMIN_PASSWORD`. Compose loads `.env` with `env_file`, so the yaml file has no password value. | Not used in CI. | Not deployed. |

`.env.example` is committed and has the key names with placeholder values. `.env` is gitignored.

## Rotation

| Secret | Real or placeholder | Rotated | Removal date | Why it was not rotated |
|---|---|---|---|---|
| HelloLogging Postgres (`compose.yaml` line 12, same value again on line 24) | Dev-only placeholder | No | 2026-11-04 | Local Docker only. Never a shared or production credential. |
| HelloLogging Grafana admin (`compose.yaml` line 65) | Dev-only placeholder | No | 2026-11-04 | Local Grafana only. |
| CourseHub Postgres (Day 1 seed `docker-compose.yml` and `appsettings.json`) | Dev-only placeholder shipped in the starter | No | 2026-11-04 | Local Postgres only. |

Nothing was rotated. The private `.env` on this machine still holds the old placeholder so an existing Docker volume keeps starting. That file is not committed. A fresh clone should choose a new `.env` value and run `docker compose down -v` before the first start.

## A new commit does not erase a secret

Git keeps every blob that an old commit can reach. Deleting the line only changes the latest tree. Anyone who can clone the repo can check out the old commit and read the value. Secret scanning and push protection block new pushes. They do not scrub history.

## Remedy chosen for each repo

History was not rewritten in either repo.

- Plan 1, `hello-logging-structured-logging` PR 2: the three GitGuardian findings moved to a gitignored `.env`, with a committed `.env.example`. The API connection string for local `dotnet run` is in user-secrets. A history rewrite would force-push `w1-docker-integration`, which the EM has already reviewed, and the values are local placeholders.
- CourseHub, `gio-training-plan-2`: password removed from `appsettings.json` and `docker-compose.yml`, same `.env` plus user-secrets split. `main` is protected, the Day 1 seed commit is the baseline CI already ran, and the value is the starter's local placeholder. A force-push was the wrong remedy.

If a value had been a real credential, the order would be: revoke it and record the rotation date here, remove it from the tree, then rewrite history only if the old value still mattered.

## Plan 1 findings

1. `compose.yaml:12` — Username Password inside `ConnectionStrings__Db`. Dev-only placeholder. Moved to `POSTGRES_PASSWORD` in `.env`. The same value on line 24 was removed in the same change.
2. `compose.yaml:65` — Generic Password, Grafana admin. Dev-only placeholder. Moved to `GF_SECURITY_ADMIN_PASSWORD` in `.env`.
3. `outputs/w2-thu-monitoring-deliverable-summary.md:47` (same value on line 46) — Curl Username Password. Same Grafana placeholder. The commands now read `GF_SECURITY_ADMIN_PASSWORD` from the environment. Copies in `README.md` (the services table and the dashboard URL) were removed as well.

GitGuardian's check on Plan 1 PR 2 still lists the original commits. The scan covers every commit in the pull request, not only the latest tree. Those three historical occurrences stay until history is rewritten. The latest commit does not add a password literal.
