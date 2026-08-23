namespace FinTrack.Application.DTOs
{
    /// <summary>
    /// Data transfer object for SharedExpense entity.
    /// </summary>
    public class ExpenseDto
    {
        /// <summary>Expense ID.</summary>
        public int Id { get; set; }

        /// <summary>Creator user ID.</summary>
        public string CreatorId { get; set; } = string.Empty;

        /// <summary>Expense description.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Total amount.</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Split type (Equal or Custom).</summary>
        public string SplitType { get; set; } = string.Empty;

        /// <summary>Expense status.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Created date.</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>List of participants.</summary>
        public List<ParticipantDto> Participants { get; set; } = new();
    }
}
