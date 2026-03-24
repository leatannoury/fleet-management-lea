# Fleet Management Platform

A robust, enterprise-ready Fleet Management Platform built with **ASP.NET Core 8** and **Domain-Driven Design (DDD)**. This system manages vehicles, driver assignments, and live trip tracking across two independent microservices communicating asynchronously via RabbitMQ.

## 🏛️ Project Architecture

The solution implements a strict Domain-Driven Design (DDD) approach with two bounded contexts, each owning its own PostgreSQL database.

*   **Fleet Service (`FleetService.API`)**: Owns the lifecycle of `Vehicle` and `Driver` entities. Maintains physical attributes, statuses, and driver-vehicle assignments.
*   **Trip Service (`TripService.API`)**: Owns `Trip` entities and `TripLog` records. Responsible for trip requests, assignment, live GPS tracking, and cost calculation.
*   **Shared Kernel (`SharedKernel`)**: Common base classes (`Entity`, `ValueObject`), cross-cutting concerns (Global Exception Middleware), and messaging contracts.

### Key Architectural Patterns
*   **Microservices Architecture**: Services are physically decoupled. Trip Service maintains a read-only cache of Driver/Vehicle availability rather than synchronously querying the Fleet Service database.
*   **Event-Driven Architecture**: Uses **RabbitMQ** for asynchronous communication (e.g., updating vehicle mileage when a trip completes).
*   **CQRS Principles**: Commands (state-changing) and Queries are separated at the application layered service level.
*   **Clean Architecture**: Dependencies point inward. Domain model has no external dependencies. Controllers delegate entirely to application services.

---

## 🛠️ Tech Stack
*   **Framework**: .NET 8 (ASP.NET Core Web API)
*   **Database**: PostgreSQL
*   **ORM**: Entity Framework Core
*   **Message Broker**: RabbitMQ
*   **Authentication**: Keycloak (JWT Bearer Auth)
*   **Real-time**: SignalR (WebSockets)
*   **Logging**: Serilog
*   **Testing**: xUnit, Moq
*   **Containerization**: Docker, Docker Compose

---

## 🚀 Setup & Run (Docker Deployment)

The entire platform is containerized and can be launched with a single command. 

### Prerequisites
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### 1. Start the System
Open a terminal in the solution root directory and run:
```bash
docker-compose up --build
```
This will spin up: PostgreSQL (w/ 2 databases), RabbitMQ, Keycloak, FleetService, and TripService. Wait approx 2-3 minutes for all services to become healthy.

### 2. Configure Keycloak (First Run Only)
Keycloak handles Authentication and Role-Based Access Control (RBAC).

1.  Navigate to http://localhost:8080
2.  Log in as administrator: `admin` / `admin`
3.  Create a realm named `fleet-platform`
4.  Create four Realm Roles: `Admin`, `FleetManager`, `Dispatcher`, `Driver`
5.  Create a client `fleet-client` (Access Type: confidential, or public for testing)
6.  Create users and assign them to the roles.

---

## 🛡️ API Documentation & Access

Both APIs self-document using **Swagger/OpenAPI**.

*   **Fleet Service Swagger**: http://localhost:5062/swagger
*   **Trip Service Swagger**: http://localhost:5089/swagger

### Authentication Flow (Postman/Swagger)
Once Keycloak is configured, retrieve a JWT token:

**Endpoint**: `POST http://localhost:8080/realms/fleet-platform/protocol/openid-connect/token`
**Body (x-www-form-urlencoded)**:
*   `grant_type`: `password`
*   `client_id`: `fleet-client`
*   `username`: *(your user)*
*   `password`: *(your password)*

Copy the `access_token` from the response and paste it into the **Authorize** lock icon at the top of the Swagger UI.

---

## 📡 Key Features & Interactions

### Fleet Service Features (Port 5062)
*   **Full CRUD**: Manage Vehicles and Drivers.
*   **State Machine Enforcement**: Vehicles move strictly through states: `Registered` → `Active` → `InMaintenance` → `Decommissioned`.
*   **Assignment Rules**: Ensures valid driver licenses, prevents assigning drivers to decommissioned vehicles or multiple vehicles.

### Trip Service Features (Port 5089)
*   **Trip Lifecycle**: `Requested` → `Assigned` → `InProgress` → `Completed` → `Invoiced`.
*   **Live Tracking (SignalR)**: Connect to `ws://localhost:5089/hubs/trip-tracking`. Drivers send live GPS pings, dispatchers subscribe to receive real-time map updates.
*   **Cross-Service Caching**: Subscribes to Fleet modifications via RabbitMQ, caching availability status in-memory to validate trip creation without querying the Fleet DB.

---

## ❤️ Health Monitoring

Both services expose built-in health checks that verify connectivity to PostgreSQL and RabbitMQ.
*   **Fleet Health**: http://localhost:5062/health
*   **Trip Health**: http://localhost:5089/health

---

## 🧪 Running Unit Tests
Unit tests validate core domain constraints, mocking layer separation, and API responses. 

To run tests locally outside of Docker:
```bash
dotnet test
```

*Coverage includes: State transition guards, Driver assignment rules, Decommission blocks, Trip status flows.*
