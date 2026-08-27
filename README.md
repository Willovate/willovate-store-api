# Willovate Store API

The backend for Willovate Store: an ASP.NET Core Web API with PostgreSQL, EF Core migrations, OpenAPI documentation, and integration tests.

## What is ready

- Product catalog list, search, category filtering, and product detail endpoints
- PostgreSQL schema migration and repeatable starter catalog seed
- Swagger UI in development
- CORS configured for the React store
- Health endpoint at `GET /api/health`
- Docker setup and GitHub Actions CI
- Self-contained integration tests (tests do not require PostgreSQL)

## Prerequisites

- .NET SDK 9.0.304 or a compatible 9.0 feature band
- Docker Desktop or another Docker Compose runtime

## Run locally

```bash
cp .env.example .env
docker compose up -d postgres
dotnet tool restore
dotnet restore
dotnet run --project src/Willovate.Store.Api
```

The API starts at `http://localhost:5191`; Swagger is at `http://localhost:5191/swagger`. The application applies pending migrations and seeds the starter catalog during startup.

To run the API and PostgreSQL together in containers:

```bash
docker compose up --build
```

## Validate a change

```bash
dotnet build
dotnet test
dotnet format --verify-no-changes
```

## Database workflow

After changing an entity or EF configuration, create and inspect a migration:

```bash
dotnet tool run dotnet-ef migrations add DescribeTheChange \
  --project src/Willovate.Store.Api \
  --startup-project src/Willovate.Store.Api \
  --output-dir Data/Migrations
```

Do not edit an already deployed migration. Add a follow-up migration instead. Use environment variables or a secrets manager for non-local connection strings; .NET maps `ConnectionStrings__Store` to the configured database connection.

## API surface

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/health` | Service and database readiness |
| `GET` | `/api/products` | Paginated catalog; supports `search`, `category`, `featured`, `page`, `pageSize` |
| `GET` | `/api/products/categories` | Available categories |
| `GET` | `/api/products/{slug}` | Product detail |

See [ARCHITECTURE.md](ARCHITECTURE.md) for code boundaries and [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.
