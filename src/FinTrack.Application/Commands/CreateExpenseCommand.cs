using MediatR;
using FinTrack.Application.DTOs;

namespace FinTrack.Application.Commands
{
    /// <summary>
    /// Command to create a new shared expense.
    /// </summary>
    public class CreateExpenseCommand : IRequest<ExpenseDto>
    {
        /// <summary>ID of user creating the expense.</summary>
        public string CreatorId { get; set; } = string.Empty;

        /// <summary>Expense description.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Total expense amount.</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Type of split: Equal or Custom.</summary>
        public string SplitType { get; set; } = "Equal";

        /// <summary>List of participants.</summary>
        public List<ParticipantDto> Participants { get; set; } = new();
    }
}
