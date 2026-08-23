using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTrack.Domain.Entities;

namespace FinTrack.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Transaction data access operations.
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Gets a transaction by its ID.
        /// </summary>
        /// <param name="id">Transaction ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Transaction if found, null otherwise.</returns>
        Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all transactions for a specific user.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of user's transactions ordered by date descending.</returns>
        Task<IEnumerable<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new transaction.
        /// </summary>
        /// <param name="transaction">Transaction to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Completed task.</returns>
        Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a transaction by ID.
        /// </summary>
        /// <param name="id">Transaction ID to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Completed task.</returns>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all transactions for a user (admin only).
        /// </summary>
        /// <param name="userId">User ID whose transactions to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Completed task.</returns>
        Task DeleteAllByUserAsync(string userId, CancellationToken cancellationToken = default);
    }
}
