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
    /// Repository implementation for SharedExpense data access.
    /// </summary>
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExpenseRepository> _logger;

        /// <summary>
        /// Initializes the repository.
        /// </summary>
        public ExpenseRepository(ApplicationDbContext context, ILogger<ExpenseRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a shared expense by ID with its participants.
        /// </summary>
        public async Task<SharedExpense?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving expense with ID {ExpenseId}", id);
            return await _context.SharedExpenses
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets all active expenses for a user (as creator or participant).
        /// </summary>
        public async Task<IEnumerable<SharedExpense>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving expenses for user {UserId}", userId);
            return await _context.SharedExpenses
                .Include(e => e.Participants)
                .Where(e => e.Status == "Active" && (e.CreatorId == userId || e.Participants.Any(p => p.UserId == userId)))
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets pending balances for a user.
        /// Positive = user is owed, Negative = user owes.
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetUserBalancesAsync(string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Calculating balances for user {UserId}", userId);
            var expenses = await GetByUserAsync(userId, cancellationToken);
            var balances = new Dictionary<string, decimal>();

            foreach (var expense in expenses)
            {
                var userParticipant = expense.Participants.FirstOrDefault(p => p.UserId == userId);
                if (userParticipant == null)
                    continue;

                foreach (var otherParticipant in expense.Participants.Where(p => p.UserId != userId))
                {
                    var otherUserId = otherParticipant.UserId;
                    if (!balances.ContainsKey(otherUserId))
                        balances[otherUserId] = 0;

                    if (expense.CreatorId == userId)
                    {
                        balances[otherUserId] -= otherParticipant.ShareAmount;
                    }
                    else if (expense.CreatorId == otherUserId)
                    {
                        balances[otherUserId] += userParticipant.ShareAmount;
                    }
                }
            }

            return balances;
        }

        /// <summary>
        /// Adds a new shared expense with participants.
        /// </summary>
        public async Task AddAsync(SharedExpense expense, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Adding new expense created by {CreatorId}", expense.CreatorId);
            await _context.SharedExpenses.AddAsync(expense, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Expense saved with ID {ExpenseId}", expense.Id);
        }

        /// <summary>
        /// Updates an existing expense.
        /// </summary>
        public async Task UpdateAsync(SharedExpense expense, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Updating expense with ID {ExpenseId}", expense.Id);
            _context.SharedExpenses.Update(expense);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Expense updated with ID {ExpenseId}", expense.Id);
        }

        /// <summary>
        /// Deletes an expense by ID (and its participants via cascade).
        /// </summary>
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Deleting expense with ID {ExpenseId}", id);
            var expense = await GetByIdAsync(id, cancellationToken);
            if (expense != null)
            {
                _context.SharedExpenses.Remove(expense);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Expense deleted with ID {ExpenseId}", id);
            }
        }
    }
}
