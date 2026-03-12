# Insurance Claims API

A .NET 9 REST API for managing insurance covers and claims, with a React frontend and async audit processing.

---

![Insurance](insurance.jpg)

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for MongoDB via Testcontainers)
- [Node.js 18+](https://nodejs.org/) (for the frontend)

---

## Running the API

- Start Docker Desktop
- `cd` into the repo root
- `dotnet run --project Claims/Claims.API.csproj`
- API available at `http://localhost:5000`
- Swagger UI at `http://localhost:5000/swagger`

## Running the Frontend

- `cd frontend`
- `npm install`
- `npm run dev`
- Open `http://localhost:3000`

## Running Tests

```bash
dotnet test Claims.Tests/Claims.Tests.csproj
```

---

## Azure Service Bus

The app supports Azure Service Bus for audit event processing as an alternative to the default in-memory queue.

- Configure the connection string in `appsettings.json` under `ServiceBus:ConnectionString`
- Queue name: `audit-events`
- **The Azure subscription is active until 23 March 2026** — after that date the Service Bus resource will be unavailable and the app will fall back to the in-memory queue

---

## Stack

- **API** — ASP.NET Core 9, MongoDB (Testcontainers), in-memory audit queue
- **Frontend** — React, TypeScript, Vite, Tailwind CSS
- **Tests** — xUnit, Moq
