# CourseHub API

Base URL when running locally: `http://localhost:5090`.

The caller is the `sub` claim on a JWT bearer token (`Authorization: Bearer`). The `X-User-Id` header is ignored. A missing or invalid token is 401. A valid token that is not allowed is 403.

| Method | Path | Who | Purpose | Success |
|---|---|---|---|---|
| GET | `/api/courses` | anyone | List all courses with module count, total minutes and active enrollments | 200 |
| GET | `/api/courses/{id}` | anyone | Course detail with modules | 200, 404 |
| POST | `/api/courses` | role `instructor` | Create a course. The instructor id is the caller's `sub`. | 201, 401, 403 |
| PUT | `/api/courses/{id}` | that course's instructor | Update a course. `sub` must equal the course's instructor id. | 204, 401, 403, 404 |
| POST | `/api/courses/{id}/enrollments` | role `learner` | Enroll the caller. The learner id is the caller's `sub`. | 201, 401, 403, 404, 409 |
| DELETE | `/api/courses/{id}/enrollments/me` | role `learner` | Drop the caller's active enrollment | 204, 401, 403, 404 |
| GET | `/api/me/enrollments` | role `learner` | The caller's enrollments | 200, 401, 403 |

Local tokens are created with `dotnet user-jwts`. The signing key stays in user secrets. Do not commit the key or the tokens. `--name` is the caller id and must match the seeded instructor id or the learner id you want stored.

```bash
dotnet user-jwts create --project src/CourseHub.Api --name instructor-ben --role instructor
dotnet user-jwts create --project src/CourseHub.Api --name instructor-ana --role instructor
dotnet user-jwts create --project src/CourseHub.Api --name learner-day3 --role learner
```

Error bodies: 404 `{ "error": "..." }`, 409 `{ "message": "...", "code": "<reason code>" }`. An unhandled exception is `{ "error": "An unexpected error occurred.", "traceId": "..." }` and the exception is logged on the server.

Seed data: 200 courses (5 modules each), instructors `instructor-ana` and `instructor-ben`. Course 1 has 29 active enrollments and capacity 30. Every 10th course is unpublished. Every 25th course has capacity 0. Every 7th course closes enrollment at 2026-12-31 16:00 UTC. `learner-gio` has a dropped enrollment in course 3.
