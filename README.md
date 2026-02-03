# Orders API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![CI](https://github.com/GarrettChristianAntonio/orders-api/actions/workflows/ci.yml/badge.svg)](https://github.com/GarrettChristianAntonio/orders-api/actions/workflows/ci.yml)

REST API para gestión de pedidos. Lo armé para practicar Clean Architecture con CQRS en .NET 8.

## Stack

- .NET 8 con MediatR (CQRS)
- PostgreSQL + EF Core
- Redis para cache y distributed locking
- JWT auth custom
- xUnit + Testcontainers

## Cómo correr

Con Docker (lo más fácil):

```bash
docker-compose up -d
```

Swagger en http://localhost:5000/swagger

### Sin Docker

Necesitas Postgres y Redis corriendo, después:

```bash
cd src/Orders.API
dotnet run
```

## Endpoints principales

**Auth**
- `POST /api/v1/auth/login` - obtener token

**Orders**
- `GET /api/v1/orders` - listar
- `POST /api/v1/orders` - crear (necesita auth)
- `PATCH /api/v1/orders/{id}/status` - cambiar estado

**Products**
- `GET /api/v1/products` - listar
- `POST /api/v1/products` - crear (auth)

**Customers**
- `POST /api/v1/customers` - registrar
- `GET /api/v1/customers/{id}` - obtener

## Tests

```bash
dotnet test
```

35 unit tests + 7 integration tests con Testcontainers.

## Estructura

```
src/
├── Orders.Domain/        # entidades, value objects
├── Orders.Application/   # commands, queries, handlers
├── Orders.Infrastructure/# EF Core, Redis, JWT
└── Orders.API/           # controllers, middleware
```

## Notas

- Soporta idempotency con header `X-Idempotency-Key`
- Health checks en `/health`, `/health/ready`, `/health/live`
- Los productos se cachean 5 min en Redis
