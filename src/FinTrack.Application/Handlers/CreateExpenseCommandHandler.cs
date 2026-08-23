using MediatR;
using Microsoft.Extensions.Logging;
using FinTrack.Application.Commands;
using FinTrack.Application.DTOs;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Exceptions;
using FinTrack.Domain.Interfaces;
using AutoMapper;

namespace FinTrack.Application.Handlers
{
    /// <summary>
    /// Handler for CreateExpenseCommand.
    /// </summary>
    public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, ExpenseDto>
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly ILogger<CreateExpenseCommandHandler> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes the handler with dependencies.
        /// </summary>
        public CreateExpenseCommandHandler(
            IExpenseRepository expenseRepository,
            ILogger<CreateExpenseCommandHandler> logger,
            IMapper mapper)
        {
            _expenseRepository = expenseRepository ?? throw new ArgumentNullException(nameof(expenseRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Handles the create expense command.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created expense DTO.</returns>
        /// <exception cref="InvalidExpenseException">Thrown if validation fails.</exception>
        public async Task<ExpenseDto> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating expense by user {CreatorId} with amount {Amount} and {ParticipantCount} participants", 
                request.CreatorId, request.TotalAmount, request.Participants.Count);

            ValidateExpenseRequest(request);

            var expense = new SharedExpense
            {
                CreatorId = request.CreatorId,
                Description = request.Description,
                TotalAmount = request.TotalAmount,
                SplitType = request.SplitType,
                Status = "Active",
                CreatedDate = DateTime.UtcNow
            };

            expense.Participants = CalculateShares(request, expense);

            if (!expense.IsValid())
            {
                _logger.LogWarning("Expense validation failed for creator {CreatorId}", request.CreatorId);
                throw new InvalidExpenseException("Expense validation failed. Invalid data.");
            }

            await _expenseRepository.AddAsync(expense, cancellationToken);
            _logger.LogInformation("Expense created successfully with ID {ExpenseId} and {ParticipantCount} participants", 
                expense.Id, expense.Participants.Count);

            return _mapper.Map<ExpenseDto>(expense);
        }

        /// <summary>
        /// Validates the expense creation request.
        /// </summary>
        private void ValidateExpenseRequest(CreateExpenseCommand request)
        {
            if (request.TotalAmount <= 0)
                throw new InvalidExpenseException("Total amount must be greater than 0");

            if (request.Participants.Count < 2)
                throw new InvalidExpenseException("Expense must have at least 2 participants");

            if (request.Participants.Count > 100)
                throw new InvalidExpenseException("Expense cannot have more than 100 participants");

            if (request.SplitType == "Custom")
            {
                var customSum = request.Participants.Sum(p => p.ShareAmount);
                if (Math.Abs(customSum - request.TotalAmount) >= 0.01m)
                    throw new InvalidExpenseException("Custom shares must sum to total amount");
            }
        }

        /// <summary>
        /// Calculates share amounts for participants based on split type.
        /// </summary>
        private List<ExpenseParticipant> CalculateShares(CreateExpenseCommand request, SharedExpense expense)
        {
            if (request.SplitType == "Equal")
            {
                var shareAmount = request.TotalAmount / request.Participants.Count;
                return request.Participants.Select(p => new ExpenseParticipant
                {
                    UserId = p.UserId,
                    ShareAmount = Math.Round(shareAmount, 2),
                    Status = "Pending",
                    CreatedDate = DateTime.UtcNow
                }).ToList();
            }
            else // Custom
            {
                return request.Participants.Select(p => new ExpenseParticipant
                {
                    UserId = p.UserId,
                    ShareAmount = p.ShareAmount,
                    Status = "Pending",
                    CreatedDate = DateTime.UtcNow
                }).ToList();
            }
        }
    }
}
