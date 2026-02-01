# Orders API

[![CI](https://github.com/christian/orders-api/actions/workflows/ci.yml/badge.svg)](https://github.com/christian/orders-api/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/christian/orders-api/branch/main/graph/badge.svg)](https://codecov.io/gh/christian/orders-api)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

A production-ready Orders REST API built with **Clean Architecture** and **CQRS** pattern using **.NET 8**.

## Features

- **Clean Architecture** - Separation of concerns with Domain, Application, Infrastructure, and API layers
- **CQRS Pattern** - Commands and Queries handled separately using MediatR
- **Domain-Driven Design** - Rich domain model with entities, value objects, and domain events
- **JWT Authentication** - Secure API endpoints with custom JWT implementation
- **PostgreSQL** - Robust relational database with EF Core
- **Redis Caching** - Distributed caching for improved performance
- **Distributed Locking** - Prevent race conditions in concurrent operations
- **Idempotency** - Support for idempotent POST/PUT operations via `X-Idempotency-Key` header
- **Health Checks** - Readiness and liveness probes for container orchestration
- **API Versioning** - URL-based API versioning for backward compatibility
- **OpenAPI/Swagger** - Interactive API documentation
- **Structured Logging** - Serilog with console and optional Seq support
- **Docker Support** - Multi-stage Dockerfile and Docker Compose setup
- **CI/CD** - GitHub Actions workflow for build, test, and deploy

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         API Layer                               │
│  (Controllers, Middleware, Extensions)                          │
├─────────────────────────────────────────────────────────────────┤
│                     Application Layer                           │
│  (Commands, Queries, Handlers, Validators, Behaviors)           │
├─────────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                         │
│  (EF Core, Repositories, Redis, JWT)                           │
├─────────────────────────────────────────────────────────────────┤
│                       Domain Layer                              │
│  (Entities, Value Objects, Events, Interfaces)                  │
└─────────────────────────────────────────────────────────────────┘
```

### Project Structure

```
orders-api/
├── src/
│   ├── Orders.Domain/        # Core business logic and entities
│   ├── Orders.Application/   # Use cases (CQRS commands/queries)
│   ├── Orders.Infrastructure/# External concerns (DB, cache, auth)
│   └── Orders.API/           # Web API layer
├── tests/
│   ├── Orders.UnitTests/     # Domain and Application tests
│   └── Orders.IntegrationTests/ # API integration tests
├── docker-compose.yml
└── .github/workflows/ci.yml
```

## Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 8 Minimal APIs + Controllers |
| Architecture | Clean Architecture + CQRS |
| Mediator | MediatR 12.x |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL 16 |
| Cache/Locking | Redis (StackExchange.Redis) |
| Auth | JWT (custom implementation) |
| Docs | OpenAPI/Swagger (Swashbuckle) |
| Logging | Serilog |
| Tests | xUnit + FluentAssertions + Testcontainers |
| CI | GitHub Actions |
| Containers | Docker + Docker Compose |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) (optional, for containers)
- [PostgreSQL 16](https://www.postgresql.org/) (or use Docker)
- [Redis](https://redis.io/) (or use Docker)

### Running with Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/christian/orders-api.git
cd orders-api

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f api
```

The API will be available at:
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

### Running Locally

1. **Start PostgreSQL and Redis** (using Docker):
```bash
docker run -d --name postgres -p 5432:5432 \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=orders_db \
  postgres:16-alpine

docker run -d --name redis -p 6379:6379 redis:7-alpine
```

2. **Run the API**:
```bash
cd src/Orders.API
dotnet run
```

3. **Access the API** at http://localhost:5000/swagger

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run only unit tests
dotnet test tests/Orders.UnitTests

# Run only integration tests (requires Docker for Testcontainers)
dotnet test tests/Orders.IntegrationTests
```

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/login` | Login with email to get JWT token |
| POST | `/api/v1/auth/validate` | Validate a JWT token |

### Customers

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/customers` | Create a new customer (register) |
| GET | `/api/v1/customers/{id}` | Get customer by ID |

### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/products` | Get all products (paginated) |
| GET | `/api/v1/products/{id}` | Get product by ID |
| POST | `/api/v1/products` | Create a new product (auth required) |
| PUT | `/api/v1/products/{id}` | Update a product (auth required) |

### Orders

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/orders` | Get all orders (paginated) |
| GET | `/api/v1/orders/{id}` | Get order by ID |
| GET | `/api/v1/orders/customer/{customerId}` | Get orders by customer |
| POST | `/api/v1/orders` | Create a new order (auth required) |
| PATCH | `/api/v1/orders/{id}/status` | Update order status (auth required) |
| POST | `/api/v1/orders/{id}/cancel` | Cancel an order (auth required) |

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | localhost |
| `ConnectionStrings__Redis` | Redis connection string | localhost:6379 |
| `JwtSettings__Secret` | JWT signing key | - |
| `JwtSettings__Issuer` | JWT issuer | orders-api |
| `JwtSettings__Audience` | JWT audience | orders-api-clients |
| `JwtSettings__ExpirationInMinutes` | Token expiration | 60 |

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=orders_db;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKey...",
    "Issuer": "orders-api",
    "Audience": "orders-api-clients",
    "ExpirationInMinutes": 60
  }
}
```

## Idempotency

For POST and PUT requests, you can include an `X-Idempotency-Key` header to ensure idempotent operations:

```bash
curl -X POST http://localhost:5000/api/v1/orders \
  -H "Authorization: Bearer <token>" \
  -H "X-Idempotency-Key: unique-key-123" \
  -H "Content-Type: application/json" \
  -d '{ ... }'
```

The response will be cached for 24 hours and returned for subsequent requests with the same key.

## Health Checks

| Endpoint | Description |
|----------|-------------|
| `/health` | Full health check (DB + Redis) |
| `/health/ready` | Readiness probe (DB only) |
| `/health/live` | Liveness probe (always 200 OK) |

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Christian** - [GitHub](https://github.com/christian)

---

Built with .NET 8 and Clean Architecture principles.
