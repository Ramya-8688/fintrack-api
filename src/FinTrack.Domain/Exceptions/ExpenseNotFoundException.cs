using System;

namespace FinTrack.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when expense is not found.
    /// </summary>
    public class ExpenseNotFoundException : Exception
    {
        /// <summary>Error code for API responses.</summary>
        public string Code { get; } = "EXPENSE_NOT_FOUND";

        /// <summary>HTTP status code (404).</summary>
        public int HttpStatusCode { get; } = 404;

        /// <summary>
        /// Creates an ExpenseNotFoundException.
        /// </summary>
        /// <param name="expenseId">ID of expense that was not found.</param>
        public ExpenseNotFoundException(int expenseId) 
            : base($"Expense with ID {expenseId} not found") { }

        /// <summary>
        /// Creates an ExpenseNotFoundException with custom message.
        /// </summary>
        /// <param name="message">Error message.</param>
        public ExpenseNotFoundException(string message) : base(message) { }
    }
}
