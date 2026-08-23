using System;

namespace FinTrack.Domain.Entities
{
    /// <summary>
    /// Represents a participant in a shared expense.
    /// Tracks how much each user owes or is owed.
    /// </summary>
    public class ExpenseParticipant
    {
        /// <summary>Unique participant record identifier.</summary>
        public int Id { get; set; }

        /// <summary>Foreign key to shared expense.</summary>
        public int SharedExpenseId { get; set; }

        /// <summary>ID of the user participating in the expense.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Amount this user owes or is owed in this expense.</summary>
        public decimal ShareAmount { get; set; }

        /// <summary>Participant status: "Pending", "Paid", "Settled".</summary>
        public string Status { get; set; } = "Pending";

        /// <summary>When the participant was added to the expense (UTC).</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>Navigation property to parent SharedExpense.</summary>
        public SharedExpense? SharedExpense { get; set; }
    }
}
