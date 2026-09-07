# Stockroom

A small warehouse inventory API: products with stock levels, and orders that reserve
and release that stock. Built as a reference-quality ASP.NET Core service with a
layered architecture.

```
src/
  Stockroom.Domain          entities, invariants, domain exceptions (no dependencies)
  Stockroom.Application     use cases, DTOs, repository and clock abstractions
  Stockroom.Infrastructure  EF Core + SQLite persistence, system clock
  Stockroom.Api             controllers, error handling, composition root
tests/
  Stockroom.Tests           xUnit v3 unit tests against in-memory fakes
```

## Run

```bash
dotnet run --project src/Stockroom.Api
```

The API listens on `http://localhost:5080`. OpenAPI document: `/openapi/v1.json`.
The SQLite database is created on startup at `stockroom.db` next to the executable.

## Test

```bash
dotnet test
```

## Endpoints

| Method | Route                          | Description                              |
| ------ | ------------------------------ | ---------------------------------------- |
| GET    | `/api/products`                | List products (optionally `?activeOnly`) |
| GET    | `/api/products/{id}`           | Product details                          |
| POST   | `/api/products`                | Create a product                         |
| PUT    | `/api/products/{id}`           | Update name / price                      |
| POST   | `/api/products/{id}/restock`   | Increase stock on hand                   |
| DELETE | `/api/products/{id}`           | Discontinue a product                    |
| GET    | `/api/orders`                  | List orders (optionally `?status`)       |
| GET    | `/api/orders/{id}`             | Order details                            |
| POST   | `/api/orders`                  | Place an order (reserves stock)          |
| POST   | `/api/orders/{id}/ship`        | Mark an order shipped                    |
| POST   | `/api/orders/{id}/cancel`      | Cancel an order (releases stock)         |
