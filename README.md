# MediBook — Backend API

A microservices healthcare management system built with ASP.NET Core, EF Core, PostgreSQL, and an API Gateway.

## Architecture & Services

All client traffic routes through a unified **API Gateway** down to independent domain services.

```text
API Gateway (Port 5000)
 │
 ├── AuthService          (5002) - JWT Auth & Role management
 ├── AppointmentService   (5003) - Appointment lifecycle & tracking
 ├── PaymentService       (5004) - Billing & secure transactions
 ├── ReviewService        (5005) - Patient feedback & ratings
 ├── NotificationService  (5006) - Automated alerts & reminders
 ├── MedicalRecordService (5007) - Clinical notes & histories
 ├── ProviderService      (5117) - Provider profiles & specialties
 └── ScheduleService      (5298) - Availability & time-slots
```

*Note: Each service follows a standard layered architecture (`Controllers`, `Services`, `Repositories`, `Entities`, `DTOs`).*

## Tech Stack

*   **Framework:** .NET 8 / ASP.NET Core
*   **Database:** PostgreSQL (Entity Framework Core)
*   **Auth:** JWT Bearer tokens
*   **API Docs:** Swagger / OpenAPI

## Getting Started

### 1. Configure Connection Strings
Update `appsettings.json` in each service directory with your local PostgreSQL connection string:
```json
"ConnectionStrings": { "DefaultConnection": "Host=localhost;Database=MediBook_{Service}DB;Username=postgres;Password=pass;" }
```

### 2. Apply Database Migrations
From the solution root, apply EF Core migrations for each service to generate your schemas:
```bash
dotnet ef database update --project MediBook/services/auth-service
# Repeat for appointment, payment, review, medical-record, provider, and schedule services.
```

### 3. Run Services
Start the API Gateway and the required microservices using the .NET CLI:
```bash
dotnet run --project MediBook/services/api-gateway
# Repeat for other services as needed.
```

## API Docs & Auth

*   **Swagger UI:** Available at `http://localhost:{PORT}/swagger` for each individual service.
*   **Authentication:** Obtain a JWT via `POST /api/auth/login` (through Gateway port 5000). Pass it in the `Authorization: Bearer <token>` header for secured routes.

## Testing

Run all unit/integration tests from the solution root:
```bash
dotnet test
```

---
`#dotnet` `#microservices` `#csharp` `#backend` `#postgresql` `#api`