using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTrack.Domain.Entities;

namespace FinTrack.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for SharedExpense data access operations.
    /// </summary>
    public interface IExpenseRepository
    {
        /// <summary>
        /// Gets a shared expense by its ID.
        /// </summary>
        /// <param name="id">Expense ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>SharedExpense if found, null otherwise.</returns>
        Task<SharedExpense?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all active expenses for a user (creator or participant).
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of user's expenses.</returns>
        Task<IEnumerable<SharedExpense>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pending balances for a user (what they owe or are owed).
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Dictionary of user IDs to net amount owed (negative = user owes).</returns>
        Task<Dictionary<string, decimal>> GetUserBalancesAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new shared expense.
        /// </summary>
        /// <param name="expense">Expense to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Completed task.</returns>
        Task AddAsync(SharedExpense expense, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing expense.
        /// </summary>
        /// <param name="expense">Expense to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Completed task.</returns>
        Task UpdateAsync(SharedExpense expense, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an expense by ID.
        /// </summary>
        /// <param name="id">Expense ID to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Completed task.</returns>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
