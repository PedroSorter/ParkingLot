# ParkingLot

A parking lot management experience sample:

- .NET 10 API using PostgreSQL through Npgsql.
- React + Vite frontend.
- Docker Compose database setup with seeded demo data.
- Swagger documentation for the API.

## Preparation

- .NET 10 SDK
- Node.js
- Docker Desktop

## Database Setup

Start the database:

bash docker compose up -d

Default local database credentials:

- Host: `localhost`
- Port: `5432`
- Database: `postgres`
- Username: `postgres`
- Password: `parkinglot`

## Demo Data

The Docker database is seeded with two parking lots:

- `11111111-1111-1111-1111-111111111111`: empty demo lot with small, regular, and large spots.
- `22222222-2222-2222-2222-222222222222`: demo lot with parked vehicles.

Seeded vehicles:

- `DEMO-CAR`: parked in the second demo lot.
- `DEMO-VAN`: parked in the second demo lot.

Reset the database and reseed it:

bash
docker compose down -v
docker compose up -d

## Run the API

bash
dotnet run --project src/ParkingLot.Api/ParkingLot.Api.csproj

Swagger is at `http://localhost:5276/swagger` or `https://localhost:7274/swagger` in dev

## Run the User Access API

bash
dotnet run --project src/UserAccess.Api/UserAccess.Api.csproj

Swagger is at `http://localhost:5196/swagger` or `https://localhost:7049/swagger`. Use the `Authorize` button with `Bearer <token>` after calling the login endpoint.

Demo user:

- Email: `demo@example.com`
- Password: `ParkingLot123!`

Endpoints:

- `POST /api/users/register`: create a user.
- `POST /api/users/login`: log in and receive a bearer token.
- `GET /api/users/me`: authorized endpoint for the current user.
- `GET /api/demo/public`: public endpoint.
- `GET /api/demo/protected`: authorized endpoint that requires `Authorization: Bearer <token>`.

## Run the React Frontend

bash
cd src/ParkingLot.Web
npm install
npm run dev

Open the frontend at `http://localhost:5173`.

