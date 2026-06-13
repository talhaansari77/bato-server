# BATO Clinic API Backend

BATO Clinic API is the backend service for the BATO Clinic mobile app. It is built with the .NET ecosystem for enterprise-style backend practice.

## Backend Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core Web API |
| Language | C# |
| Runtime | .NET 9 |
| Database | MySQL Server |
| Database Runtime | Docker |
| ORM | Entity Framework Core |
| MySQL Provider | Pomelo.EntityFrameworkCore.MySql |
| Authentication | ASP.NET Core Identity |
| API Auth | JWT Bearer Authentication |
| Token Renewal | Refresh Tokens with rotation |
| API Docs | Swagger / Swashbuckle |
| API Testing | Postman |
| Version Control | Git + GitHub |

## Main Purpose

The backend supports a clinic/mobile app where:

- Patients can browse services, book appointments, manage profiles, view treatment plans, view medical records, add progress photo URLs, and receive notifications.
- Doctors can view appointments, complete/no-show appointments, manage their profiles, create treatment plans, sessions, and medical records.
- Admins can manage branches, services, doctors, appointments, patients, notifications, and dashboard stats.

## Project Structure

```txt
BatoClinic.Api/
  Configuration/
    JwtSettings.cs

  Controllers/
    AdminController.cs
    AppointmentsController.cs
    AuthController.cs
    BranchesController.cs
    DoctorsController.cs
    MedicalRecordsController.cs
    NotificationsController.cs
    PatientsController.cs
    ProgressPhotosController.cs
    ServiceCategoriesController.cs
    ServicesController.cs
    TreatmentPlansController.cs
    TreatmentSessionsController.cs

  Data/
    AppDbContext.cs
    DbSeeder.cs

  DTOs/
    Admin/
    Appointments/
    Auth/
    Branches/
    Doctors/
    MedicalRecords/
    Notifications/
    Patients/
    ProgressPhotos/
    ServiceCategories/
    Services/
    TreatmentPlans/
    TreatmentSessions/

  Entities/
    ApplicationUser.cs
    Appointment.cs
    Branch.cs
    ClinicService.cs
    DoctorBranch.cs
    DoctorProfile.cs
    DoctorService.cs
    MedicalRecord.cs
    Notification.cs
    PatientProfile.cs
    ProgressPhoto.cs
    RefreshToken.cs
    ServiceCategory.cs
    TreatmentPlan.cs
    TreatmentSession.cs

  Enums/
    AppointmentStatus.cs
    PaymentMethod.cs
    PaymentStatus.cs

  Helpers/
    ApiErrorResponse.cs
    ApiResponse.cs
    NotificationHelper.cs

  Interfaces/
    ITokenService.cs

  Middleware/
    ExceptionMiddleware.cs

  Services/
    TokenService.cs

  docker-compose.yml
  Program.cs
  appsettings.json
```

## Docker Setup

Docker is currently used for **MySQL only**. The ASP.NET Core API runs locally with `dotnet run`.

```txt
.NET API = runs locally
MySQL = runs inside Docker
```

Start MySQL:

```bash
docker compose up -d
```

Check containers:

```bash
docker ps
```

Expected container:

```txt
bato-clinic-mysql
```

Docker MySQL config:

```yaml
services:
  bato-mysql:
    image: mysql:8.4
    container_name: bato-clinic-mysql
    restart: unless-stopped
    environment:
      MYSQL_ROOT_PASSWORD: bato_root_password
      MYSQL_DATABASE: bato_clinic_db
      MYSQL_USER: bato_user
      MYSQL_PASSWORD: bato_password
    ports:
      - "3307:3306"
    volumes:
      - bato_mysql_data:/var/lib/mysql

volumes:
  bato_mysql_data:
```

## Connection String

In `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3307;Database=bato_clinic_db;User=bato_user;Password=bato_password;"
}
```

## JWT Settings

In `appsettings.json`:

```json
"Jwt": {
  "Issuer": "BatoClinic",
  "Audience": "BatoClinicMobile",
  "Key": "BATO_CLINIC_DEV_SECRET_KEY_SHOULD_BE_LONG_FOR_JWT_SIGNING_123456789"
}
```

For production, move secrets and database credentials to environment variables or a secret manager.

## Important Dependencies

```txt
Pomelo.EntityFrameworkCore.MySql
Microsoft.EntityFrameworkCore.Design
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.AspNetCore.Authentication.JwtBearer
Swashbuckle.AspNetCore
```

| Package | Purpose |
|---|---|
| Pomelo.EntityFrameworkCore.MySql | Connects EF Core to MySQL |
| Microsoft.EntityFrameworkCore.Design | Enables EF migration commands |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | Adds Identity users, roles, claims, logins, tokens |
| Microsoft.AspNetCore.Authentication.JwtBearer | Validates JWT Bearer tokens |
| Swashbuckle.AspNetCore | Adds Swagger UI/API docs |

## Entity Framework Commands

Add migration:

```bash
dotnet ef migrations add MigrationName
```

Apply migration:

```bash
dotnet ef database update
```

Build:

```bash
dotnet build
```

Run:

```bash
dotnet run
```

Swagger:

```txt
http://localhost:5243/swagger
```

## Seed Data

`DbSeeder.cs` runs automatically when the backend starts.

Seeded roles:

```txt
Patient
Doctor
Admin
```

Seeded users:

```txt
Admin:  admin@batoclinic.com   Password123!
Doctor: doctor@batoclinic.com  Password123!
Patient: patient@batoclinic.com Password123!
```

Seeded data:

```txt
Branch:
- BATO Clinic - Main Branch

Service Categories:
- Skin Treatments
- Hair Treatments
- Face Treatments

Services:
- HydraFacial Treatment
- Hair Nourishment
- Botox Consultation

Doctor:
- Dr. Sarah Khan

Assignments:
- Dr. Sarah Khan assigned to main branch
- Dr. Sarah Khan assigned to seeded services
```

## Authentication Design

```txt
ASP.NET Core Identity = users, passwords, roles
JWT Bearer = mobile API access token
Refresh Tokens = long-lived session renewal
```

Protected APIs use:

```txt
Authorization: Bearer YOUR_ACCESS_TOKEN
```

Refresh token rotation is implemented:

```txt
Old refresh token is revoked.
New access token is created.
New refresh token is created.
```

Logout revokes a refresh token.

## Roles

Current roles:

```txt
Patient
Doctor
Admin
```

Nurse role is planned later.

Common authorization patterns:

```csharp
[Authorize]
[Authorize(Roles = "Admin")]
[Authorize(Roles = "Doctor,Admin")]
```

## Core Business Rules

### Booking Flow

```txt
Service → Branch → Doctor → Date/Time → Payment Method → Appointment Status
```

### Pay-at-Clinic Rule

If payment method is `PayAtClinic`:

```txt
AppointmentStatus = PendingAdminApproval
PaymentStatus = PayAtClinic
```

Admin must approve before the appointment becomes confirmed.

### Online Payment Rule

Online payment is prepared but not integrated yet.

For now:

```txt
AppointmentStatus = PendingPayment
PaymentStatus = Unpaid
```

### Appointment Conflict Rule

A doctor cannot have overlapping active appointments.

Cancelled, rejected, and refunded appointments are ignored in conflict checks.

### Soft Delete Rule

Branches and services are not permanently deleted. They are deactivated with:

```txt
IsActive = false
```

This keeps old appointment history safe.

## Enums

### AppointmentStatus

```txt
PendingPayment
PendingAdminApproval
Confirmed
Rejected
Completed
Cancelled
Rescheduled
NoShow
Refunded
```

### PaymentStatus

```txt
Unpaid
Paid
PartiallyPaid
Refunded
Failed
PayAtClinic
```

### PaymentMethod

```txt
Online
PayAtClinic
```

Enums are stored as strings in MySQL for readability.

## API Endpoints

### Auth APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register user |
| POST | `/api/auth/login` | Public | Login and return access + refresh token |
| GET | `/api/auth/me` | Authenticated | Get logged-in user |
| POST | `/api/auth/refresh` | Public | Rotate refresh token and return new tokens |
| POST | `/api/auth/logout` | Public | Revoke refresh token |

### Branch APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/branches` | Public | Get branches |
| GET | `/api/branches/{id}` | Public | Get branch by id |
| POST | `/api/branches` | Admin | Create branch |
| PATCH | `/api/branches/{id}` | Admin | Update branch |
| DELETE | `/api/branches/{id}` | Admin | Deactivate branch |

### Service Category APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/service-categories` | Public | Get active categories |
| POST | `/api/service-categories` | Admin | Create category |

### Service APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/services` | Public | Get active services |
| GET | `/api/services/{id}` | Public | Get service by id |
| POST | `/api/services` | Admin | Create service |
| PATCH | `/api/services/{id}` | Admin | Update service |
| DELETE | `/api/services/{id}` | Admin | Deactivate service |

### Doctor APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/doctors` | Public | Get doctors |
| GET | `/api/doctors/{id}` | Public | Get doctor by id |
| GET | `/api/doctors/available?serviceId=&branchId=` | Public | Get doctors by service + branch |
| GET | `/api/doctors/me` | Doctor | Get own profile |
| PATCH | `/api/doctors/me` | Doctor | Update own profile |
| PATCH | `/api/doctors/{id}` | Admin | Update doctor profile |
| POST | `/api/doctors/{doctorId}/branches` | Admin | Assign branches |
| POST | `/api/doctors/{doctorId}/services` | Admin | Assign services |

### Patient APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/patients/me` | Patient | Get own profile |
| PATCH | `/api/patients/me` | Patient | Update own profile |
| GET | `/api/patients` | Admin | Get all patients |
| GET | `/api/patients/{id}` | Admin | Get patient by id |

### Appointment APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/appointments/available-slots?doctorProfileId=&clinicServiceId=&branchId=&date=` | Public | Get available slots |
| POST | `/api/appointments` | Patient | Create appointment |
| GET | `/api/appointments/my` | Patient | Get own appointments |
| GET | `/api/appointments` | Admin | Get all appointments |
| GET | `/api/appointments/{id}` | Patient/Admin | Get appointment by id |
| GET | `/api/appointments/doctor/my` | Doctor | Get doctor appointments |
| PATCH | `/api/appointments/{id}/approve` | Admin | Approve appointment |
| PATCH | `/api/appointments/{id}/reject` | Admin | Reject appointment |
| PATCH | `/api/appointments/{id}/cancel` | Patient/Admin | Cancel appointment |
| PATCH | `/api/appointments/{id}/reschedule` | Patient/Admin | Reschedule appointment |
| PATCH | `/api/appointments/{id}/complete` | Doctor/Admin | Complete appointment |
| PATCH | `/api/appointments/{id}/no-show` | Doctor/Admin | Mark no-show |

### Treatment Plan APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/treatment-plans` | Doctor/Admin | Create treatment plan |
| GET | `/api/treatment-plans/my` | Patient | Get own plans |
| GET | `/api/treatment-plans/patient/{patientProfileId}` | Doctor/Admin | Get patient plans |
| GET | `/api/treatment-plans/{id}` | Patient/Doctor/Admin | Get plan by id |

### Treatment Session APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/treatment-plans/{planId}/sessions` | Doctor/Admin | Add session |
| GET | `/api/treatment-plans/{planId}/sessions` | Patient/Doctor/Admin | Get plan sessions |
| PATCH | `/api/treatment-sessions/{id}` | Doctor/Admin | Update session |
| DELETE | `/api/treatment-sessions/{id}` | Doctor/Admin | Delete session |

### Medical Record APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/medical-records` | Doctor/Admin | Create medical record |
| GET | `/api/medical-records/my` | Patient | Get own records |
| GET | `/api/medical-records/patient/{patientProfileId}` | Doctor/Admin | Get patient records |
| GET | `/api/medical-records/{id}` | Patient/Doctor/Admin | Get record by id |
| PATCH | `/api/medical-records/{id}` | Doctor/Admin | Update record |

### Progress Photo APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/progress-photos` | Patient | Add progress photo URL |
| GET | `/api/progress-photos/my` | Patient | Get own photos |
| GET | `/api/progress-photos/patient/{patientProfileId}` | Doctor/Admin | Get patient photos |
| DELETE | `/api/progress-photos/{id}` | Patient/Admin | Delete photo |

### Notification APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/notifications` | Admin | Create notification |
| GET | `/api/notifications/my` | Authenticated | Get own notifications |
| PATCH | `/api/notifications/{id}/read` | Authenticated | Mark one as read |
| PATCH | `/api/notifications/read-all` | Authenticated | Mark all as read |
| DELETE | `/api/notifications/{id}` | Authenticated | Delete own notification |

### Admin APIs

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/admin/dashboard-summary` | Admin | Dashboard stats |

Dashboard includes:

```txt
Total patients
Total doctors
Total branches
Total services
Total appointments
Pending approvals
Today appointments
Completed appointments
Cancelled appointments
```

## Automatic Notifications

`NotificationHelper.cs` creates notifications for important appointment actions.

Automatic notifications currently happen when:

```txt
Appointment approved
Appointment rejected
Appointment cancelled
Appointment rescheduled
```

## Error Handling

The backend includes:

```txt
ApiResponse<T>
ApiErrorResponse
ExceptionMiddleware
```

Unexpected server errors return:

```json
{
  "success": false,
  "message": "An unexpected server error occurred",
  "errors": []
}
```

## CORS

CORS policy name:

```txt
BatoCorsPolicy
```

Allowed local origins:

```txt
http://localhost:3000
http://localhost:5173
http://localhost:8081
http://127.0.0.1:3000
http://127.0.0.1:5173
http://127.0.0.1:8081
```

## Swagger

Swagger URL:

```txt
http://localhost:5243/swagger
```

JWT support is configured.

To test protected endpoints:

```txt
1. Login with /api/auth/login
2. Copy token
3. Click Authorize
4. Enter: Bearer YOUR_TOKEN
5. Test protected endpoint
```

## Postman Testing

For protected endpoints:

```txt
Authorization: Bearer YOUR_TOKEN
```

For JSON bodies:

```txt
Content-Type: application/json
```

## Common Commands

```bash
# Start MySQL
docker compose up -d

# Stop MySQL
docker compose down

# Build API
dotnet build

# Run API
dotnet run

# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Git commit
git status
git add .
git commit -m "Your commit message"
git push
```

## Completed Backend Modules

```txt
Docker MySQL setup
ASP.NET Core Web API setup
EF Core + MySQL connection
ASP.NET Core Identity setup
JWT authentication
Refresh token authentication
Swagger + JWT support
CORS setup
Global exception middleware
Database seeding
Branch APIs
Service category APIs
Service APIs
Doctor profile APIs
Doctor branch/service assignment APIs
Patient profile APIs
Appointment booking APIs
Appointment approval/rejection/cancel/reschedule APIs
Doctor appointment APIs
Treatment plan APIs
Treatment session APIs
Medical record APIs
Progress photo APIs
Notification APIs
Automatic appointment notifications
Admin dashboard summary API
```

## Planned Later

```txt
Real payment integration
Phone OTP auth
Email verification
Forgot/reset password
File upload for progress photos
Cloud storage integration
Firebase push notifications
Doctor availability schedule tables
Nurse role and APIs
Audit logs
Advanced reporting and revenue analytics
Rate limiting
Production environment variables
Dockerize ASP.NET Core API
Deployment setup
```

## Development Notes

- Keep GitHub updated after every stable feature.
- Use Postman for endpoint testing.
- Use Swagger for quick API discovery/testing.
- Use Docker for MySQL.
- Keep controller code simple while learning .NET.
- Move repeated logic into services later when project grows.
- Use DTOs instead of returning raw entities.
- Use soft delete for important clinic data when history may be needed.
- Keep medical and patient data protected by role-based authorization.
