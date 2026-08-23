using MediatR;
using FinTrack.Application.DTOs;

namespace FinTrack.Application.Commands
{
    /// <summary>
    /// Command to create a new transaction.
    /// </summary>
    public class CreateTransactionCommand : IRequest<TransactionDto>
    {
        /// <summary>User ID.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Transaction type (Payment, Refund, Split).</summary>
        public string TransactionType { get; set; } = string.Empty;

        /// <summary>Transaction amount.</summary>
        public decimal Amount { get; set; }

        /// <summary>Transaction description.</summary>
        public string Description { get; set; } = string.Empty;
    }
}
