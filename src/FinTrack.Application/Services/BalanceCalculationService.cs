using FinTrack.Application.DTOs;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinTrack.Application.Services
{
    /// <summary>
    /// Service for balance calculation operations.
    /// Computes net balances between users from shared expenses.
    /// </summary>
    public interface IBalanceCalculationService
    {
        /// <summary>
        /// Calculates net balances for a user across all expenses.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="expenses">Collection of expenses the user is involved in.</param>
        /// <returns>Dictionary of user IDs to net amounts.</returns>
        Dictionary<string, decimal> CalculateNetBalances(string userId, IEnumerable<SharedExpense> expenses);
    }

    /// <summary>
    /// Implementation of IBalanceCalculationService.
    /// </summary>
    public class BalanceCalculationService : IBalanceCalculationService
    {
        private readonly ILogger<BalanceCalculationService> _logger;

        /// <summary>
        /// Initializes the service with dependencies.
        /// </summary>
        public BalanceCalculationService(ILogger<BalanceCalculationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calculates net balances for a user.
        /// Positive value = user is owed, Negative value = user owes.
        /// </summary>
        public Dictionary<string, decimal> CalculateNetBalances(string userId, IEnumerable<SharedExpense> expenses)
        {
            _logger.LogInformation("Calculating net balances for user {UserId}", userId);

            var balances = new Dictionary<string, decimal>();

            foreach (var expense in expenses.Where(e => e.Status == "Active"))
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
                        // User is creator, others owe them
                        balances[otherUserId] -= otherParticipant.ShareAmount;
                    }
                    else if (expense.CreatorId == otherUserId)
                    {
                        // Other user is creator, current user owes them
                        balances[otherUserId] += userParticipant.ShareAmount;
                    }
                    else
                    {
                        // Neither is creator, calculate based on who created
                        balances[otherUserId] += userParticipant.ShareAmount - otherParticipant.ShareAmount;
                    }
                }
            }

            _logger.LogDebug("Calculated balances for user {UserId}: {@Balances}", userId, balances);
            return balances;
        }
    }
}
