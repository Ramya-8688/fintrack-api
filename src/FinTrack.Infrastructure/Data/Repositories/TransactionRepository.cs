using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository implementation for Transaction data access.
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionRepository> _logger;

        /// <summary>
        /// Initializes the repository.
        /// </summary>
        public TransactionRepository(ApplicationDbContext context, ILogger<TransactionRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a transaction by ID.
        /// </summary>
        public async Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving transaction with ID {TransactionId}", id);
            return await _context.Transactions.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets all transactions for a user, ordered by date descending.
        /// </summary>
        public async Task<IEnumerable<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving transactions for user {UserId}", userId);
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Adds a new transaction.
        /// </summary>
        public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Adding new transaction for user {UserId}", transaction.UserId);
            await _context.Transactions.AddAsync(transaction, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Transaction saved with ID {TransactionId}", transaction.Id);
        }

        /// <summary>
        /// Deletes a transaction by ID.
        /// </summary>
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Deleting transaction with ID {TransactionId}", id);
            var transaction = await GetByIdAsync(id, cancellationToken);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Transaction deleted with ID {TransactionId}", id);
            }
        }

        /// <summary>
        /// Deletes all transactions for a user.
        /// </summary>
        public async Task DeleteAllByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting all transactions for user {UserId}", userId);
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync(cancellationToken);
            
            _context.Transactions.RemoveRange(transactions);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Deleted {TransactionCount} transactions for user {UserId}", transactions.Count, userId);
        }
    }
}
