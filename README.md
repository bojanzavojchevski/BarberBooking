# BarberBooking

BarberBooking is a web application for managing a barber shop.

The application allows users to view the barber shop, its available services, and its barbers. The owner can authenticate and manage shop data, services, and barbers through the backend API.

The project is built as a three-service architecture consisting of:

- React frontend
- ASP.NET Core Web API backend
- PostgreSQL database

The project is being extended with Docker, Docker Compose, GitHub Actions, Docker Hub, and Kubernetes as part of the Continuous Integration and Delivery course project.

## Architecture

BarberBooking uses a three-service architecture:

```text
React Frontend
      |
      v
ASP.NET Core Web API
      |
      v
PostgreSQL Database
```

The frontend is responsible for the user interface and communicates with the backend through HTTP API requests.

The ASP.NET Core Web API contains the application and business logic and communicates with PostgreSQL for data persistence.

PostgreSQL stores the application's persistent data, including shop, service, barber, authentication, and related information.

## Technologies

### Backend

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity
- JWT authentication
- PostgreSQL
- Swagger / OpenAPI

### Frontend

- React
- Vite
- JavaScript
- HTML
- CSS

### Testing

- xUnit
- ASP.NET Core integration testing
- Testcontainers
- PostgreSQL test containers

### DevOps

- Git
- GitHub
- GitHub Actions
- Docker
- Docker Compose
- Docker Hub
- Kubernetes

## Project Structure

```text
BarberBooking/
|
├── BarberBooking.Domain/
├── BarberBooking.Application/
├── BarberBooking.Infrastructure/
├── BarberBooking.WebApi/
├── BarberBooking.IntegrationTests/
├── barberbooking-frontend/
├── docs/
├── .github/
├── BarberBooking.sln
└── README.md
```

The backend follows a layered architecture:

- `BarberBooking.Domain` contains the core domain entities and business rules.
- `BarberBooking.Application` contains application use cases and abstractions.
- `BarberBooking.Infrastructure` contains database, authentication, and infrastructure implementations.
- `BarberBooking.WebApi` exposes the HTTP API.
- `BarberBooking.IntegrationTests` contains integration tests for the backend.
- `barberbooking-frontend` contains the React frontend.

## Running the Application Locally

### Prerequisites

Install the following tools:

- .NET 9 SDK
- Node.js and npm
- PostgreSQL
- Git

### 1. Configure PostgreSQL

Create a PostgreSQL database for BarberBooking.

The local database connection string should be stored using .NET User Secrets instead of being committed to the repository.

From the `BarberBooking.WebApi` directory, configure:

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=BarberBookingDb;Username=postgres;Password=<YOUR_PASSWORD>"
```

### 2. Configure Authentication Secrets

The JWT signing key and refresh-token pepper must also be stored using .NET User Secrets.

```powershell
dotnet user-secrets set "Jwt:SigningKey" "<YOUR_JWT_SIGNING_KEY>"
dotnet user-secrets set "RefreshTokens:Pepper" "<YOUR_REFRESH_TOKEN_PEPPER>"
```

Real secrets must never be committed to Git.

### 3. Restore and Build the Backend

From the repository root:

```powershell
dotnet restore
dotnet build
```

### 4. Run the Tests

```powershell
dotnet test
```

Integration tests use Testcontainers, so Docker must be running when executing them.

### 5. Run the Backend

```powershell
dotnet run --project BarberBooking.WebApi
```

During local development, the API is available at:

```text
https://localhost:7099
```

Swagger can be opened at:

```text
https://localhost:7099/swagger
```

### 6. Run the Frontend

Open another terminal:

```powershell
cd barberbooking-frontend
npm install
npm run dev
```

The frontend is available at:

```text
http://localhost:5173
```

During development, Vite proxies `/api` requests from the frontend to the ASP.NET Core backend.

## Current Functionality

The application currently supports:

- Viewing barber shop information
- Viewing available services
- Viewing barbers
- Owner authentication using JWT
- Owner shop management
- Owner service management
- Owner barber management
- Refresh-token authentication flow
- Backend health-check endpoints
- Integration testing with PostgreSQL Testcontainers

## CI/CD and Containerization

The project will include:

- Docker images for the frontend and backend
- Docker Compose orchestration for frontend, backend, and PostgreSQL
- GitHub Actions continuous integration
- Automatic Docker image builds
- Docker Hub image publishing
- Kubernetes deployment

Detailed Docker and Kubernetes instructions will be added after those parts of the project are implemented.

## Security

Sensitive configuration is not stored in tracked application configuration files.

Local secrets such as the following are stored using .NET User Secrets:

- PostgreSQL credentials
- JWT signing key
- Refresh-token pepper

Environment variables and Kubernetes Secrets will be used when the application is containerized and deployed.

## Documentation

Development notes are available in the `docs/` directory.