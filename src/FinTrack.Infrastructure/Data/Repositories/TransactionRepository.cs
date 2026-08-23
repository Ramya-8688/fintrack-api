using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Exceptions;
using FinTrack.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Transaction data access with error handling.
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
        
        try
        {
            return await _context.Transactions.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction with ID {TransactionId}", id);
            throw new DataAccessException($"Failed to retrieve transaction: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all transactions for a user, ordered by date descending.
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving transactions for user {UserId}", userId);
        
        try
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation cancelled while retrieving transactions for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for user {UserId}", userId);
            throw new DataAccessException($"Failed to retrieve transactions: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds a new transaction.
    /// </summary>
    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding new transaction for user {UserId}", transaction.UserId);
        
        try
        {
            await _context.Transactions.AddAsync(transaction, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Transaction saved with ID {TransactionId}", transaction.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while adding transaction for user {UserId}", transaction.UserId);
            throw new DataAccessException($"Failed to add transaction: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation cancelled while adding transaction for user {UserId}", transaction.UserId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a transaction by ID.
    /// </summary>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting transaction with ID {TransactionId}", id);
        
        try
        {
            var transaction = await GetByIdAsync(id, cancellationToken);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Transaction deleted with ID {TransactionId}", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent transaction with ID {TransactionId}", id);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting transaction with ID {TransactionId}", id);
            throw new DataAccessException($"Failed to delete transaction: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation cancelled while deleting transaction with ID {TransactionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes all transactions for a user with error handling.
    /// </summary>
    public async Task DeleteAllByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting all transactions for user {UserId}", userId);
        
        try
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync(cancellationToken);
            
            if (!transactions.Any())
            {
                _logger.LogInformation("No transactions found for user {UserId} to delete", userId);
                return;
            }

            _context.Transactions.RemoveRange(transactions);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully deleted {Count} transactions for user {UserId}", 
                transactions.Count, userId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting transactions for user {UserId}", userId);
            throw new DataAccessException($"Failed to delete transactions for user {userId}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Delete operation cancelled for user {UserId}", userId);
            throw;
        }
    }
}
