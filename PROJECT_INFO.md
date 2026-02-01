# Orders API - Información del Proyecto

## Resumen General
- **Nombre**: Orders API
- **Framework**: .NET 8
- **Arquitectura**: Clean Architecture + CQRS
- **Ubicación**: `C:\Users\Christian\Desktop\projectos github christian\orders-api\`

---

## Stack Tecnológico

| Componente | Tecnología | Versión |
|------------|------------|---------|
| Framework | .NET | 8.0 |
| Mediator | MediatR | 12.4.1 |
| Validación | FluentValidation | 11.10.0 |
| ORM | Entity Framework Core | 8.0.11 |
| Base de datos | PostgreSQL | 16 |
| Cache/Locking | Redis (StackExchange.Redis) | 2.8.16 |
| Auth | JWT (custom) | - |
| Docs | Swashbuckle (OpenAPI) | 6.9.0 |
| Logging | Serilog | 8.0.3 |
| Tests | xUnit | 2.9.2 |
| Tests | FluentAssertions | 6.12.2 |
| Tests | Testcontainers | 3.10.0 |

---

## Estructura del Proyecto

```
orders-api/
├── src/
│   ├── Orders.Domain/          # Entidades, Value Objects, Domain Events
│   ├── Orders.Application/     # CQRS (Commands, Queries, Handlers)
│   ├── Orders.Infrastructure/  # EF Core, Redis, JWT
│   └── Orders.API/             # Controllers, Middleware
├── tests/
│   ├── Orders.UnitTests/       # Tests de Domain y Application
│   └── Orders.IntegrationTests/# Tests de API con Testcontainers
├── docker-compose.yml
├── docker-compose.override.yml
├── .github/workflows/ci.yml
└── README.md
```

---

## Historial de Commits (15 commits)

| # | Fecha | Hash | Descripción |
|---|-------|------|-------------|
| 1 | 2026-01-02 | 33ef455 | chore: initial project setup |
| 2 | 2026-01-04 | 9e2fa9f | feat(domain): add domain layer with entities and value objects |
| 3 | 2026-01-07 | 1c597a6 | feat(application): add application layer foundation |
| 4 | 2026-01-09 | c9d72e2 | feat(application): implement order CQRS operations |
| 5 | 2026-01-11 | f7a6599 | feat(application): implement product and customer CQRS |
| 6 | 2026-01-14 | 43d0d77 | feat(infrastructure): add EF Core persistence layer |
| 7 | 2026-01-16 | 3916a6d | feat(infrastructure): add Redis caching and distributed locking |
| 8 | 2026-01-18 | 02633f7 | feat(infrastructure): add JWT authentication |
| 9 | 2026-01-21 | fde9d96 | feat(api): add middleware and service extensions |
| 10 | 2026-01-23 | 63dd124 | feat(api): add REST controllers |
| 11 | 2026-01-25 | b924457 | feat(api): configure application startup and settings |
| 12 | 2026-01-27 | 0dd33ad | feat(docker): add containerization support |
| 13 | 2026-01-29 | 65c66ec | test: add unit tests for domain and application layers |
| 14 | 2026-01-31 | 8bfdbc3 | test: add integration tests with Testcontainers |
| 15 | 2026-02-01 | aa0dd37 | docs: add CI pipeline and documentation |

---

## Entidades del Dominio

### Customer
- Id, Email, FirstName, LastName, Phone, ShippingAddress

### Product
- Id, Name, Description, Sku, Price (Money), StockQuantity, IsActive

### Order
- Id, OrderNumber, CustomerId, Status, ShippingAddress, SubTotal, Tax, Total, Notes
- Items (OrderItem collection)

### OrderItem
- Id, OrderId, ProductId, ProductName, ProductSku, UnitPrice, Quantity

---

## Value Objects

- **Money**: Amount, Currency (con operaciones Add, Subtract, Multiply)
- **Address**: Street, City, State, Country, ZipCode
- **OrderStatus**: Pending → Confirmed → Processing → Shipped → Delivered (o Cancelled)

---

## API Endpoints

### Authentication (`/api/v1/auth`)
- `POST /login` - Login con email, retorna JWT
- `POST /validate` - Validar token JWT

### Customers (`/api/v1/customers`)
- `POST /` - Crear cliente (registro)
- `GET /{id}` - Obtener cliente por ID

### Products (`/api/v1/products`)
- `GET /` - Listar productos (paginado)
- `GET /{id}` - Obtener producto por ID
- `POST /` - Crear producto (auth)
- `PUT /{id}` - Actualizar producto (auth)

### Orders (`/api/v1/orders`)
- `GET /` - Listar órdenes (paginado)
- `GET /{id}` - Obtener orden por ID
- `GET /customer/{customerId}` - Órdenes por cliente
- `POST /` - Crear orden (auth)
- `PATCH /{id}/status` - Actualizar estado (auth)
- `POST /{id}/cancel` - Cancelar orden (auth)

### Health Checks
- `GET /health` - Health check completo
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe

---

## Comandos Útiles

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run unit tests only
dotnet test tests/Orders.UnitTests

# Docker Compose
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Swagger UI
http://localhost:5000/swagger
```

---

## Configuración (appsettings.json)

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

---

## Características Especiales

1. **Idempotency**: Header `X-Idempotency-Key` para operaciones POST/PUT
2. **Distributed Locking**: Previene race conditions al crear órdenes
3. **Caching**: Productos cacheados en Redis (TTL 5 min)
4. **Domain Events**: OrderCreatedEvent, OrderStatusChangedEvent
5. **API Versioning**: URL-based (`/api/v1/...`)

---

## Tests

- **35 unit tests** (Domain + Application)
- **7 integration tests** (API con Testcontainers)
- Coverage con `coverlet.collector`

---

## CI/CD (GitHub Actions)

- Build y restore
- Code formatting check (`dotnet format`)
- Unit tests con coverage
- Integration tests
- Security audit (`dotnet list package --vulnerable`)
- Docker image build

---

*Archivo generado el 2026-02-03*
