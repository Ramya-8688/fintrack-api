namespace FinTrack.Application.DTOs
{
    /// <summary>
    /// Data transfer object for Transaction entity.
    /// </summary>
    public class TransactionDto
    {
        /// <summary>Transaction ID.</summary>
        public int Id { get; set; }

        /// <summary>User ID.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Transaction type.</summary>
        public string TransactionType { get; set; } = string.Empty;

        /// <summary>Amount.</summary>
        public decimal Amount { get; set; }

        /// <summary>Description.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Status.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Created date.</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>Related expense ID.</summary>
        public int? SharedExpenseId { get; set; }
    }
}
