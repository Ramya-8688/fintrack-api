# GitHub Copilot Custom Instructions - FinTrack API

## Project Context

FinTrack is a personal finance management API built with .NET 8.0 and C# 12.0. We're implementing an expense-splitting feature that allows users to split shared expenses and track balances. This is a fintech application requiring high security and data integrity standards.

## Technology Stack Requirements

### Language & Runtime
- **Language:** C# 12.0 only (no VB.NET, F#, or other .NET languages)
- **Runtime:** .NET 8.0 LTS or later
- **Target Framework:** net8.0
- **Nullable Reference Types:** Enable strict null checking (`<Nullable>enable</Nullable>`)
- **Implicit Usings:** Enabled

### Architecture Layers

```
API Layer (Controllers, Routes)
  ↓
Application Layer (Services, Commands, Queries, Validators)
  ↓
Domain Layer (Entities, Value Objects, Interfaces, Exceptions)
  ↓
Infrastructure Layer (DbContext, Repositories, Authentication, Logging)
  ↓
Database (SQL Server / PostgreSQL)
```

### Framework & Library Standards

1. **Web Framework:** ASP.NET Core 8.0 Web API only
2. **Dependency Injection:** Microsoft.Extensions.DependencyInjection (built-in)
3. **ORM:** Entity Framework Core 8.0 (DbContext, DbSet, migrations)
4. **Validation:** FluentValidation 11.x (AbstractValidator<T>)
5. **CQRS:** MediatR 12.x (IRequest, IRequestHandler, IPipelineBehavior)
6. **Mapping:** AutoMapper 13.x (IMapper, Profile)
7. **Testing:** xUnit 2.x + Moq 4.x + FluentAssertions 6.x
8. **Logging:** Serilog 3.x with structured logging
9. **Authentication:** JWT tokens with System.IdentityModel.Tokens.Jwt
10. **Password Security:** bcrypt.net (never use plain text)

## Coding Standards

### Naming Conventions

- **Namespaces:** `FinTrack.{Layer}.{Feature}` (e.g., `FinTrack.Application.Services`)
- **Classes:** PascalCase (e.g., `TransactionService`, `CreateExpenseCommand`)
- **Methods:** PascalCase (e.g., `GetUserBalances`, `CreateExpense`)
- **Parameters:** camelCase (e.g., `userId`, `expenseId`)
- **Private Fields:** _camelCase (e.g., `_repository`, `_logger`)
- **Constants:** UPPER_SNAKE_CASE (e.g., `MAX_AMOUNT`, `DEFAULT_CURRENCY`)
- **Interfaces:** IPascalCase (e.g., `ITransactionRepository`, `IExpenseService`)
- **Properties:** PascalCase with get; set; (e.g., `public string UserId { get; set; }`)
- **Async Methods:** Suffix with `Async` (e.g., `GetUserAsync`, `CreateAsync`)

### Code Style

- **Access Modifiers:** Always explicit (public, private, protected, internal)
- **Async/Await:** Use async Task for operations without return, Task<T> for returns
- **Null Handling:** Use null-coalescing (??) and null-conditional (?.) operators
- **LINQ:** Prefer method syntax over query syntax
- **Using Statements:** Prefer using declarations (C# 8.0+)
- **String Interpolation:** Use $"" format only
- **Exception Handling:** Catch specific exceptions, never bare catch {}
- **Logging:** Use _logger.LogInformation, LogError, LogDebug at appropriate levels

### File Organization

```csharp
// 1. Using statements (System, Microsoft, third-party, FinTrack)
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FinTrack.Domain.Entities;

// 2. Namespace
namespace FinTrack.Application.Services;

/// <summary>
/// XML documentation for the class
/// </summary>
public class TransactionService : ITransactionService
{
    // Private fields
    private readonly ITransactionRepository _repository;
    private readonly ILogger<TransactionService> _logger;

    // Constructor
    public TransactionService(
        ITransactionRepository repository,
        ILogger<TransactionService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Public methods
    public async Task<TransactionDto> CreateAsync(CreateTransactionCommand command)
    {
        // Implementation
    }

    // Private methods
    private bool ValidateAmount(decimal amount)
    {
        return amount > 0;
    }
}
```

## Architecture Patterns

### 1. Repository Pattern

```csharp
public interface ITransactionRepository
{
    /// <summary>Gets transaction by ID</summary>
    Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>Gets all transactions for a user</summary>
    Task<IEnumerable<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>Adds new transaction</summary>
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    
    /// <summary>Deletes transaction</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var transaction = await GetByIdAsync(id, cancellationToken);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### 2. Service Layer with Logging

```csharp
public interface ITransactionService
{
    /// <summary>Creates a new transaction</summary>
    Task<TransactionDto> CreateAsync(CreateTransactionCommand command);
    
    /// <summary>Retrieves user transactions</summary>
    Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(string userId);
}

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository repository,
        ILogger<TransactionService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionCommand command)
    {
        _logger.LogInformation("Creating transaction for user {UserId} with amount {Amount}", 
            command.UserId, command.Amount);

        try
        {
            var transaction = new Transaction
            {
                UserId = command.UserId,
                Amount = command.Amount,
                Description = command.Description,
                TransactionType = command.TransactionType,
                CreatedDate = DateTime.UtcNow,
                Status = "Completed"
            };

            if (!transaction.IsValid())
            {
                _logger.LogWarning("Transaction validation failed for user {UserId}", command.UserId);
                throw new InvalidOperationException("Transaction validation failed");
            }

            await _repository.AddAsync(transaction);
            
            _logger.LogInformation("Transaction created successfully with ID {TransactionId}", transaction.Id);
            
            return new TransactionDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                Amount = transaction.Amount,
                Description = transaction.Description,
                CreatedDate = transaction.CreatedDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction for user {UserId}", command.UserId);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(string userId)
    {
        _logger.LogInformation("Retrieving transactions for user {UserId}", userId);
        
        var transactions = await _repository.GetByUserAsync(userId);
        
        return transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            UserId = t.UserId,
            Amount = t.Amount,
            Description = t.Description,
            CreatedDate = t.CreatedDate
        }).ToList();
    }
}
```

### 3. CQRS with MediatR

```csharp
// Command
public class CreateExpenseCommand : IRequest<ExpenseDto>
{
    /// <summary>ID of user creating the expense</summary>
    public string CreatorId { get; set; } = string.Empty;
    
    /// <summary>Expense description</summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Total expense amount</summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>Type of split: Equal or Custom</summary>
    public string SplitType { get; set; } = "Equal";
    
    /// <summary>List of participants</summary>
    public List<ParticipantDto> Participants { get; set; } = new();
}

// Handler
public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, ExpenseDto>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger<CreateExpenseCommandHandler> _logger;

    public CreateExpenseCommandHandler(
        IExpenseRepository expenseRepository,
        ILogger<CreateExpenseCommandHandler> logger)
    {
        _expenseRepository = expenseRepository ?? throw new ArgumentNullException(nameof(expenseRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExpenseDto> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating expense by user {CreatorId} with amount {Amount}", 
            request.CreatorId, request.TotalAmount);

        // Validation
        if (request.TotalAmount <= 0)
            throw new InvalidExpenseException("Total amount must be greater than 0");
        
        if (request.Participants.Count < 2)
            throw new InvalidExpenseException("Expense must have at least 2 participants");

        // Create expense
        var expense = new SharedExpense
        {
            CreatorId = request.CreatorId,
            Description = request.Description,
            TotalAmount = request.TotalAmount,
            SplitType = request.SplitType,
            CreatedDate = DateTime.UtcNow
        };

        // Calculate shares
        var participants = CalculateShares(request, expense);
        expense.Participants = participants;

        await _expenseRepository.AddAsync(expense, cancellationToken);
        
        _logger.LogInformation("Expense created with ID {ExpenseId}", expense.Id);
        
        return MapToDto(expense);
    }

    private List<ExpenseParticipant> CalculateShares(CreateExpenseCommand request, SharedExpense expense)
    {
        if (request.SplitType == "Equal")
        {
            var shareAmount = request.TotalAmount / request.Participants.Count;
            return request.Participants.Select(p => new ExpenseParticipant
            {
                UserId = p.UserId,
                ShareAmount = shareAmount,
                Status = "Pending"
            }).ToList();
        }
        else // Custom
        {
            var totalShares = request.Participants.Sum(p => p.ShareAmount ?? 0);
            if (Math.Abs(totalShares - request.TotalAmount) > 0.01m)
                throw new InvalidExpenseException("Custom shares must sum to total amount");
            
            return request.Participants.Select(p => new ExpenseParticipant
            {
                UserId = p.UserId,
                ShareAmount = p.ShareAmount ?? 0,
                Status = "Pending"
            }).ToList();
        }
    }

    private ExpenseDto MapToDto(SharedExpense expense)
    {
        return new ExpenseDto
        {
            Id = expense.Id,
            CreatorId = expense.CreatorId,
            Description = expense.Description,
            TotalAmount = expense.TotalAmount,
            SplitType = expense.SplitType,
            Participants = expense.Participants.Select(p => new ParticipantDto
            {
                UserId = p.UserId,
                ShareAmount = p.ShareAmount
            }).ToList(),
            CreatedDate = expense.CreatedDate
        };
    }
}
```

### 4. Validation with FluentValidation

```csharp
public class CreateExpenseValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.CreatorId)
            .NotEmpty().WithMessage("Creator ID is required")
            .Length(1, 100).WithMessage("Creator ID must be between 1 and 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .Length(3, 500).WithMessage("Description must be between 3 and 500 characters");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(10000000).WithMessage("Amount cannot exceed $10,000,000")
            .DecimalPrecision(2).WithMessage("Amount cannot have more than 2 decimal places");

        RuleFor(x => x.Participants)
            .NotEmpty().WithMessage("Participants list cannot be empty")
            .Must(p => p.Count >= 2).WithMessage("Expense must have at least 2 participants")
            .Must(p => p.Count <= 100).WithMessage("Expense cannot have more than 100 participants");

        RuleForEach(x => x.Participants)
            .ChildRules(participant =>
            {
                participant.RuleFor(p => p.UserId)
                    .NotEmpty().WithMessage("Participant user ID is required");
                
                participant.RuleFor(p => p.ShareAmount)
                    .GreaterThan(0).When(x => x.SplitType == "Custom")
                    .WithMessage("Share amount must be greater than 0");
            });
    }
}
```

## Security Requirements

### Authorization
- Add [Authorize] attribute to all protected endpoints
- Verify user owns the resource before operations
- Log all authorization failures
- Use ClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) for user ID

### Data Validation
- Validate all inputs at API layer with FluentValidation
- Validate business rules at domain/service layer
- Never trust user input for sensitive operations
- Use specific exception types for error handling

### Sensitive Data
- Never log passwords, tokens, or sensitive information
- Use value objects for Money (decimal precision)
- Implement audit trails for all financial transactions
- Hash passwords with bcrypt.net

### Error Handling

```csharp
public class ApiException : Exception
{
    public string Code { get; }
    public int HttpStatusCode { get; }

    public ApiException(string message, string code, int statusCode = 400) : base(message)
    {
        Code = code;
        HttpStatusCode = statusCode;
    }
}

public class InvalidExpenseException : ApiException
{
    public InvalidExpenseException(string message) 
        : base(message, "INVALID_EXPENSE", 400) { }
}

public class UnauthorizedAccessException : ApiException
{
    public UnauthorizedAccessException(string message) 
        : base(message, "UNAUTHORIZED", 403) { }
}

public class ExpenseNotFoundException : ApiException
{
    public ExpenseNotFoundException(int id) 
        : base($"Expense with ID {id} not found", "EXPENSE_NOT_FOUND", 404) { }
}
```

## Testing Standards

### Unit Tests with xUnit

```csharp
public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _mockRepository;
    private readonly Mock<ILogger<CreateExpenseCommandHandler>> _mockLogger;
    private readonly CreateExpenseCommandHandler _handler;

    public ExpenseServiceTests()
    {
        _mockRepository = new Mock<IExpenseRepository>();
        _mockLogger = new Mock<ILogger<CreateExpenseCommandHandler>>();
        _handler = new CreateExpenseCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidEqualSplit_ShouldCreateExpenseWithEqualShares()
    {
        // Arrange
        var command = new CreateExpenseCommand
        {
            CreatorId = "user1",
            Description = "Dinner",
            TotalAmount = 120m,
            SplitType = "Equal",
            Participants = new()
            {
                new() { UserId = "user1" },
                new() { UserId = "user2" },
                new() { UserId = "user3" }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(120m);
        result.Participants.Should().HaveCount(3);
        result.Participants.First().ShareAmount.Should().Be(40m);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<SharedExpense>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidAmount_ShouldThrowInvalidExpenseException()
    {
        // Arrange
        var command = new CreateExpenseCommand
        {
            CreatorId = "user1",
            Description = "Dinner",
            TotalAmount = 0,
            SplitType = "Equal",
            Participants = new() { new() { UserId = "user1" } }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidExpenseException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
```

### Test Naming Convention

`[MethodName]_[Condition]_[ExpectedResult]`

Example: `CreateExpense_WithEqualSplit_ShouldReturnThreeEqualShares`

## Logging Standards

### Structured Logging with Serilog

```csharp
// Information level - Business-significant events
_logger.LogInformation("Creating expense for user {UserId} with amount {Amount}", userId, amount);

// Warning level - Potential issues
_logger.LogWarning("Validation failed for expense {ExpenseId}: {Reason}", expenseId, reason);

// Error level - Exceptions and failures
_logger.LogError(ex, "Failed to create expense for user {UserId}", userId);

// Debug level - Detailed information for debugging
_logger.LogDebug("Expense participants calculated: {@Participants}", participants);
```

## Database & Migrations

### Entity Framework Core Conventions

- Table names: Plural (Transactions, SharedExpenses)
- Primary key: Id (int or Guid)
- Foreign keys: {EntityName}Id (UserId, ExpenseId)
- Datetime fields: CreatedDate, UpdatedDate (UTC)
- Precision for money: decimal(18, 2)

### Creating Migrations

```bash
dotnet ef migrations add AddExpenseParticipantsTable --project src/FinTrack.Infrastructure
dotnet ef database update --project src/FinTrack.Infrastructure
```

## API Design Standards

### HTTP Methods
- **GET:** Retrieve resources (safe, idempotent)
- **POST:** Create new resources
- **PUT:** Replace entire resource
- **DELETE:** Remove resource

### Status Codes
- **200 OK:** Successful GET/PUT/DELETE
- **201 Created:** Successful POST
- **400 Bad Request:** Invalid input or validation failure
- **401 Unauthorized:** Missing/invalid authentication
- **403 Forbidden:** Authenticated but no permission
- **404 Not Found:** Resource doesn't exist
- **409 Conflict:** Business rule violation
- **500 Internal Server Error:** Unexpected error

### Response Format

```json
{
  "success": true,
  "data": {
    "id": 1,
    "description": "Dinner",
    "totalAmount": 120.00,
    "createdDate": "2024-01-15T10:30:00Z"
  },
  "message": "Expense created successfully"
}
```

## Prompting Strategies for Copilot

### Good Prompts

1. **Be Specific:**
   - ✅ "Generate ExpenseRepository class implementing IExpenseRepository in Domain layer with async GetByIdAsync, GetByUserAsync, AddAsync methods using EF Core DbSet"
   - ❌ "Generate a repository"

2. **Include Constraints:**
   - ✅ "Add FluentValidation for CreateExpenseCommand validator. Include rules: CreatorId not empty, TotalAmount > 0, at least 2 participants, custom shares sum to total"
   - ❌ "Add validation"

3. **Reference Patterns:**
   - ✅ "Generate ExpenseService similar to TransactionService with same error handling, logging pattern, and DI constructor"
   - ❌ "Generate a service"

4. **Specify Tests:**
   - ✅ "Generate xUnit test class for ExpenseService with Moq mocks including test cases: equal split 3 users, custom split validation, invalid amount, at least 2 participants validation"
   - ❌ "Generate tests"

## Common Pitfalls to Avoid

- ❌ Raw SQL queries → Always use EF Core DbSet and LINQ
- ❌ Hardcoded values → Use configuration and constants
- ❌ Bare catch blocks → Catch specific exceptions
- ❌ Synchronous calls → Use async/await throughout
- ❌ No error handling → Always handle exceptions
- ❌ Missing authorization → Verify user ownership
- ❌ No logging → Log business events at Information level
- ❌ Null dereference → Use null-coalescing and null-conditional operators
- ❌ Direct DbContext usage → Use Repository pattern
- ❌ Hardcoded connection strings → Use configuration

## Code Review Checklist for Copilot Output

Before accepting generated code, verify:

- [ ] Follows PascalCase for classes, _camelCase for fields
- [ ] All async methods are Task or Task<T>
- [ ] Specific exception types used
- [ ] Null checks with ?. or ?? operators
- [ ] Logging at Information level for business events
- [ ] Authorization checks for user operations
- [ ] Validation in FluentValidator or service layer
- [ ] XML documentation on public members
- [ ] No hardcoded strings or magic numbers
- [ ] Repository pattern used for data access
- [ ] Tests follow Arrange-Act-Assert
- [ ] No direct DbContext outside repositories
- [ ] CancellationToken parameters in async methods
- [ ] Proper error messages in exceptions
- [ ] Dependency injection in constructor

## When NOT to Use Copilot Auto-Complete

- Security-critical code (authentication, authorization)
- Financial calculations (manual verification required)
- Database migrations (review before applying)
- Error handling for edge cases
- Authorization logic for protected endpoints

## Final Notes

- All code must compile with zero warnings
- No unauthorized external dependencies
- Every PR must include tests
- Maintain backward compatibility
- Update PROMPTS.md with all Copilot-assisted features
- Use meaningful commit messages (Conventional Commits)
- Always include correlation IDs in logs for request tracing
