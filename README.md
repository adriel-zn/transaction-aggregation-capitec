# Transaction Aggregation API

Production-style .NET 8 backend that aggregates financial transactions from multiple mock providers, normalizes them into a canonical model, categorizes them, persists them to PostgreSQL, and exposes query and summary APIs.

## Why this design

The project uses a modular layered architecture with explicit application abstractions. Provider-specific formats are isolated behind `ITransactionProvider`, while the application layer works only with normalized transactions. PostgreSQL provides durable persistence, aggregate querying, indexing, and a database-enforced idempotency constraint.

A modular monolith was chosen deliberately instead of microservices. The supplied requirements do not justify independently deployable services, and this design keeps operational complexity low while still preserving clear boundaries for future extraction.

## Tech stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- PostgreSQL 17
- Swagger / OpenAPI
- xUnit + FluentAssertions
- Testcontainers for integration testing
- Docker / Docker Compose
- GitHub Actions

## Architecture

```text
REST API
   |
Application services
   |---- ITransactionProvider ---- BankA / BankB / BankC adapters
   |---- ITransactionCategorizer
   |---- ITransactionRepository
                  |
              PostgreSQL
```

## Run with Docker

```bash
docker compose up --build
```

The API will be available at:

- API: `http://localhost:8080`
- Swagger (Development): `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`

## Example usage

### Import transactions

```bash
curl -X POST http://localhost:8080/api/v1/customers/cust-123/transactions/import \
  -H "Content-Type: application/json" \
  -d '{
    "from": "2026-09-01T00:00:00Z",
    "to": "2026-09-30T23:59:59Z"
  }'
```

### Get transactions

```bash
curl "http://localhost:8080/api/v1/customers/cust-123/transactions?page=1&pageSize=20&category=Groceries"
```

### Get summary

```bash
curl "http://localhost:8080/api/v1/customers/cust-123/summary?from=2026-09-01T00:00:00Z&to=2026-09-30T23:59:59Z"
```

### Get category summary

```bash
curl "http://localhost:8080/api/v1/customers/cust-123/categories?from=2026-09-01T00:00:00Z&to=2026-09-30T23:59:59Z"
```

## Local development

Requirements:

- .NET 8 SDK
- PostgreSQL 17, or run PostgreSQL via Docker

Start only the database:

```bash
docker compose up postgres -d
```

Then:

```bash
dotnet restore
dotnet run --project src/TransactionAggregation.Api
```

## Tests

```bash
dotnet test
```

Integration tests use Testcontainers and therefore require Docker.

## Important implementation decisions

### Idempotency

Transactions are uniquely identified by `(Source, ExternalTransactionId)`. A unique PostgreSQL index is the final concurrency-safe guarantee against duplicates. Application-only `exists` checks would still be vulnerable to race conditions.

### Partial provider failures

The aggregation service runs providers independently. If one provider fails, successful provider results can still be persisted and the import response reports the provider failure. This prevents useful data from being discarded because another source is temporarily unavailable.

### Money

`decimal` is used for monetary values. Binary floating-point types such as `double` are intentionally avoided for financial amounts.

### Categorization

The default implementation is a deterministic rule-based categorizer. It is isolated behind `ITransactionCategorizer`, so it could later be replaced by database-driven rules or a machine-learning classifier without changing the aggregation workflow.

### Security

Authentication was not added because identity requirements are outside the brief. In production the API should be protected using OAuth 2.0/OIDC, authorization should enforce customer/account scope, secrets should come from a secret manager, and TLS should terminate at the ingress/load-balancer layer. Sensitive transaction details should not be included in application logs.

## Production improvements

Given additional requirements or scale, useful extensions would include:

- Retry / timeout / circuit-breaker policies for real upstream HTTP providers
- OpenTelemetry tracing and metrics
- Background ingestion using a message broker when throughput requirements justify it
- OAuth2/OIDC authentication and policy-based authorization
- Database-driven categorization rules with effective dates and audit history
- Currency-aware aggregation rather than assuming a single reporting currency
- Bulk insert / PostgreSQL `ON CONFLICT DO NOTHING` for very large import batches
- Separate read models if analytical query load becomes substantial

## API behavior

### `POST /api/v1/customers/{customerId}/transactions/import`

Aggregates all providers for the requested date range and returns counts for received, created, duplicate, successful source, and failed source records.

### `GET /api/v1/customers/{customerId}/transactions`

Supports:

- `from`
- `to`
- `category`
- `page` (1-based)
- `pageSize` (max 200)

### `GET /api/v1/customers/{customerId}/summary`

Returns income, expenses, net movement, transaction count, and currency.

### `GET /api/v1/customers/{customerId}/categories`

Returns expense totals grouped by category.

## Interview discussion points

Be prepared to explain:

- Why a modular monolith was selected rather than microservices
- Why PostgreSQL is appropriate for the data model
- Why idempotency is enforced in the database
- How concurrent imports behave
- How partial source failures are handled
- How provider adapters prevent external schemas leaking into the domain
- How the architecture would evolve toward asynchronous ingestion
- How authentication and authorization would be added
- How you would support multiple currencies
- How you would improve bulk ingestion performance
