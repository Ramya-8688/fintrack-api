# FinTrack API - Expense Splitting Feature

## Technology Stack

**Runtime & Language:**
- .NET 8.0 LTS
- C# 12.0
- ASP.NET Core 8.0

**Framework & Server:**
- ASP.NET Core Web API 8.0
- Kestrel HTTP server
- Built-in Dependency Injection

**Database & ORM:**
- SQL Server 2022 / PostgreSQL 15
- Entity Framework Core 8.0
- EF Core Code-First Migrations

**Validation & Business Logic:**
- FluentValidation 11.x
- MediatR 12.x (CQRS pattern)
- AutoMapper 13.x

**Testing:**
- xUnit 2.x
- Moq 4.x
- FluentAssertions 6.x

**Security:**
- JWT (JSON Web Tokens)
- bcrypt.net password hashing
- CORS middleware

**Logging:**
- Serilog 3.x structured logging

## Project Structure

```
fintrack-api/
├── .github/
│   └── copilot-instructions.md
├── src/
│   ├── FinTrack.Api/
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   └── FinTrack.Api.csproj
│   ├── FinTrack.Application/
│   │   ├── Services/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Validators/
│   │   └── FinTrack.Application.csproj
│   ├── FinTrack.Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Interfaces/
│   │   ├── Exceptions/
│   │   └── FinTrack.Domain.csproj
│   ├── FinTrack.Infrastructure/
│   │   ├── Data/
│   │   ├── Repositories/
│   │   ├── Authentication/
│   │   └── FinTrack.Infrastructure.csproj
│   └── FinTrack.Tests/
│       ├── Unit/
│       ├── Integration/
│       └── FinTrack.Tests.csproj
├── FinTrack.sln
├── docker-compose.yml
└── .env.example
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server 2022 or PostgreSQL 15
- Docker (optional, for SQL Server)

### Setup

```bash
# Clone repository
git clone https://github.com/Ramya-8688/fintrack-api.git
cd fintrack-api

# Restore NuGet packages
dotnet restore

# Configure database
cp .env.example .env
# Edit .env with your database connection string

# Start SQL Server with Docker (optional)
docker-compose up -d

# Apply migrations
dotnet ef database update --project src/FinTrack.Infrastructure

# Run the API
dotnet run --project src/FinTrack.Api
```

API will be available at `https://localhost:5001`
Swagger UI at `https://localhost:5001/swagger`

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test project
dotnet test src/FinTrack.Tests/FinTrack.Tests.csproj
```

## API Endpoints

### Transactions
- `POST /api/transactions` - Create transaction
- `GET /api/transactions/user/{userId}` - Get user transactions
- `DELETE /api/transactions/user/{userId}` - Delete all user transactions

### Expenses
- `POST /api/expenses` - Create shared expense
- `GET /api/expenses/user/{userId}/balances` - Get user balances
- `GET /api/expenses/{expenseId}` - Get expense details
- `DELETE /api/expenses/{expenseId}` - Delete expense

## Architecture

**Layered Architecture:**
```
API Controllers
  ↓
Application Services (MediatR)
  ↓
Domain Entities & Business Logic
  ↓
Infrastructure Repositories (EF Core)
  ↓
Database (SQL Server/PostgreSQL)
```

**Key Principles:**
- Domain-Driven Design (DDD)
- CQRS pattern with MediatR
- Repository pattern for data access
- Dependency Injection throughout
- Comprehensive input validation
- User authorization checks
- Structured logging with correlation IDs

## Authentication

All protected endpoints require JWT Bearer token:

```
Authorization: Bearer {jwt_token}
```

## Configuration

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FinTrackDb;User Id=sa;Password=YourPassword;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "fintrack-api",
    "Audience": "fintrack-client",
    "ExpirationMinutes": 60
  }
}
```

## Testing Coverage

Minimum 6+ test cases covering:
- Equal split among participants
- Custom split with validation
- Invalid amounts (should fail)
- Net balance calculations
- Single participant edge case
- Unauthorized access
- Authorization checks

## Documentation

- **REVIEW.md** - Transaction module code review and remediation
- **PROMPTS.md** - Copilot prompt chain and techniques used
- **ARCHITECTURE.md** - System design and data flow
- **TOOL_STRATEGY.md** - Copilot feature usage and limitations
- **PR_DESCRIPTION.md** - Pull request submission details

## Contributing

- Follow C# naming conventions (PascalCase for public members)
- Write unit tests for all business logic
- Use Conventional Commits format
- Submit PRs with detailed descriptions

## License

Proprietary - FinTrack Fintech Startup
