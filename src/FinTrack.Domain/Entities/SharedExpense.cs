using System;
using System.Collections.Generic;

namespace FinTrack.Domain.Entities
{
    /// <summary>
    /// Represents a shared expense split among multiple users.
    /// Tracks who paid and who owes what.
    /// </summary>
    public class SharedExpense
    {
        /// <summary>Unique expense identifier.</summary>
        public int Id { get; set; }

        /// <summary>User who created/initiated the expense.</summary>
        public string CreatorId { get; set; } = string.Empty;

        /// <summary>Expense description (e.g., "Dinner", "Rent").</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Total expense amount in USD.</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Split type: "Equal" or "Custom".</summary>
        public string SplitType { get; set; } = "Equal";

        /// <summary>Status of the expense (Active, Settled, Cancelled).</summary>
        public string Status { get; set; } = "Active";

        /// <summary>When the expense was created (UTC).</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>List of participants and their shares.</summary>
        public List<ExpenseParticipant> Participants { get; set; } = new();

        /// <summary>
        /// Validates the expense has required fields and valid participants.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(CreatorId)
                && !string.IsNullOrWhiteSpace(Description)
                && TotalAmount > 0
                && Participants.Count >= 2
                && Participants.TrueForAll(p => p.ShareAmount > 0);
        }

        /// <summary>
        /// Calculates if all shares sum to the total amount (with 0.01 tolerance for rounding).
        /// </summary>
        /// <returns>True if sums match, false otherwise.</returns>
        public bool ValidateSharesSum()
        {
            var sum = Participants.Sum(p => p.ShareAmount);
            return Math.Abs(sum - TotalAmount) < 0.01m;
        }
    }
}
