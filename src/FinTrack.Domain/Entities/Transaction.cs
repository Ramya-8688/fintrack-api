using System;

namespace FinTrack.Domain.Entities
{
    /// <summary>
    /// Represents a financial transaction between users.
    /// Immutable record for audit trail and compliance.
    /// </summary>
    public class Transaction
    {
        /// <summary>Unique transaction identifier.</summary>
        public int Id { get; set; }

        /// <summary>User who initiated the transaction.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Type of transaction (Payment, Refund, Split).</summary>
        public string TransactionType { get; set; } = string.Empty;

        /// <summary>Transaction amount in USD.</summary>
        public decimal Amount { get; set; }

        /// <summary>Human-readable transaction description.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Transaction status (Pending, Completed, Failed).</summary>
        public string Status { get; set; } = "Completed";

        /// <summary>When the transaction was recorded (UTC).</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>Related shared expense ID (if applicable).</summary>
        public int? SharedExpenseId { get; set; }

        /// <summary>
        /// Validates the transaction has required fields.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(UserId) 
                && Amount > 0 
                && !string.IsNullOrWhiteSpace(TransactionType)
                && !string.IsNullOrWhiteSpace(Description);
        }
    }
}
