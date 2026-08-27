# API architecture

The starter is intentionally a modular monolith. It keeps deployment simple while making feature boundaries clear enough to extract later if scale demands it.

## Request flow

`Controller → feature service → StoreDbContext → PostgreSQL`

- `Controllers/` owns HTTP concerns, status codes, and request binding.
- `Contracts/` contains the public response shape consumed by the UI.
- `Services/` owns query and business behavior.
- `Models/` contains persisted domain entities.
- `Data/` owns EF Core configuration, migrations, and development seed data.
- `tests/` exercises the real HTTP pipeline with an isolated in-memory database.

## Team conventions

- Add a feature vertically across contract, service, persistence, endpoint, and tests.
- Do not return EF entities directly from controllers.
- Pass cancellation tokens through asynchronous I/O.
- Treat migration review as part of schema review.
- Keep secrets out of settings files and Git; use environment variables in hosted environments.

Likely next modules are identity, inventory, checkout, orders, payments, and an admin API. Add them as explicit feature folders before considering separate services.
