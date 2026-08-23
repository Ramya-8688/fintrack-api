# Architecture Documentation - FinTrack Expense Splitting Feature

## System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        API Layer                                 │
│   TransactionsController  │  ExpensesController                 │
│   (HTTP Endpoints)        │  (HTTP Endpoints)                   │
└────────────────┬──────────────────────────┬─────────────────────┘
                 │                          │
┌────────────────┴──────────────────────────┴─────────────────────┐
│                    Application Layer                             │
│  ┌──────────────────┐  ┌─────────────────┐  ┌────────────────┐ │
│  │ Services         │  │ CQRS Handlers   │  │ Validators     │ │
│  ├─ Transaction    │  ├─ Commands       │  ├─ Fluent        │ │
│  │  Service        │  │  - Create       │  │  Validation    │ │
│  ├─ Balance        │  ├─ Handlers       │  └────────────────┘ │
│  │  Calculation    │  │  - CreateCmd    │                      │
│  │  Service        │  │  - GetBalances  │  ┌────────────────┐ │
│  └──────────────────┘  └─────────────────┘  │ AutoMapper     │ │
│                                             │ DTOs           │ │
│                                             └────────────────┘ │
└────────────────┬──────────────────────────┬─────────────────────┘
                 │                          │
┌────────────────┴──────────────────────────┴─────────────────────┐
│                    Domain Layer                                  │
│  ┌──────────────────┐  ┌─────────────────┐                      │
│  │ Entities         │  │ Interfaces      │                      │
│  ├─ Transaction    │  ├─ ITransaction   │                      │
│  ├─ SharedExpense  │  │  Repository     │                      │
│  ├─ Expense        │  ├─ IExpense       │                      │
│  │  Participant    │  │  Repository     │                      │
│  ├─ User           │  └─────────────────┘                      │
│  └──────────────────┘  ┌─────────────────┐                      │
│                        │ Exceptions      │                      │
│  ┌──────────────────┐  ├─ Invalid        │                      │
│  │ Value Objects    │  │  Transaction    │                      │
│  ├─ Money           │  ├─ Invalid        │                      │
│  │  (decimal)       │  │  Expense        │                      │
│  └──────────────────┘  ├─ Unauthorized   │                      │
│                        ├─ DataAccess     │                      │
│                        └─────────────────┘                      │
└────────────────┬──────────────────────────┬─────────────────────┘
                 │                          │
┌────────────────┴──────────────────────────┴─────────────────────┐
│                  Infrastructure Layer                            │
│  ┌────────────────────┐  ┌──────────────────────┐                │
│  │ Data Access        │  │ Authentication       │                │
│  ├─ Transaction       │  ├─ JWT Token          │                │
│  │  Repository        │  │  Provider           │                │
│  ├─ Expense           │  └──────────────────────┘                │
│  │  Repository        │  ┌──────────────────────┐                │
│  ├─ DbContext         │  │ Logging              │                │
│  │ (EF Core)          │  ├─ Serilog            │                │
│  └────────────────────┘  │  Configuration      │                │
│                          └──────────────────────┘                │
└────────────────┬──────────────────────────┬─────────────────────┘
                 │                          │
┌────────────────┴──────────────────────────┴─────────────────────┐
│                    Database Layer                                │
│   SQL Server 2022 / PostgreSQL 15                               │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│   │ Transactions │  │SharedExpenses│  │Participants  │         │
│   │  Table       │  │   Table      │  │   Table      │         │
│   └──────────────┘  └──────────────┘  └──────────────┘         │
└─────────────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. **API Layer (Controllers)**
- **Location:** `src/FinTrack.Api/Controllers/`
- **Responsibility:** HTTP request/response handling
- **Key Components:**
  - `TransactionsController` - CRUD operations on transactions
  - `ExpensesController` - Shared expense management
  - Authorization via `[Authorize]` attribute
  - Request validation using FluentValidation
  - Consistent `ApiResponse<T>` wrapper for all responses

**Why:** Separates HTTP concerns from business logic. Easy to test and evolve API independently.

### 2. **Application Layer (Services & CQRS)**
- **Location:** `src/FinTrack.Application/`
- **Responsibility:** Orchestrate business logic, cross-cutting concerns
- **Key Components:**
  - **Services:**
    - `TransactionService` - Create, read transactions with authorization
    - `BalanceCalculationService` - Net balance computation logic
  - **CQRS Pattern:**
    - `CreateExpenseCommand` / `CreateExpenseCommandHandler`
    - `GetUserBalancesQuery` / `GetUserBalancesQueryHandler`
  - **Validators:**
    - `CreateTransactionValidator` - FluentValidation rules for transactions
    - `CreateExpenseValidator` - FluentValidation rules for expenses
  - **DTOs:** Transfer objects for API serialization

**Why:** CQRS separates write (commands) from read (queries) operations. Services handle cross-layer coordination. Validators ensure consistent input validation before processing.

### 3. **Domain Layer (Business Logic & Entities)**
- **Location:** `src/FinTrack.Domain/`
- **Responsibility:** Core business logic, independent of infrastructure
- **Key Components:**
  - **Entities:**
    - `Transaction` - Financial transaction record
    - `SharedExpense` - Shared expense with participants
    - `ExpenseParticipant` - Individual share in an expense
    - `User` - User identity and profile
  - **Interfaces:**
    - `ITransactionRepository` - Contract for transaction persistence
    - `IExpenseRepository` - Contract for expense persistence
  - **Exceptions:**
    - `InvalidTransactionException`
    - `InvalidExpenseException`
    - `UnauthorizedAccessException`
    - `DataAccessException`
  - **Value Objects:**
    - `Money` - Represents decimal amounts with precision

**Why:** Domain is framework-agnostic and business-focused. Can be tested independently. Contains all validation rules. Exceptions provide specific error context.

### 4. **Infrastructure Layer (Data & External Services)**
- **Location:** `src/FinTrack.Infrastructure/`
- **Responsibility:** External system integration (database, auth, logging)
- **Key Components:**
  - **Data Access:**
    - `ApplicationDbContext` - EF Core DbContext
    - `TransactionRepository` - Transaction persistence implementation
    - `ExpenseRepository` - Expense persistence implementation
  - **Authentication:**
    - `JwtTokenProvider` - JWT token generation and validation
  - **Logging:**
    - `LoggingConfiguration` - Serilog setup

**Why:** Isolates infrastructure details (database, auth) from business logic. Repositories implement domain interfaces, allowing implementation swapping (SQL Server ↔ PostgreSQL).

### 5. **Database Layer**
- **Technology:** SQL Server 2022 / PostgreSQL 15
- **Design:**
  - **Transactions Table:**
    ```sql
    CREATE TABLE Transactions (
        Id INT PRIMARY KEY IDENTITY,
        UserId NVARCHAR(100) NOT NULL,
        TransactionType NVARCHAR(50) NOT NULL, -- Payment, Refund, Split
        Amount DECIMAL(18, 2) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        Status NVARCHAR(50) NOT NULL, -- Pending, Completed, Failed
        CreatedDate DATETIME2 NOT NULL,
        SharedExpenseId INT NULL,
        FOREIGN KEY (SharedExpenseId) REFERENCES SharedExpenses(Id)
    );
    ```
  - **SharedExpenses Table:**
    ```sql
    CREATE TABLE SharedExpenses (
        Id INT PRIMARY KEY IDENTITY,
        CreatorId NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        TotalAmount DECIMAL(18, 2) NOT NULL,
        SplitType NVARCHAR(50) NOT NULL, -- Equal, Custom
        Status NVARCHAR(50) NOT NULL, -- Active, Settled, Cancelled
        CreatedDate DATETIME2 NOT NULL,
        FOREIGN KEY (CreatorId) REFERENCES Users(Id)
    );
    ```
  - **ExpenseParticipants Table:**
    ```sql
    CREATE TABLE ExpenseParticipants (
        Id INT PRIMARY KEY IDENTITY,
        ExpenseId INT NOT NULL,
        UserId NVARCHAR(100) NOT NULL,
        ShareAmount DECIMAL(18, 2) NOT NULL,
        Status NVARCHAR(50) NOT NULL, -- Pending, Settled
        FOREIGN KEY (ExpenseId) REFERENCES SharedExpenses(Id),
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    ```

**Why:** Structured schema with proper relationships. Decimal(18,2) for financial amounts ensures precision. Status fields track workflow state.

---

## Data Flow

### Transaction Create Flow

```
HTTP POST /api/transactions
    ↓
[TransactionsController.CreateTransaction()]
    ├─ Extract current user from JWT claims
    ├─ Validate authorization (user creating for self)
    ↓
[TransactionService.CreateAsync()]
    ├─ Validate with CreateTransactionValidator (FluentValidation)
    ├─ Call Transaction.IsValid() (domain validation)
    ├─ Log "Creating transaction..."
    ↓
[TransactionRepository.AddAsync()]
    ├─ _context.Transactions.AddAsync(transaction)
    ├─ _context.SaveChangesAsync()
    ├─ Log success or error
    ↓
[Response: 201 Created with TransactionDto]
```

### Expense Create + Balance Calculation Flow

```
HTTP POST /api/expenses
    ↓
[ExpensesController.CreateExpense()]
    ├─ Set CreatorId = current authenticated user
    ├─ Send CreateExpenseCommand via MediatR
    ↓
[CreateExpenseCommandHandler.Handle()]
    ├─ Validate command with CreateExpenseValidator
    ├─ Calculate shares:
    │  ├─ If Equal: TotalAmount / ParticipantCount
    │  └─ If Custom: Verify sums to total
    ├─ Create SharedExpense + ExpenseParticipants
    ├─ Call repository.AddAsync()
    ├─ Log "Expense created..."
    ↓
[ExpenseRepository.AddAsync()]
    ├─ _context.SharedExpenses.AddAsync(expense)
    ├─ _context.ExpenseParticipants.AddRangeAsync(participants)
    ├─ _context.SaveChangesAsync()
    ↓
[Response: 201 Created with ExpenseDto]

---

Later when user queries balances:

HTTP GET /api/expenses/user/balances
    ↓
[ExpensesController.GetUserBalances()]
    ├─ Send GetUserBalancesQuery via MediatR
    ↓
[GetUserBalancesQueryHandler.Handle()]
    ├─ Fetch all expenses for user
    ├─ Call BalanceCalculationService.CalculateNetBalances()
    │  ├─ For each expense where Status="Active":
    │  │  ├─ If CreatorId == userId: others OWE user
    │  │  ├─ Else if CreatorId == otherUserId: user OWES
    │  │  └─ Calculate net (sum all transactions with each person)
    │  └─ Return Dictionary<UserId, NetAmount>
    ├─ Map to UserBalanceDto[] with Status (Owed/Owes/Settled)
    ↓
[Response: 200 OK with UserBalanceDto[]]
```

---

## Why This Architecture is Appropriate for Fintech

### 1. **Separation of Concerns**
- **Domain Layer** focuses purely on business rules (expense splitting, balance calculation)
- **Application Layer** handles orchestration without domain knowledge
- **Infrastructure Layer** is swappable (database, auth provider changes)
- **Benefit:** Financial logic is isolated and testable. Changes to database don't affect business logic.

### 2. **Multi-Tenancy & Authorization**
- Every transaction/expense includes `UserId` / `CreatorId`
- Service layer enforces `currentUserId` checks
- Controller uses `[Authorize]` attribute to require JWT
- **Benefit:** Prevents users from accessing others' financial data (PCI-DSS compliance).

### 3. **Auditability & Logging**
- Structured logging at every layer (Serilog)
- Every financial operation logged with amounts, participants, outcomes
- `CreatedDate` timestamps on all entities (UTC)
- **Benefit:** Audit trail for compliance, debugging, fraud investigation.

### 4. **Data Integrity**
- EF Core DbContext manages transactions
- `Transaction.IsValid()` and validators prevent invalid data at entry
- Foreign keys enforce referential integrity
- Decimal(18,2) ensures precise financial amounts
- **Benefit:** No corrupt financial data in database. Supports reconciliation.

### 5. **Error Handling & Recovery**
- Specific exception hierarchy (`InvalidExpenseException`, `UnauthorizedAccessException`)
- Each handler catches and logs specific errors
- API returns structured error responses
- **Benefit:** Clients know why operations failed. Easier debugging and support.

### 6. **Scalability & Performance**
- Repository pattern allows for query optimization (e.g., caching expensive balance calculations)
- CQRS separates read and write, allowing independent scaling
- Async/await throughout prevents thread starvation
- CancellationTokens enable graceful shutdown
- **Benefit:** Handles high transaction volume without blocking.

### 7. **Security**
- JWT authentication on all endpoints
- Authorization checks at service layer (defense in depth)
- No sensitive data (passwords, tokens) in logs
- Input validation prevents injection attacks
- **Benefit:** Protects against common fintech vulnerabilities.

### 8. **Testability**
- Domain logic has no external dependencies (no HTTP, no DB)
- Services use constructor injection for mocking
- Repository pattern allows fake implementations for testing
- **Benefit:** >90% code coverage possible. Bugs caught before production.

---

## Key Design Decisions

| Decision | Reasoning |
|----------|-----------|
| **CQRS Pattern** | Separates expense creation (write) from balance queries (read). Balance calculation is expensive; can cache reads separately. |
| **Repository Pattern** | Abstracts database details from business logic. Can swap SQL Server ↔ PostgreSQL without changing services. |
| **FluentValidation** | Centralized, declarative validation rules. Easy to test and modify business constraints. |
| **JWT Authentication** | Stateless, scalable. No session management needed. Standard for APIs. |
| **Serilog Structured Logging** | Machine-readable logs for debugging and compliance audits. Easy integration with APM tools. |
| **MediatR for CQRS** | Single responsibility per handler. Decouples controllers from service logic. Easy to add cross-cutting concerns (logging, validation). |
| **Decimal Type for Amounts** | Precise financial calculations (no floating-point rounding errors). Matches database DECIMAL(18,2). |
| **Soft Delete Strategy (Status field)** | Don't physically delete expenses; mark as "Cancelled". Maintains audit trail and referential integrity. |

---

## Deployment Considerations

1. **Database Migrations:** EF Core Code-First approach allows version control of schema
2. **Logging Infrastructure:** Serilog configured to write to file/Seq for centralized logging
3. **Authentication:** JWT secret key stored in secure configuration (not in code)
4. **CORS:** Configured for specific allowed origins (not *)
5. **Rate Limiting:** Implement per-user API rate limits to prevent abuse

---

## Compliance & Standards

- **PCI-DSS:** No credit card storage (out of scope). Financial data isolated by user.
- **GDPR:** User deletion logic can cascade through expenses/transactions.
- **SOC 2:** Audit logging, error handling, secure communication (HTTPS enforced).
- **Financial Regulations:** Decimal precision, immutable audit trails, clear transaction records.

