# Claims Insurance API

## Overview

Claims Insurance API is a production-ready ASP.NET Core Web API that
manages:

-   Claims
-   Covers
-   Audit events

The application follows Clean Architecture principles with clear
separation of concerns:

-   Domain Layer
-   Application Layer (Services & Interfaces)
-   Infrastructure Layer
-   API Layer

It integrates Azure Service Bus for asynchronous audit processing and
background message handling.

------------------------------------------------------------------------

## Architecture

### Design Principles

-   Controllers contain no business logic
-   Services encapsulate business rules
-   Repository pattern abstracts data access
-   Background services handle asynchronous processing
-   Enum-based domain modeling prevents string-based bugs
-   Dependency Injection ensures loose coupling and testability

------------------------------------------------------------------------

## Technology Stack

-   .NET 9+
-   ASP.NET Core Web API
-   Entity Framework Core
-   MongoDB (Audit storage if applicable)
-   Azure Service Bus
-   Docker

------------------------------------------------------------------------

## Prerequisites

### 1. Install Visual Studio

Include: - ASP.NET Core workload - .NET development tools

### 2. Install .NET SDK

Verify installation:

    dotnet --version

If not recognized, add to PATH: C:`\Program `{=tex}Files`\dotnet`{=tex}\

### 3. Install Docker Desktop

Used for containerized services (e.g., MongoDB).

------------------------------------------------------------------------

## Installation & Setup

### Restore Dependencies

    dotnet restore

### Run Migrations (Audit Context)

    Add-Migration InitialCreate -Context AuditContext
    Update-Database -Context AuditContext

### Run Application

    dotnet run

Swagger available at:

    https://localhost:{port}/swagger

------------------------------------------------------------------------

## Premium Computation Logic

### Base Rate

1250 per day

### Type Multipliers

-   Yacht: +10%
-   PassengerShip: +20%
-   Tanker: +50%
-   Other: +30%

### Progressive Discounts

-   First 30 days: no discount
-   Next 150 days:
    -   Yacht: -5%
    -   Others: -2%
-   Remaining days:
    -   Yacht: -8% total discount
    -   Others: -3% total discount

### Example Response

```json
{
    "id": "5d1b4ff9-87d0-46f2-8692-1e934ba38d55",
    "startDate": "2026-02-21T00:59:30.573Z",
    "endDate": "2026-03-21T00:59:30.573Z",
    "type": "Yacht",
    "premium": 38500
}
```

------------------------------------------------------------------------

## Azure Service Bus Integration

### NuGet Package

    dotnet add package Azure.Messaging.ServiceBus

### Azure Resources

Resource Group: claims-app-rg

Service Bus Namespace: claimsservicebus

Queue: audit-events

### Background Worker

A hosted background service listens to the `audit-events` queue and:

-   Processes audit messages
-   Persists audit records
-   Logs results
-   Ensures reliable asynchronous processing

------------------------------------------------------------------------

## Refactoring & Improvements

-   Removed direct DbContext usage from controllers
-   Introduced service layer abstractions
-   Implemented repository pattern
-   Added XML documentation for public APIs
-   Implemented enum-safe premium calculation
-   Improved dependency injection configuration

------------------------------------------------------------------------

## Production Considerations

Recommended future improvements:

-   API Management Layer
-   Health Checks
-   Structured Logging (Serilog)
-   Retry Policies for Service Bus
-   CI/CD Pipeline
-   Containerized Deployment
-   Application Insights integration
-   Rate limiting and authentication

------------------------------------------------------------------------

## Running with Docker (Optional)

Build image:

    docker build -t claims-api .

Run container:

    docker run -p 5000:80 claims-api

------------------------------------------------------------------------

## 🚀 CI Pipeline

A GitHub Actions workflow runs on every push to main:

-   Build (.NET 9)

-   Run tests

-   Publish artifacts

All changes must pass the pipeline before being considered stable.

------------------------------------------------------------------------
