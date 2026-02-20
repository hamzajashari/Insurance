# 📘 Insurance Claims API

## Architecture Documentation

------------------------------------------------------------------------

## 📌 Overview

This project implements an **Insurance Claims Management API** using a
clean, layered architecture.

The codebase was refactored to:

-   Reduce controller responsibilities
-   Introduce proper layering
-   Follow **SOLID principles**
-   Improve maintainability and testability
-   Add XML documentation

------------------------------------------------------------------------

# 🏗 Architecture

The solution follows a **Layered Architecture**:

    Claims.API
    Claims.Application
    Claims.Infrastructure
    Claims.Domain

------------------------------------------------------------------------

## 🔹 Claims.API

### Responsibility

Handles HTTP requests and responses.

### Contains

-   Controllers
-   Swagger configuration
-   Dependency Injection setup

### Rules

-   No business logic
-   No database logic
-   Depends only on Application layer

------------------------------------------------------------------------

## 🔹 Claims.Application

### Responsibility

Contains business logic and orchestration.

### Contains

-   Service interfaces (IClaimService, ICoverService, IAuditService)
-   Service implementations
-   Premium calculation logic
-   Validation rules

### Rules

-   Depends only on Claims.Domain
-   Uses abstractions for Infrastructure communication

------------------------------------------------------------------------

## 🔹 Claims.Infrastructure

### Responsibility

Handles persistence and external concerns.

### Contains

-   MongoDB contexts
-   SQL Server Audit context
-   Repository implementations
-   Audit implementation

### Rules

-   Implements interfaces defined in Application layer
-   Does not contain business logic

------------------------------------------------------------------------

## 🔹 Claims.Domain

### Responsibility

Core business entities and domain models.

### Contains

-   Entities (Claim, Cover)
-   Enums
-   Domain-related structures

### Rules

-   No dependencies on other layers
-   Pure business objects

------------------------------------------------------------------------

# 🧠 SOLID Principles Applied

-   **Single Responsibility Principle**\
    Each layer has a clearly defined responsibility.

-   **Open/Closed Principle**\
    Business logic can be extended without modifying controllers.

-   **Liskov Substitution Principle**\
    Services depend on abstractions and can be substituted.

-   **Interface Segregation Principle**\
    Small, focused service interfaces are used.

-   **Dependency Inversion Principle**\
    High-level modules depend on abstractions, not concrete
    implementations.

------------------------------------------------------------------------

# 🗄 Persistence

-   MongoDB is used for Claims and Covers.
-   SQL Server is used for auditing.
-   EF Core is used as ORM.
-   Testcontainers are used for testing environments.

------------------------------------------------------------------------

# ✅ Result

The application is now:

-   Structured
-   Maintainable
-   Testable
-   Layered
-   SOLID-compliant
