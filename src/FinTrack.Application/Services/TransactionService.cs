using FinTrack.Application.DTOs;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinTrack.Application.Services
{
    /// <summary>
    /// Service for transaction-related operations.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        /// <param name="transaction">Transaction to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created transaction DTO.</returns>
        Task<TransactionDto> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all transactions for a user.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of transaction DTOs.</returns>
        Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(string userId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementation of ITransactionService.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repository;
        private readonly ILogger<TransactionService> _logger;

        /// <summary>
        /// Initializes the service with dependencies.
        /// </summary>
        public TransactionService(
            ITransactionRepository repository,
            ILogger<TransactionService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        public async Task<TransactionDto> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating transaction for user {UserId} with amount {Amount}", 
                transaction.UserId, transaction.Amount);

            await _repository.AddAsync(transaction, cancellationToken);
            
            return new TransactionDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                TransactionType = transaction.TransactionType,
                Amount = transaction.Amount,
                Description = transaction.Description,
                Status = transaction.Status,
                CreatedDate = transaction.CreatedDate
            };
        }

        /// <summary>
        /// Retrieves all transactions for a user.
        /// </summary>
        public async Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving transactions for user {UserId}", userId);

            var transactions = await _repository.GetByUserAsync(userId, cancellationToken);

            return transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                UserId = t.UserId,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                Description = t.Description,
                Status = t.Status,
                CreatedDate = t.CreatedDate
            }).ToList();
        }
    }
}
