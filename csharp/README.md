# Stockroom

A small warehouse inventory API: products with stock levels, orders that reserve
and release that stock, and alerts when a product runs low. Built as a
reference-quality ASP.NET Core service with a layered architecture.

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

## Deploy

```bash
docker build -t ghcr.io/mantish-dev/stockroom-api:1.1.0 .
kubectl apply -f deploy/k8s/
```

The manifests in `deploy/k8s/` expect a `stockroom-data` PersistentVolumeClaim for the
SQLite file. A database created before 1.1.0 needs
`deploy/migrations/0002_add_reorder_threshold.sql` run against it once.

## Stock alerts

Every product has a **reorder threshold**. When shipping an order leaves a product with
that many units available or fewer, Stockroom raises a stock alert; the default notifier
writes it to the log as a warning.

- A new product takes `StockAlerts:DefaultReorderThreshold` (5 unless configured) when
  its request names no threshold.
- `GET /api/stock-alerts` lists every active product that needs reordering right now.
- A discontinued product never raises an alert.

## Test

```bash
dotnet test
```

## Endpoints

| Method | Route                                  | Description                                  |
| ------ | -------------------------------------- | -------------------------------------------- |
| GET    | `/api/products`                        | List products (optionally `?activeOnly`)     |
| GET    | `/api/products/{id}`                   | Product details                              |
| POST   | `/api/products`                        | Create a product                             |
| PUT    | `/api/products/{id}`                   | Update name / price                          |
| PUT    | `/api/products/{id}/reorder-threshold` | Change the reorder threshold                 |
| POST   | `/api/products/{id}/restock`           | Increase stock on hand                       |
| DELETE | `/api/products/{id}`                   | Discontinue a product                        |
| GET    | `/api/stock-alerts`                    | Products at or below their reorder threshold |
| GET    | `/api/orders`                          | List orders (optionally `?status`)           |
| GET    | `/api/orders/{id}`                     | Order details                                |
| POST   | `/api/orders`                          | Place an order (reserves stock)              |
| POST   | `/api/orders/{id}/ship`                | Mark an order shipped (may raise alerts)     |
| POST   | `/api/orders/{id}/cancel`              | Cancel an order (releases stock)             |
