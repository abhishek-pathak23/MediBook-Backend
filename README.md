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

## ER Diagram

```mermaid
erDiagram
    USER {
        int UserId PK
        string FullName
        string Email
        string PasswordHash
        string Phone
        string Role
        bool IsActive
        datetime CreatedAt
        string ProfilePicUrl
    }
    REFRESH_TOKEN {
        int Id PK
        string Token
        datetime ExpiryDate
        bool IsRevoked
        int UserId FK
    }
    PROVIDER {
        int ProviderId PK
        int UserId FK
        string Specialization
        string Qualification
        int ExperienceYears
        string Bio
        string ClinicName
        string ClinicAddress
        double AvgRating
        bool IsVerified
        bool IsAvailable
        bool IsActive
        date CreatedAt
    }
    AVAILABILITY_SLOT {
        int SlotId PK
        int ProviderId FK
        date Date
        time StartTime
        time EndTime
        int DurationMinutes
        bool IsBooked
        bool IsBlocked
        string Recurrence
        datetime CreatedAt
    }
    APPOINTMENT {
        int AppointmentId PK
        int PatientId FK
        int ProviderId FK
        int SlotId FK
        string ServiceType
        date AppointmentDate
        time StartTime
        time EndTime
        string Status
        string Notes
        string ModeOfConsultation
        datetime CreatedAt
        datetime UpdatedAt
    }
    PAYMENT {
        int PaymentId PK
        int AppointmentId FK
        int PatientId FK
        int ProviderId FK
        decimal Amount
        string Currency
        string Status
        string Mode
        string TransactionId
        datetime PaidAt
        datetime RefundedAt
        datetime CreatedAt
        datetime UpdatedAt
    }
    MEDICAL_RECORD {
        int RecordId PK
        int AppointmentId FK
        int PatientId FK
        int ProviderId FK
        string Diagnosis
        string Prescription
        string Notes
        string AttachmentUrl
        date FollowUpDate
        datetime CreatedAt
        datetime UpdatedAt
    }
    REVIEW {
        int ReviewId PK
        int AppointmentId FK
        int PatientId FK
        int ProviderId FK
        int Rating
        string Comment
        datetime ReviewDate
        bool IsVerified
        bool IsAnonymous
    }
    NOTIFICATION {
        int NotificationId PK
        int RecipientId FK
        string Type
        string Title
        string Message
        string Channel
        int RelatedId
        bool IsRead
        datetime SentAt
    }

    USER ||--o{ REFRESH_TOKEN : "has"
    USER ||--o| PROVIDER : "becomes"
    PROVIDER ||--o{ AVAILABILITY_SLOT : "creates"
    PROVIDER ||--o{ APPOINTMENT : "receives"
    USER ||--o{ APPOINTMENT : "books"
    AVAILABILITY_SLOT ||--o| APPOINTMENT : "used in"
    APPOINTMENT ||--o| PAYMENT : "has"
    APPOINTMENT ||--o| MEDICAL_RECORD : "generates"
    APPOINTMENT ||--o| REVIEW : "gets"
    USER ||--o{ NOTIFICATION : "receives"
```

---

## Architectural Diagram

```mermaid
graph TB
    subgraph CLIENT["Client Layer"]
        ANG["Angular Frontend - Vercel"]
    end

    subgraph GATEWAY["API Gateway Layer"]
        GW["API Gateway - Ocelot - Render"]
    end

    subgraph SERVICES["Microservices Layer - Render Docker"]
        AUTH["auth-service"]
        PROV["provider-service"]
        SCHED["schedule-service"]
        APPT["appointment-service"]
        PAY["payment-service"]
        NOTIF["notification-service"]
        MED["medical-record-service"]
        REV["review-service"]
    end

    subgraph DB["Database Layer"]
        PG[("PostgreSQL - Render Managed DB")]
    end

    subgraph EXTERNAL["External Services"]
        RZP["Razorpay Payment Gateway"]
        SIGNALR["SignalR Real-time WebSocket"]
    end

    ANG -->|"All API calls HTTPS"| GW
    GW --> AUTH
    GW --> PROV
    GW --> SCHED
    GW --> APPT
    GW --> PAY
    GW --> NOTIF
    GW --> MED
    GW --> REV

    AUTH --- PG
    PROV --- PG
    SCHED --- PG
    APPT --- PG
    PAY --- PG
    NOTIF --- PG
    MED --- PG
    REV --- PG

    APPT -->|"HTTP POST"| NOTIF
    APPT -->|"HTTP GET/PUT"| SCHED
    APPT -->|"HTTP POST"| PAY
    PAY -->|"HTTP PUT status"| APPT
    REV -->|"HTTP PUT rating"| PROV
    AUTH -->|"HTTP PUT status"| PROV

    PAY <-->|"Create Order / Verify Signature"| RZP
    NOTIF <-->|"Push Events"| SIGNALR
    ANG <-->|"WebSocket"| SIGNALR
```

---

## Low Level Design

```mermaid
graph TB
    subgraph HTTP["HTTP Request"]
        REQ["POST /api/v1/appointments"]
    end

    subgraph MW["Middleware Layer"]
        EX["ExceptionHandlingMiddleware"]
        JWT_MW["JWT Auth Middleware"]
    end

    subgraph CTRL["Controller Layer"]
        AC["AppointmentController"]
    end

    subgraph SVC["Service Layer"]
        AS["AppointmentService implements IAppointmentService"]
    end

    subgraph HTTPSVC["HTTP Service Layer"]
        NHS["NotificationHttpService"]
        SHS["ScheduleHttpService"]
        PHS["PaymentHttpService"]
    end

    subgraph REPO["Repository Layer"]
        AR["AppointmentRepository implements IAppointmentRepository"]
    end

    subgraph DATA["Data Layer"]
        DBC["ApplicationDbContext extends DbContext"]
    end

    subgraph DB["Database"]
        PG[("PostgreSQL - Appointments Table")]
    end

    subgraph EXT["External Microservices"]
        NS["notification-service"]
        SS["schedule-service"]
        PS["payment-service"]
    end

    REQ --> EX --> JWT_MW --> AC
    AC -->|"DI Inject"| AS
    AS -->|"DI Inject"| AR
    AS -->|"DI Inject"| NHS
    AS -->|"DI Inject"| SHS
    AS -->|"DI Inject"| PHS
    AR -->|"DI Inject"| DBC
    DBC --> PG
    NHS -->|"HttpClient POST"| NS
    SHS -->|"HttpClient GET/PUT"| SS
    PHS -->|"HttpClient POST"| PS
```

---

## UML Class Diagram

```mermaid
classDiagram
    class IAppointmentService {
        <<interface>>
        +BookAppointmentAsync(appointment) Task~Appointment~
        +BookAppointment(appointment) Appointment
        +GetById(id) Appointment
        +GetByPatient(patientId) List~Appointment~
        +CancelAppointmentAsync(id) Task
        +RescheduleAppointmentAsync(id, slotId) Task~Appointment~
        +UpdateStatusAsync(id, status) Task~string~
        +GetAllAppointments() List~Appointment~
    }

    class IAppointmentRepository {
        <<interface>>
        +Add(appointment) void
        +Update(appointment) void
        +Delete(appointment) void
        +GetById(id) Appointment
        +FindByPatientId(id) List~Appointment~
        +FindByProviderId(id) List~Appointment~
        +FindBySlotId(id) Appointment
        +SaveChanges() bool
    }

    class INotificationService {
        <<interface>>
        +SendBookingConfirmationAsync() Task
        +SendCancellationAlertAsync() Task
        +BroadcastDashboardEventAsync() Task
    }

    class AppointmentService {
        -IAppointmentRepository _repo
        -IScheduleService _schedSvc
        -IPaymentService _paySvc
        -INotificationService _notifSvc
        -HashSet~string~ ValidStatuses
        +BookAppointmentAsync() Task~Appointment~
        +CancelAppointmentAsync() Task
        +RescheduleAppointmentAsync() Task~Appointment~
        +CompleteAppointment() void
        +UpdateStatusAsync() Task~string~
    }

    class AppointmentRepository {
        -ApplicationDbContext _context
        +Add(appointment) void
        +Update(appointment) void
        +GetById(id) Appointment
        +FindByPatientId(id) List~Appointment~
        +FindUpcomingByPatientId(id) List~Appointment~
        +SaveChanges() bool
    }

    class NotificationHttpService {
        -HttpClient _httpClient
        -IConfiguration _config
        +SendBookingConfirmationAsync() Task
        +SendCancellationAlertAsync() Task
        +BroadcastDashboardEventAsync() Task
    }

    class ApplicationDbContext {
        +DbSet~Appointment~ Appointments
        +OnModelCreating(builder) void
    }

    class DbContext {
        <<abstract>>
        +SaveChanges() int
        +OnModelCreating(builder) void
    }

    class ControllerBase {
        <<abstract>>
        +Ok() OkResult
        +BadRequest() BadRequestResult
        +NotFound() NotFoundResult
    }

    class AppointmentController {
        -IAppointmentService _apptService
        +Book(dto) Task~IActionResult~
        +GetById(id) IActionResult
        +Cancel(id) Task~IActionResult~
        +Reschedule(id, dto) Task~IActionResult~
        +Complete(id) IActionResult
        +UpdateStatus(id, dto) Task~IActionResult~
    }

    class Appointment {
        +int AppointmentId
        +int PatientId
        +int ProviderId
        +int SlotId
        +string Status
        +GetStatus() string
        +SetStatus(status) void
    }

    class NotifHub {
        +OnConnectedAsync() Task
        +OnDisconnectedAsync(ex) Task
    }

    class Hub {
        <<abstract>>
        +OnConnectedAsync() Task
        +OnDisconnectedAsync(ex) Task
    }

    class FollowUpReminderService {
        -IServiceScopeFactory _scopeFactory
        +ExecuteAsync(token) Task
        -CheckFollowUps() Task
    }

    class BackgroundService {
        <<abstract>>
        +ExecuteAsync(token) Task
    }

    AppointmentService ..|> IAppointmentService : implements
    AppointmentRepository ..|> IAppointmentRepository : implements
    NotificationHttpService ..|> INotificationService : implements
    ApplicationDbContext --|> DbContext : inherits
    AppointmentController --|> ControllerBase : inherits
    NotifHub --|> Hub : inherits
    FollowUpReminderService --|> BackgroundService : inherits
    AppointmentService --> IAppointmentRepository : uses
    AppointmentService --> INotificationService : uses
    AppointmentController --> IAppointmentService : uses
    AppointmentRepository --> ApplicationDbContext : uses
    AppointmentService --> Appointment : manages
```

---
