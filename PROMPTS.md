# Prompts Used to Build Expense Splitting Feature

## Overview

This document captures the complete prompt chain used to build the FinTrack Expense Splitting feature with GitHub Copilot. Each prompt demonstrates specific techniques for eliciting high-quality, production-ready code from an AI assistant.

**Total Prompts Used:** 12  
**Copilot Features Used:** Chat, Inline Code Completion, VS Code Commands  
**Techniques Applied:** Specificity, Decomposition, Few-shot examples, Constraint-based, Role-based, Iterative refinement

---

## Prompt Chain Execution

### Prompt #1: Define Expense Domain Model
**Feature:** Copilot Chat  
**Technique:** Specificity + Decomposition  
**Status:** ✅ USED

**Exact Prompt:**
```
Generate SharedExpense entity class for FinTrack.Domain.Entities namespace. 
Include properties: Id (int), CreatorId (string), Description (string), 
TotalAmount (decimal), SplitType (string "Equal"|"Custom"), Status (string), 
CreatedDate (DateTime), and Participants (List<ExpenseParticipant>).
Add XML documentation. Include IsValid() method checking CreatorId, Description not empty, 
TotalAmount > 0, at least 2 participants, all shares > 0. 
Add ValidateSharesSum() checking if shares sum to total with 0.01 tolerance.
Follow C# 12 conventions with nullable reference types enabled.
```

**Rationale:**
- **Specificity**: Listed all properties with types and constraints
- **Decomposition**: Broke into entity structure + validation methods
- **Few-shot**: Showed example property format (CreatorId: string)
- Ensured AI understood the fintech context (decimal precision, validations)

**Output Quality:** ⭐⭐⭐⭐⭐ Excellent  
**Changes Made:** None - code was production-ready

---

### Prompt #2: Create Expense Participant Entity
**Feature:** Copilot Chat  
**Technique:** Specificity + Few-shot  
**Status:** ✅ USED

**Exact Prompt:**
```
Generate ExpenseParticipant entity in FinTrack.Domain.Entities.
Include: Id (int), ExpenseId (int FK), UserId (string), ShareAmount (decimal), 
Status (string default "Pending").
Add XML docs. Use same patterns as Transaction entity for consistency.
Example property: public string UserId { get; set; } = string.Empty;
Target: .NET 8.0, C# 12.0
```

**Rationale:**
- **Few-shot**: Showed exact property initialization pattern from Transaction
- **Specificity**: Listed all fields with types and FK relationship
- **Reference pattern**: "Use same patterns as Transaction entity" triggers consistency

**Output Quality:** ⭐⭐⭐⭐⭐ Excellent  
**Changes Made:** None

---

### Prompt #3: Create IExpenseRepository Interface
**Feature:** Copilot Chat  
**Technique:** Specificity + Reference pattern  
**Status:** ✅ USED

**Exact Prompt:**
```
Generate IExpenseRepository interface in FinTrack.Domain.Interfaces.
Model after ITransactionRepository. Include methods:
- GetByIdAsync(int id, CancellationToken) -> Task<SharedExpense?>
- GetByUserAsync(string userId, CancellationToken) -> Task<IEnumerable<SharedExpense>>
- AddAsync(SharedExpense expense, CancellationToken) -> Task
- DeleteAsync(int id, CancellationToken) -> Task
- DeleteAllByUserAsync(string userId, CancellationToken) -> Task

Add XML documentation for each method. Use CancellationToken throughout.
Return nullable SharedExpense? for GetByIdAsync pattern.
```

**Rationale:**
- **Reference pattern**: "Model after ITransactionRepository" ensures consistency
- **Specificity**: Exact method signatures with parameter names and return types
- **Constraint-based**: "Use CancellationToken throughout" enforces async best practices

**Output Quality:** ⭐⭐⭐⭐⭐ Excellent  
**Changes Made:** None

---

### Prompt #4: Create CreateExpenseCommand (CQRS)
**Feature:** Copilot Chat  
**Technique:** Specificity + Constraint-based  
**Status:** ✅ USED

**Exact Prompt:**
```
Generate CreateExpenseCommand class in FinTrack.Application.Commands.
Implement IRequest<ExpenseDto> from MediatR.
Properties: CreatorId (string), Description (string), TotalAmount (decimal),
SplitType (string "Equal"|"Custom"), Participants (List<ParticipantDto>).

Follow this pattern:
public string CreatorId { get; set; } = string.Empty;
public decimal TotalAmount { get; set; }
public List<ParticipantDto> Participants { get; set; } = new();

Add XML documentation. Return type is ExpenseDto.
```

**Rationale:**
- **Few-shot**: Showed exact property initialization pattern with three examples
- **Specificity**: Listed all CQRS requirements (IRequest<T>)
- **Constraint-based**: "Return type is ExpenseDto" prevents AI from defaulting to void

**Output Quality:** ⭐⭐⭐⭐⭐ Excellent  
**Changes Made:** None

---

### Prompt #5: Create CreateExpenseValidator (Fluent Validation)
**Feature:** Copilot Chat  
**Technique:** Specificity + Constraint-based + Role-based  
**Status:** ✅ USED

**Exact Prompt:**
```
You are a fintech validation expert. Generate CreateExpenseValidator class 
in FinTrack.Application.Validators extending AbstractValidator<CreateExpenseCommand>.

Add these rules in constructor:
1. CreatorId: NotEmpty, Length(1,100)
2. Description: NotEmpty, Length(3,500)
3. TotalAmount: GreaterThan(0), LessThanOrEqualTo(10000000), DecimalPrecision(2)
4. Participants: NotEmpty, Count must be >= 2 and <= 100
5. For each participant: UserId NotEmpty, ShareAmount GreaterThan(0) when SplitType=="Custom"
6. Custom shares must sum to TotalAmount (add manual validation method)

Use namespace FinTrack.Application.Validators. Add XML docs.
Reference: Create validation similar to CreateTransactionValidator pattern.
```

**Rationale:**
- **Role-based**: "You are a fintech validation expert" sets context
- **Specificity**: Six numbered rules with exact constraints (10000000 limit, 0.01 tolerance)
- **Decomposition**: Broke validation into separate rules and custom validation
- **Reference pattern**: "similar to CreateTransactionValidator pattern" triggers consistency

**Output Quality:** ⭐⭐⭐⭐ Good  
**Changes Made:** Minor - added custom validation helper methods

---

### Prompt #6: Create CreateExpenseCommandHandler
**Feature:** Copilot Chat  
**Technique:** Specificity + Few-shot + Iterative refinement  
**Status:** ✅ USED (with refinement)

**Exact Prompt:**
```
Generate CreateExpenseCommandHandler in FinTrack.Application.Handlers 
implementing IRequestHandler<CreateExpenseCommand, ExpenseDto>.

Requirements:
1. Inject IExpenseRepository, ILogger<>, IValidator<CreateExpenseCommand>
2. In Handle() method:
   - Log "Creating expense by user {CreatorId} with amount {Amount}"
   - Validate command using injected validator, throw InvalidExpenseException if invalid
   - Create SharedExpense entity setting CreatorId, Description, TotalAmount, SplitType="Active"
   - Calculate shares by calling private CalculateShares() method
   - Validate shares sum correctly using expense.ValidateSharesSum()
   - Call _expenseRepository.AddAsync()
   - Log success, return MapToDto(expense)
3. Add CalculateShares(command, expense) private method:
   - If SplitType=="Equal": divide TotalAmount by participant count
   - If SplitType=="Custom": verify sum equals TotalAmount (tolerance 0.01m), throw if not
   - Return List<ExpenseParticipant> with participants
4. Add MapToDto() private method converting to ExpenseDto
5. Add XML documentation

Follow patterns from TransactionService.
```

**Rationale:**
- **Specificity**: Five numbered sections with exact method signatures
- **Few-shot**: Showed logging format and calculation logic
- **Reference pattern**: "Follow patterns from TransactionService"
- **Decomposition**: Broke into Handle, CalculateShares, MapToDto methods

**Output Quality:** ⭐⭐⭐⭐ Good  
**Changes Made:** Enhanced error handling in CalculateShares method

---

### Prompt #6b: Refinement - Add Error Handling
**Feature:** Copilot Chat  
**Technique:** Iterative refinement  
**Status:** ✅ USED

**Exact Prompt:**
```
The CreateExpenseCommandHandler needs better error handling. Update:

1. Wrap the Handle() method in try-catch
2. Catch InvalidExpenseException and re-throw (let it propagate)
3. Catch general Exception, log with LogError, throw new InvalidExpenseException("Failed to create expense: {ex.Message}")
4. In CalculateShares(), if totalShares != TotalAmount by >0.01m, 
   throw InvalidExpenseException("Custom shares ($X) do not match total ($Y)")
5. Add logging in CalculateShares with LogDebug for equal split amount and custom validation

Keep all existing code, only add error handling and logging.
```

**Rationale:**
- **Iterative refinement**: Improved code based on first generation
- **Specificity**: Five numbered requirements for error paths
- **Constraint-based**: ">0.01m" and specific exception messages

**Output Quality:** ⭐⭐⭐⭐⭐ Excellent  
**Changes Made:** None

---

### Prompt #7: Create BalanceCalculationService
**Feature:** Copilot Chat  
**Technique:** Specificity + Role-based + Few-shot  
**Status:** ✅ USED

**Exact Prompt:**
```
You are a financial calculations expert. Generate BalanceCalculationService 
in FinTrack.Application.Services.

Create interface IBalanceCalculationService with method:
Dictionary<string, decimal> CalculateNetBalances(string userId, IEnumerable<SharedExpense> expenses);

Implement with these rules:
- Only process expenses where Status == "Active"
- For each expense, find user's participant record
- For each OTHER participant in same expense:
  * If CreatorId == userId: others OWE user their shares (balance -= otherShare)
  * Else if CreatorId == otherUserId: user OWES creator (balance += userShare)
  * Else: net calculation (balance += userShare - otherShare)
- Return dictionary where positive = user is owed, negative = user owes
- Log: "Calculating net balances for user {UserId}"
- Log debug: "Calculated balances for user {UserId}: {@Balances}"
- Inject ILogger<BalanceCalculationService>, handle null expenses gracefully

Add XML documentation. Use namespace FinTrack.Application.Services.
```

**Rationale:**
- **Role-based**: "financial calculations expert" sets domain context
- **Specificity**: Six calculation rules with clear positive/negative semantics
- **Few-shot**: Showed logging statement format with structured variables
- **Constraint-based**: "Status == Active" ensures only active expenses count

**Output Quality:** ⭐⭐⭐⭐ Good  
**Changes Made:** Simplified three-way logic in balance calculation

---

### Prompt #8: Create BalanceDto
**Feature:** Copilot Chat  
**Technique:** Specificity  
**Status:** ✅ USED

**Exact Prompt:**
```
Generate UserBalanceDto class in FinTrack.Application.DTOs.

Properties:
- OtherUserId: string (who the balance is with)
- NetAmount: decimal (positive = owed to user, negative = user owes)
- Status: string ("Owed", "Owes", or "Settled")

Add XML documentation for each property explaining the meaning.
Use C# auto-properties with get; set;
```

**Rationale:**
- **Specificity**: Three properties with exact types
- Clear documentation requirement prevents ambiguous AI interpretations

**Output Quality:** ⭐⭐⭐⭐⭐ Excellent  
**Changes Made:** None

---

### Prompt #9: Create GetUserBalancesQuery (CQRS)
**Feature:** Copilot Chat  
**Technique:** Specificity  
**Status:** ✅ USED

**Exact Prompt:**
```
Generate GetUserBalancesQuery in FinTrack.Application.Queries.
Implement IRequest<IEnumerable<UserBalanceDto>>.

Single property: UserId string

Add XML docs. Use namespace FinTrack.Application.Queries.
Follow pattern from CreateExpenseCommand.
```

**Rationale:**
- **Specificity**: Type signature and return type explicit
- **Reference pattern**: "Follow pattern from CreateExpenseCommand"

**Output Quality:** ⭐⭐⭐⭐⭐ Excellent  
**Changes Made:** None

---

### Prompt #10: Create GetUserBalancesQueryHandler
**Feature:** Copilot Chat  
**Technique:** Specificity + Decomposition  
**Status:** ✅ USED

**Exact Prompt:**
```
Generate GetUserBalancesQueryHandler in FinTrack.Application.Handlers
implementing IRequestHandler<GetUserBalancesQuery, IEnumerable<UserBalanceDto>>.

1. Inject IExpenseRepository, IBalanceCalculationService, ILogger<>
2. Handle method:
   - Log info "Retrieving balances for user {UserId}"
   - Get user expenses: await _expenseRepository.GetByUserAsync(request.UserId, cancellationToken)
   - Calculate balances: _balanceService.CalculateNetBalances(request.UserId, userExpenses)
   - Convert to DTOs: select from balances dictionary to UserBalanceDto
   - Set Status based on NetAmount (>0 "Owed", <0 "Owes", ==0 "Settled")
   - Log info "Retrieved {Count} balance records for user {UserId}"
   - Return balance DTOs
3. Handle exceptions with try-catch, log error, re-throw
4. Add XML documentation

Follow pattern from CreateExpenseCommandHandler.
```

**Rationale:**
- **Decomposition**: Broke into numbered steps
- **Specificity**: Exact DTO mapping logic including Status determination
- **Reference pattern**: "Follow pattern from CreateExpenseCommandHandler"

**Output Quality:** ⭐⭐⭐⭐⭐ Excellent  
**Changes Made:** None

---

### Prompt #11: Generate Test Cases (Unit Tests)
**Feature:** Copilot Chat  
**Technique:** Specificity + Constraint-based  
**Status:** ✅ USED

**Exact Prompt:**
```
Generate xUnit test class ExpenseServiceTests in FinTrack.Tests/Unit/.

Requirements: 6 test cases using xUnit, Moq, FluentAssertions

Test Cases:
1. Handle_WithEqualSplit_ShouldCreateWithEqualShares
   - 3 participants, $120 expense
   - Assert: 3 participants, each $40, repository.AddAsync called once

2. Handle_WithCustomSplit_ShouldCreateWithCustomShares
   - Participants with custom amounts [$50, $40, $30] summing to $120
   - Assert: shares preserved, repository.AddAsync called once

3. Handle_WithInvalidCustomSplit_ShouldThrowInvalidExpenseException
   - Custom shares [$50, $40, $20] NOT summing to $120 (total $110)
   - Assert: throws InvalidExpenseException, repository NOT called

4. Handle_WithZeroAmount_ShouldThrowInvalidExpenseException
   - TotalAmount = 0
   - Assert: throws InvalidExpenseException

5. Handle_WithSingleParticipant_ShouldThrowInvalidExpenseException
   - Only 1 participant (needs at least 2)
   - Assert: throws InvalidExpenseException

6. Handle_WithUnauthorized_ShouldThrowUnauthorizedAccessException
   - User A creates expense for User B
   - Assert: throws UnauthorizedAccessException

Use Arrange-Act-Assert pattern. Mock repository. Use It.IsAny<> for verifications.
Namespace: FinTrack.Tests.Unit
```

**Rationale:**
- **Specificity**: Six exact test cases with input/expected output
- **Constraint-based**: "uses xUnit, Moq, FluentAssertions" and "Arrange-Act-Assert"
- **Few-shot**: Showed assertion examples

**Output Quality:** ⭐⭐⭐⭐ Good  
**Changes Made:** Added additional assertions for logging verification

---

### Prompt #12: Create Integration/Service Layer Tests
**Feature:** Copilot Chat  
**Technique:** Specificity + Role-based  
**Status:** ✅ USED

**Exact Prompt:**
```
You are a QA engineer testing fintech systems. Generate integration tests 
for TransactionService in FinTrack.Tests/Integration/.

Create 4 additional test cases:

1. GetUserTransactions_WithCurrentUserIdMatch_ShouldReturnTransactions
   - Mock repository to return 2 transactions
   - Call GetUserTransactionsAsync("user1", "user1")
   - Assert: returns 2 DTOs, matches data

2. GetUserTransactions_WithDifferentUserId_ShouldThrowUnauthorizedException
   - Call GetUserTransactionsAsync("user1", "user2")
   - Assert: throws UnauthorizedAccessException, logs warning

3. DeleteAllTransactions_WithConcurrentModification_ShouldHandleGracefully
   - Mock repository to throw DbUpdateException
   - Assert: throws DataAccessException with message

4. CreateTransaction_WithNegativeAmount_ShouldThrowInvalidTransactionException
   - Amount = -50
   - Assert: throws InvalidTransactionException

Use real TransactionService + mocked dependencies.
Namespace: FinTrack.Tests.Integration
```

**Rationale:**
- **Role-based**: "QA engineer testing fintech systems" emphasizes edge cases
- **Specificity**: Four test cases with exact scenarios
- **Constraint-based**: "real TransactionService + mocked dependencies" ensures integration-level testing

**Output Quality:** ⭐⭐⭐⭐ Good  
**Changes Made:** Enhanced mock setup for realistic scenarios

---

## Copilot Features Used

| Feature | Count | Purpose |
|---------|-------|---------|
| **Copilot Chat** | 12 | Multi-line prompts, complex logic generation |
| **Inline Code Completion** | ~8 | Property initialization, method signatures |
| **Comment-based Prompts** | 3 | Quick fixes and refinements |

---

## Prompting Techniques Summary

| Technique | Usage | Effectiveness |
|-----------|-------|----------------|
| **Specificity** | All 12 prompts | ⭐⭐⭐⭐⭐ Essential - exact types/constraints prevent ambiguity |
| **Decomposition** | 8 prompts | ⭐⭐⭐⭐⭐ Breaking into steps greatly improved code quality |
| **Few-shot Examples** | 9 prompts | ⭐⭐⭐⭐⭐ Showing format/patterns ensures consistency |
| **Constraint-based** | 10 prompts | ⭐⭐⭐⭐⭐ Explicit rules prevent anti-patterns |
| **Role-based** | 3 prompts | ⭐⭐⭐⭐ Sets context but less critical than specificity |
| **Iterative Refinement** | 2 prompts | ⭐⭐⭐⭐⭐ Essential for error handling and edge cases |

---

## Post-Generation Corrections

### 1. **BalanceCalculationService - Logic Error in Three-Way Calculation**

**What Copilot Generated:**
```csharp
else
{
    // Neither is creator, calculate based on who created
    balances[otherUserId] += userParticipant.ShareAmount - otherParticipant.ShareAmount;
}
```

**Problem:** 
When neither user is the creator, subtracting shares doesn't represent actual balance. This branch is logically incomplete in a true expense-splitting system.

**What Was Wrong:**
- Copilot tried to be clever but the three-way logic is edge-case
- The calculation doesn't match financial logic (if both paid, net should be zero)

**How It Was Fixed:**
Simplified to handle only two main cases:
- Creator pays, others owe
- Non-creator participant owes to creator

Removed the ambiguous "neither is creator" case, which simplified logic significantly.

---

### 2. **CreateExpenseValidator - Decimal Precision Check Missing**

**What Copilot Generated:**
```csharp
RuleFor(x => x.TotalAmount)
    .GreaterThan(0)
    .LessThanOrEqualTo(10000000)
    // DecimalPrecision not present
```

**Problem:**
Prompt specified `DecimalPrecision(2)` but Copilot didn't recognize this FluentValidation extension.

**What Was Wrong:**
- FluentValidation's DecimalPrecision validator exists but isn't commonly used
- Copilot didn't generate it

**How It Was Fixed:**
Added manual decimal precision validation:
```csharp
private bool HaveValidDecimalPrecision(decimal amount)
{
    var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(amount)[3])[2];
    return decimalPlaces <= 2;
}
```

This check ensures financial amounts never exceed 2 decimal places (cents).

---

### 3. **TransactionService - Missing Authorization Layer**

**What Copilot Generated:**
```csharp
public async Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(
    string userId, 
    CancellationToken cancellationToken = default)
{
    var transactions = await _repository.GetByUserAsync(userId, cancellationToken);
    return transactions.Select(...).ToList();
}
```

**Problem:**
Original version didn't check if the caller is authorized to see these transactions. This is critical in fintech.

**What Was Wrong:**
- Copilot generated happy-path code
- No consideration for multi-tenant security
- Missing the `currentUserId` parameter to verify ownership

**How It Was Fixed:**
Added authorization check:
```csharp
if (userId != currentUserId)
{
    throw new UnauthorizedAccessException(...);
}
```

This required injecting the current user ID from the HTTP context (done at controller level).

---

### 4. **CreateExpenseCommandHandler - No Validation Before Persistence**

**What Copilot Generated:**
```csharp
public async Task<ExpenseDto> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
{
    // Log
    var expense = new SharedExpense { ... };
    var participants = CalculateShares(request, expense);
    await _expenseRepository.AddAsync(expense, cancellationToken); // No validation check
}
```

**Problem:**
Validator was injected but never called before creating the expense.

**What Was Wrong:**
- Dead code pattern: validator injected but unused
- Invalid expenses could be persisted
- No specific error messages for API consumers

**How It Was Fixed:**
```csharp
var validationResult = await _validator.ValidateAsync(request, cancellationToken);
if (!validationResult.IsValid)
{
    var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
    throw new InvalidExpenseException($"Validation failed: {errors}");
}
```

---

### 5. **ExpenseRepository - Missing Error Handling on Batch Operations**

**What Copilot Generated:**
```csharp
public async Task DeleteAllByUserAsync(string userId, CancellationToken cancellationToken = default)
{
    var transactions = await _context.Transactions.ToListAsync(cancellationToken);
    _context.Transactions.RemoveRange(transactions);
    await _context.SaveChangesAsync(cancellationToken); // No error handling
}
```

**Problem:**
DbUpdateException can occur if concurrent modifications happen. No graceful error handling.

**What Was Wrong:**
- Unhandled exceptions crash the application
- Fintech operations need strong error reporting
- No logging of failures

**How It Was Fixed:**
```csharp
try
{
    // ... deletion logic
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Database error while deleting...");
    throw new DataAccessException($"Failed to delete: {ex.Message}");
}
```

---

### 6. **Test Cases - Incomplete Mock Setup**

**What Copilot Generated:**
```csharp
var mockRepository = new Mock<IExpenseRepository>();
// Missing: setup for mock returns
var handler = new CreateExpenseCommandHandler(mockRepository.Object, _mockLogger.Object);
```

**Problem:**
Mocks weren't configured to return data, so tests would fail at runtime.

**What Was Wrong:**
- Mock objects need `.Setup()` calls to return data
- Copilot generated incomplete test setup
- Tests wouldn't compile or run

**How It Was Fixed:**
```csharp
var mockRepository = new Mock<IExpenseRepository>();
mockRepository
    .Setup(r => r.AddAsync(It.IsAny<SharedExpense>(), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);

var handler = new CreateExpenseCommandHandler(
    mockRepository.Object, 
    _mockLogger.Object, 
    _validator.Object);
```

---

## Key Learnings

### What AI Excels At
✅ **Boilerplate code** - Service layer structure, repository patterns, DTOs  
✅ **Consistent formatting** - Following established patterns when shown examples  
✅ **Large code volumes** - Generating 50+ line files accurately  
✅ **CQRS patterns** - Commands, Handlers, Queries follow predictable structure  

### What Requires Human Oversight
❌ **Security/Authorization** - AI doesn't automatically think about access control  
❌ **Error handling** - AI generates happy-path; edge cases need manual addition  
❌ **Business logic verification** - Financial calculations need domain expert review  
❌ **Test completeness** - Mock setup and edge cases need human thinking  
❌ **Fintech constraints** - Decimal precision, audit trails, compliance requirements  

### Prompting Best Practices (Proven Effective)
1. **Be Extremely Specific** - Include exact property names, types, and limits
2. **Show Examples** - One or two lines of desired code format
3. **List Constraints** - Use numbered lists for rules and requirements
4. **Reference Patterns** - "Follow pattern from [existing class]"
5. **Define Roles** - "You are a fintech expert" improves context awareness
6. **Separate Concerns** - Ask for one class/feature per prompt
7. **Iterate on Refinement** - Prompt 1 → generate → Prompt 2 to improve

---

**Total Time Saved:** ~4-5 hours of boilerplate code  
**Manual Review/Fixes:** ~2-3 hours  
**AI-Assisted Development:** ~40% faster than hand-coding  
**Code Quality:** Production-ready after corrections

