# BarberBooking

BarberBooking is a web application for managing a barber shop.

The application allows users to view the barber shop, its available services, and its barbers. The owner can authenticate and manage shop data, services, and barbers through the backend API.

The project uses a three-service architecture consisting of:

- React frontend
- ASP.NET Core Web API backend
- PostgreSQL database

The application is containerized with Docker, orchestrated locally with Docker Compose, built and tested through GitHub Actions, published to Docker Hub, and deployed to Kubernetes.

---

## Architecture

### Application Architecture

```text
React Frontend
      |
      | HTTP / API requests
      v
ASP.NET Core Web API
      |
      | Entity Framework Core
      v
PostgreSQL Database
```

The frontend provides the user interface and communicates with the backend through HTTP API requests.

The ASP.NET Core Web API contains the application and business logic and communicates with PostgreSQL for persistent storage.

PostgreSQL stores application data including shop, service, barber, authentication, and related information.

---

## Technologies

### Backend

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity
- JWT authentication
- PostgreSQL
- Swagger / OpenAPI
- Serilog
- ASP.NET Core Health Checks

### Frontend

- React
- Vite
- JavaScript
- HTML
- CSS
- Nginx

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
- ingress-nginx

---

## Project Structure

```text
BarberBooking/
│
├── BarberBooking.Domain/
├── BarberBooking.Application/
├── BarberBooking.Infrastructure/
├── BarberBooking.WebApi/
├── BarberBooking.IntegrationTests/
│
├── barberbooking-frontend/
│
├── docs/
│
├── .github/
│   └── workflows/
│       └── ci.yml
│
├── k8s/
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secret.example.yaml
│   ├── postgres-statefulset.yaml
│   ├── postgres-service.yaml
│   ├── api-deployment.yaml
│   ├── api-service.yaml
│   ├── frontend-deployment.yaml
│   ├── frontend-service.yaml
│   ├── ingress-nginx-controller.yaml
│   └── ingress.yaml
│
├── docker-compose.yml
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

---

# Running the Application Locally

## Prerequisites

Install:

- .NET 9 SDK
- Node.js and npm
- PostgreSQL
- Git
- Docker Desktop

---

## 1. Configure PostgreSQL

Create a PostgreSQL database for BarberBooking.

The local database connection string should be stored using .NET User Secrets instead of being committed to the repository.

From the `BarberBooking.WebApi` directory:

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=BarberBookingDb;Username=postgres;Password=<YOUR_PASSWORD>"
```

---

## 2. Configure Authentication Secrets

The JWT signing key and refresh-token pepper should also be stored using .NET User Secrets.

```powershell
dotnet user-secrets set "Jwt:SigningKey" "<YOUR_JWT_SIGNING_KEY>"
dotnet user-secrets set "RefreshTokens:Pepper" "<YOUR_REFRESH_TOKEN_PEPPER>"
```

Real secrets must never be committed to Git.

---

## 3. Restore and Build the Backend

From the repository root:

```powershell
dotnet restore
dotnet build
```

---

## 4. Run Tests

```powershell
dotnet test
```

Integration tests use Testcontainers, so Docker must be running when executing them.

---

## 5. Run the Backend

```powershell
dotnet run --project BarberBooking.WebApi
```

During local development, the API is available at:

```text
https://localhost:7099
```

Swagger:

```text
https://localhost:7099/swagger
```

---

## 6. Run the Frontend

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

During development, Vite proxies `/api` requests to the ASP.NET Core backend.

---

# Docker

Both application components have their own Docker images.

## Backend Image

The backend uses a multi-stage Dockerfile that:

1. Uses the .NET SDK image to restore and build the application.
2. Publishes the ASP.NET Core application.
3. Copies the published output into the smaller ASP.NET Core runtime image.
4. Runs the API on port `8080`.

Docker Hub image:

```text
freak4y/barberbooking-api
```

## Frontend Image

The frontend Dockerfile:

1. Uses Node.js to install dependencies.
2. Builds the React/Vite production application.
3. Copies the production build into an Nginx image.
4. Serves the frontend through Nginx on port `80`.

Docker Hub image:

```text
freak4y/barberbooking-frontend
```

The CI pipeline publishes both `latest` and commit-specific image tags.

---

# Docker Compose

Docker Compose starts the complete application as three containers:

```text
frontend
   |
   v
backend
   |
   v
postgres
```

The services communicate through the Docker Compose network using service names rather than `localhost`.

The backend connects to PostgreSQL using:

```text
postgres:5432
```

and the frontend communicates with the backend through `/api`.

PostgreSQL uses a Docker volume so database data survives container recreation.

## Configure Docker Compose Secrets

Create a local file named:

```text
.env.compose
```

Example:

```env
POSTGRES_PASSWORD=<YOUR_POSTGRES_PASSWORD>
JWT_SIGNING_KEY=<YOUR_JWT_SIGNING_KEY>
REFRESH_TOKEN_PEPPER=<YOUR_REFRESH_TOKEN_PEPPER>
```

`.env.compose` is ignored by Git and must not be committed.

## Start the Application

From the repository root:

```powershell
docker compose --env-file .env.compose up --build
```

The services are available at:

```text
Frontend:   http://localhost:3000
Backend:    http://localhost:8080
PostgreSQL: localhost:5433
```

Stop the application with:

```powershell
docker compose down
```

The PostgreSQL volume remains unless it is explicitly deleted.

---

# Continuous Integration with GitHub Actions

The project contains a GitHub Actions CI workflow.

A push to the repository triggers the pipeline.

```text
git push
   |
   v
GitHub Actions
   |
   v
Restore dependencies
   |
   v
Build
   |
   v
Run tests
   |
   v
Check formatting
   |
   v
Build Docker images
   |
   v
Push images to Docker Hub
```

The workflow contains separate build/test and Docker jobs.

Docker Hub authentication is performed using GitHub Secrets. Docker Hub credentials are never hardcoded in the workflow file.

The pipeline publishes:

```text
freak4y/barberbooking-api:latest
freak4y/barberbooking-api:<commit-sha>

freak4y/barberbooking-frontend:latest
freak4y/barberbooking-frontend:<commit-sha>
```

This ensures every successful pipeline can produce versioned application images.

---

# Kubernetes

The complete application can also run inside Kubernetes.

The project was tested using Docker Desktop Kubernetes with the kind provisioning method.

All application resources are deployed inside the:

```text
barber-booking
```

namespace.

## Kubernetes Architecture

```text
                         Browser
                            |
                            v
                    Ingress Controller
                            |
                     BarberBooking Ingress
                      /             \ /api
                     /               \
                    v                 v
             Frontend Service    Backend Service
                    |                 |
                    v                 v
             Frontend Pod        Backend Pod
              React/Nginx        ASP.NET Core
                                      |
                                      v
                              PostgreSQL Service
                                      |
                                      v
                              PostgreSQL StatefulSet
                                      |
                                      v
                                  postgres-0
                                      |
                                      v
                           PersistentVolumeClaim
```

Ingress provides one HTTP entry point for the application:

```text
/      -> frontend Service
/api   -> backend Service
```

---

## Kubernetes Namespace

All BarberBooking resources are isolated in:

```text
barber-booking
```

Create it with:

```powershell
kubectl apply -f .\k8s\namespace.yaml
```

---

## ConfigMap

`k8s/configmap.yaml` contains non-sensitive application configuration, including:

- ASP.NET Core environment
- backend URL configuration
- PostgreSQL host, port, and database name
- JWT issuer and audience
- access-token lifetime
- refresh-token lifetime

Apply it with:

```powershell
kubectl apply -f .\k8s\configmap.yaml
```

---

## Kubernetes Secrets

Real Kubernetes credentials are stored locally in:

```text
k8s/secret.yaml
```

This file is ignored by Git.

The repository instead contains:

```text
k8s/secret.example.yaml
```

Copy the example file:

```powershell
Copy-Item .\k8s\secret.example.yaml .\k8s\secret.yaml
```

Then replace the placeholder values in `secret.yaml` with your own local values.

Apply the Secret:

```powershell
kubectl apply -f .\k8s\secret.yaml
```

The Secret contains sensitive values such as:

- PostgreSQL username
- PostgreSQL password
- JWT signing key
- refresh-token pepper

---

## PostgreSQL

PostgreSQL runs as a Kubernetes StatefulSet because it is a stateful workload.

The StatefulSet uses a PersistentVolumeClaim so database files survive Pod recreation.

Deploy PostgreSQL:

```powershell
kubectl apply -f .\k8s\postgres-service.yaml
kubectl apply -f .\k8s\postgres-statefulset.yaml
```

The PostgreSQL Service provides the internal DNS name:

```text
postgres
```

The backend therefore connects to:

```text
postgres:5432
```

---

## Backend Deployment

Deploy the ASP.NET Core API:

```powershell
kubectl apply -f .\k8s\api-deployment.yaml
kubectl apply -f .\k8s\api-service.yaml
```

The Deployment uses:

```text
freak4y/barberbooking-api:latest
```

The API runs on port `8080`.

It also contains Kubernetes health probes:

```text
Liveness:  /health/live
Readiness: /health/ready
```

The backend Service is named:

```text
backend
```

---

## Frontend Deployment

Deploy the React frontend:

```powershell
kubectl apply -f .\k8s\frontend-deployment.yaml
kubectl apply -f .\k8s\frontend-service.yaml
```

The Deployment uses:

```text
freak4y/barberbooking-frontend:latest
```

The frontend container runs Nginx on port `80`.

---

## Ingress

An ingress-nginx controller is used to implement Kubernetes Ingress routing.

Install the controller:

```powershell
kubectl apply -f .\k8s\ingress-nginx-controller.yaml
```

Verify it:

```powershell
kubectl get pods -n ingress-nginx
kubectl get ingressclass
```

Then deploy the BarberBooking Ingress:

```powershell
kubectl apply -f .\k8s\ingress.yaml
```

The routes are:

```text
/      -> frontend:80
/api   -> backend:8080
```

---

# Accessing the Application Through Kubernetes

When using the local Docker Desktop Kubernetes cluster, the ingress-nginx LoadBalancer may remain in a pending state because no cloud load balancer exists.

For local access, forward the Ingress Controller:

```powershell
kubectl port-forward service/ingress-nginx-controller 8082:80 -n ingress-nginx
```

Then open:

```text
http://localhost:8082
```

Requests to `/` are sent to the frontend, while requests beginning with `/api` are routed directly to the backend.

---

# Kubernetes Verification

Check all BarberBooking resources:

```powershell
kubectl get all -n barber-booking
```

Check the Ingress:

```powershell
kubectl get ingress -n barber-booking
```

Check persistent storage:

```powershell
kubectl get pvc -n barber-booking
```

Expected application workloads:

```text
backend      Running
frontend     Running
postgres-0   Running
```

Expected higher-level resources:

```text
Backend Deployment       Ready
Frontend Deployment      Ready
PostgreSQL StatefulSet   Ready
PostgreSQL PVC           Bound
BarberBooking Ingress    Present
```

Kubernetes automatically restores the desired state if an application Pod is deleted.

For example, deleting the backend Pod causes the Deployment to create a replacement:

```powershell
kubectl delete pod -l app=backend -n barber-booking
```

PostgreSQL data remains available after its Pod is recreated because the database files are stored through the PersistentVolumeClaim.

---

# Health Checks

The backend exposes:

```text
/health/live
/health/ready
```

`/health/live` is used by the Kubernetes liveness probe to determine whether the application process is alive.

`/health/ready` is used by the readiness probe to determine whether the application is ready to receive traffic.

---

# Current Functionality

The application supports:

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

---

# Security

Sensitive configuration is not stored in tracked application configuration files.

Different environments use different mechanisms for sensitive values:

```text
Local development
    -> .NET User Secrets

Docker Compose
    -> ignored local environment file

GitHub Actions
    -> GitHub Secrets

Kubernetes
    -> ignored secret.yaml / Kubernetes Secret
```

The repository contains `secret.example.yaml` only to document which Kubernetes Secret keys must be configured.

Real passwords, JWT signing keys, refresh-token peppers, and Docker Hub credentials must never be committed to Git.

---

# Important Commands

Build and test:

```powershell
dotnet restore
dotnet build
dotnet test
```

Docker Compose:

```powershell
docker compose --env-file .env.compose up --build
docker compose down
```

Kubernetes:

```powershell
kubectl get nodes
kubectl get namespaces
kubectl get all -n barber-booking
kubectl get ingress -n barber-booking
kubectl get pvc -n barber-booking
```

View backend logs:

```powershell
kubectl logs deployment/backend -n barber-booking
```

Inspect PostgreSQL:

```powershell
kubectl logs postgres-0 -n barber-booking
```

Access the application through Ingress locally:

```powershell
kubectl port-forward service/ingress-nginx-controller 8082:80 -n ingress-nginx
```

---

# Documentation

Development notes are available in the `docs/` directory.