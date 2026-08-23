using FinTrack.Application.DTOs;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Exceptions;
using FinTrack.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MediatR;

namespace FinTrack.Application.Handlers;

/// <summary>
/// CQRS Command Handler for creating shared expenses.
/// Validates input, calculates shares, and persists to repository.
/// </summary>
public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, ExpenseDto>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger<CreateExpenseCommandHandler> _logger;
    private readonly IValidator<CreateExpenseCommand> _validator;

    /// <summary>
    /// Initializes the handler.
    /// </summary>
    public CreateExpenseCommandHandler(
        IExpenseRepository expenseRepository,
        ILogger<CreateExpenseCommandHandler> logger,
        IValidator<CreateExpenseCommand> validator)
    {
        _expenseRepository = expenseRepository ?? throw new ArgumentNullException(nameof(expenseRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Handles the create expense command.
    /// </summary>
    public async Task<ExpenseDto> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating expense by user {CreatorId} with amount {Amount}",
            request.CreatorId, request.TotalAmount);

        // Validate command
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Expense validation failed: {Errors}", errors);
            throw new InvalidExpenseException($"Validation failed: {errors}");
        }

        try
        {
            // Create expense entity
            var expense = new SharedExpense
            {
                CreatorId = request.CreatorId,
                Description = request.Description,
                TotalAmount = request.TotalAmount,
                SplitType = request.SplitType,
                Status = "Active",
                CreatedDate = DateTime.UtcNow
            };

            // Calculate shares based on split type
            var participants = CalculateShares(request, expense);
            expense.Participants = participants;

            // Validate shares sum correctly
            if (!expense.ValidateSharesSum())
            {
                _logger.LogWarning("Share validation failed for expense: shares don't sum to total");
                throw new InvalidExpenseException("Participant shares do not sum to total amount");
            }

            // Persist to repository
            await _expenseRepository.AddAsync(expense, cancellationToken);

            _logger.LogInformation("Expense created successfully with ID {ExpenseId} by user {CreatorId}",
                expense.Id, request.CreatorId);

            return MapToDto(expense);
        }
        catch (InvalidExpenseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense for user {CreatorId}", request.CreatorId);
            throw new InvalidExpenseException($"Failed to create expense: {ex.Message}");
        }
    }

    /// <summary>
    /// Calculates participant shares based on split type.
    /// </summary>
    private List<ExpenseParticipant> CalculateShares(CreateExpenseCommand request, SharedExpense expense)
    {
        if (request.SplitType == "Equal")
        {
            var shareAmount = request.TotalAmount / request.Participants.Count;
            _logger.LogDebug("Calculating equal shares: {ShareAmount} per participant", shareAmount);

            return request.Participants.Select(p => new ExpenseParticipant
            {
                UserId = p.UserId,
                ShareAmount = shareAmount,
                Status = "Pending"
            }).ToList();
        }
        else if (request.SplitType == "Custom")
        {
            var totalShares = request.Participants.Sum(p => p.ShareAmount ?? 0);
            _logger.LogDebug("Validating custom shares: total {Total} vs expense {Expense}",
                totalShares, request.TotalAmount);

            if (Math.Abs(totalShares - request.TotalAmount) > 0.01m)
            {
                throw new InvalidExpenseException(
                    $"Custom shares (${totalShares:N2}) do not match total amount (${request.TotalAmount:N2})");
            }

            return request.Participants.Select(p => new ExpenseParticipant
            {
                UserId = p.UserId,
                ShareAmount = p.ShareAmount ?? 0,
                Status = "Pending"
            }).ToList();
        }
        else
        {
            throw new InvalidExpenseException($"Unknown split type: {request.SplitType}");
        }
    }

    /// <summary>
    /// Maps SharedExpense entity to DTO.
    /// </summary>
    private static ExpenseDto MapToDto(SharedExpense expense)
    {
        return new ExpenseDto
        {
            Id = expense.Id,
            CreatorId = expense.CreatorId,
            Description = expense.Description,
            TotalAmount = expense.TotalAmount,
            SplitType = expense.SplitType,
            Status = expense.Status,
            Participants = expense.Participants.Select(p => new ParticipantDto
            {
                UserId = p.UserId,
                ShareAmount = p.ShareAmount
            }).ToList(),
            CreatedDate = expense.CreatedDate
        };
    }
}
