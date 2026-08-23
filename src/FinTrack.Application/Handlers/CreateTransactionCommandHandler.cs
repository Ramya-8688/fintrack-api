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
    /// Handler for CreateTransactionCommand.
    /// </summary>
    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
    {
        private readonly ITransactionRepository _repository;
        private readonly ILogger<CreateTransactionCommandHandler> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes the handler with dependencies.
        /// </summary>
        public CreateTransactionCommandHandler(
            ITransactionRepository repository,
            ILogger<CreateTransactionCommandHandler> logger,
            IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Handles the create transaction command.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created transaction DTO.</returns>
        /// <exception cref="InvalidOperationException">Thrown if validation fails.</exception>
        public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating transaction for user {UserId} with amount {Amount}", 
                request.UserId, request.Amount);

            var transaction = new Transaction
            {
                UserId = request.UserId,
                TransactionType = request.TransactionType,
                Amount = request.Amount,
                Description = request.Description,
                Status = "Completed",
                CreatedDate = DateTime.UtcNow
            };

            if (!transaction.IsValid())
            {
                _logger.LogWarning("Transaction validation failed for user {UserId}", request.UserId);
                throw new InvalidOperationException("Transaction validation failed. Missing required fields.");
            }

            await _repository.AddAsync(transaction, cancellationToken);
            _logger.LogInformation("Transaction created successfully with ID {TransactionId}", transaction.Id);

            return _mapper.Map<TransactionDto>(transaction);
        }
    }
}
