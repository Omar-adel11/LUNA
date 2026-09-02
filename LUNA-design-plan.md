# LUNA — Implementation Design Plan

Stack: .NET Web API (backend) + plain HTML/CSS/JS (frontend, multi-page) → React later.
Layout: full responsive redesign (mobile → tablet → desktop).

## 1. Goal & User Flow
A meditation/mindfulness app. Core loop:

Login/Register → Home (greeting, mood check-in, resume-session card, weekly streak)
  → pick an intent (Calm / Focus / Sleep / Reset)
  → Choose Duration
  → Choose Soundscape
  → Breathing Session (timer)
  → Now Playing (post-session player)
Profile/Settings reachable anytime via nav.

## 2. Screens & States
| Screen | States |
|---|---|
| Login | default, submitting, wrong-credentials error, network error |
| Register | default, submitting, validation error, email-taken error |
| Home | loading, loaded, error |
| Intent picker | static |
| Duration picker | static |
| Soundscape picker | loading, loaded, empty |
| Session (timer) | running, paused, completed |
| Now Playing | playing, paused |
| Profile | loading, loaded, editing, saved, error |

## 3. Reusable Components
- Nav bar (bottom on mobile → top/side on desktop)
- Primary / secondary button
- Labeled text input
- Auth tab toggle (Login/Register)
- Mood/intent chip
- Card (resume-session, streak)
- Week-day strip
- Selectable list row (duration, soundscape)
- Circular timer / progress ring
- Playback control bar
- Settings row (icon + label + chevron)
- Avatar

## 4. Backend API (.NET Web API)
Auth:
- `POST /api/auth/register` { name, email, password }
- `POST /api/auth/login` { email, password } → short-lived access token + refresh token
- `POST /api/auth/refresh` { refreshToken } → new access token (rotates the refresh token)
- `POST /api/auth/logout` { refreshToken } → revokes it

User:
- `GET /api/users/me`
- `PUT /api/users/me`
- `PUT /api/users/me/settings` { notifications, language, theme }

Content:
- `GET /api/intents`
- `GET /api/soundscapes`
(durations are a fixed list — hardcode in frontend, no endpoint needed)

Sessions:
- `POST /api/sessions` { intentId, durationMinutes, soundscapeId } → logs session, updates streak
- `GET /api/sessions/streak`
- `GET /api/sessions/last`

## 4b. Caching Strategy
Build one `ICacheService` interface with two implementations, swapped via DI:
1. `InMemoryCacheService` (wraps `IMemoryCache`) — build and test this first. Use it to cache `/api/intents` and `/api/soundscapes` (cache-aside: check cache → miss → hit DB → store in cache → return). This teaches the cache-aside pattern and expiration in isolation.
2. `RedisCacheService` (wraps `IDistributedCache` / `StackExchange.Redis`) — swap this in once Redis is running in Docker. Same interface, same call sites, zero changes to the services that use it. This is the payoff of coding to an interface: you feel *why* it mattered.

Redis also backs:
- **Refresh token store** — token, userId, expiry, revoked flag (or use Redis `EXPIRE` for automatic cleanup)
- **Rate-limit counters** — login/forgot-password attempt counts per IP or email, with a TTL window

## 4c. Background Jobs (Hangfire or Quartz.NET)
- **Expired/revoked refresh-token cleanup** — recurring job, sweeps tokens past expiry from Redis/DB
- **Session reminder emails** — scheduled job checks users who haven't logged a session in N days, sends a reminder via an email provider (MailKit + SMTP, or SendGrid)
- Needs: job runner package, an email-sending service/interface (so it's swappable/testable like the cache), and simple email templates

## 5. Navigation & State Passing (multi-page site)
- JWT stored in `localStorage` after login/register.
- Every page except login/register checks for the token on load; redirect to `login.html` if missing.
- In-progress session setup stored in `sessionStorage` as JSON:
  `{ intent, duration, soundscapeId }`, read/written across `intent.html` → `duration.html` → `soundscape.html` → `session.html`.
- On session completion, POST the object to `/api/sessions`, then clear it from `sessionStorage`.

Pages:
```
login.html
register.html
home.html
intent.html
duration.html
soundscape.html
session.html
profile.html
```

## 6. Frontend Folder Structure
```
frontend/
  login.html
  register.html
  home.html
  intent.html
  duration.html
  soundscape.html
  session.html
  profile.html
  css/
    reset.css
    variables.css
    layout.css
    components.css
  js/
    api.js            (fetch wrapper, base URL, auth header, redirect-if-no-token)
    auth.js           (login/register form logic)
    nav.js            (shared header/nav injection)
    home.js
    session-flow.js    (reads/writes sessionStorage across intent/duration/soundscape)
    session-timer.js
    profile.js
  assets/
    images/ icons/ fonts/
```

## 7. Backend Folder Structure (.NET)
```
backend/
  docker-compose.yml        (api + db + redis, with healthchecks)
  Luna.Api/
    Dockerfile
    Controllers/  AuthController, UsersController, SessionsController,
                   IntentsController, SoundscapesController
    Models/        User, Session, Intent, Soundscape, RefreshToken
    DTOs/
    Data/          AppDbContext (EF Core)
    Caching/       ICacheService, InMemoryCacheService, RedisCacheService
    Auth/          TokenService (issue/validate/rotate JWT + refresh tokens)
    Jobs/          TokenCleanupJob, SessionReminderJob
    Email/         IEmailService, SmtpEmailService
    Middleware/    ExceptionHandlingMiddleware (ProblemDetails), CorrelationIdMiddleware
    Program.cs      (JWT auth, CORS, rate limiting, logging, health checks setup)
```

## 7b. Docker
`docker-compose.yml` runs three services: `api`, `db` (SQL Server/Postgres), `redis` — each with a `healthcheck:` so the API container waits for DB/Redis to actually be ready, not just started. Introduce this **after** the API + DB work locally without Docker (Phase 0 below) — don't containerize before there's anything worth containerizing.

## 8. Build Order

**Phase 0 — Prove the wiring works (bare-bones, no hardening yet)**
1. .NET API: User entity, EF Core migration, *plain* JWT (no refresh yet), register/login endpoints — verify in Swagger.
2. Frontend skeleton: folders, CSS reset/variables, shared nav markup.
3. Build `login.html`/`register.html`, wire to the real API, store the token, redirect to a placeholder `home.html`.
→ Goal: confirm frontend ↔ backend ↔ DB all talk to each other, end to end, before adding any complexity.

**Phase 1 — Harden the backend (before touching any other screen)**
4. Docker: `docker-compose` for API + DB (now that both work locally) — get the app running the same way in a container.
5. Add Redis as a third service in `docker-compose`.
6. Caching: build `ICacheService`, implement `InMemoryCacheService` first and prove cache-aside works on `/api/intents`, then swap in `RedisCacheService` — same call sites, new implementation.
7. Refresh Tokens: short-lived access token + refresh token stored in Redis, `/api/auth/refresh` with rotation, `/api/auth/logout` revocation, expiry handling. Update `login.html`'s `js/auth.js` (and `api.js`) to silently refresh on a 401 instead of booting the user to login.
8. Structured Logging (Serilog) + request correlation IDs; log auth events specifically (login success/fail, refresh, logout).
9. Global Error Handling: exception middleware returning consistent `ProblemDetails` responses.
10. Rate Limiting on login/register/forgot-password, using Redis-backed counters.
11. Background Jobs (Hangfire/Quartz): expired-refresh-token cleanup job, then session-reminder emails (needs an `IEmailService` + SMTP/SendGrid setup + a simple template).
12. Health Checks for API, DB, and Redis — wire into `docker-compose` healthchecks.
→ Goal: by the end of this phase, your auth system is production-grade and every later screen builds on a hardened foundation.

**Phase 2 — Build out the rest of the app (now backed by the hardened API)**
13. Home page: static UI first, then wire `GET /api/users/me` + streak.
14. Intent → Duration → Soundscape chain (pure frontend, sessionStorage).
15. Session timer screen (JS countdown) → POST result to `/api/sessions` on finish.
16. Profile page: GET/PUT user info + settings, including working **Language** switch (JSON string dictionary + `<select>`) and **Theme**/night-mode toggle (CSS variables).
17. Now Playing screen (reuses session player UI/CSS).

**Phase 3 — Polish**
18. Responsive pass: mobile-first CSS, then tablet/desktop breakpoints, screen by screen.
19. Loading spinners, error banners (using your `ProblemDetails` responses), empty states, real streak data on the week strip.
