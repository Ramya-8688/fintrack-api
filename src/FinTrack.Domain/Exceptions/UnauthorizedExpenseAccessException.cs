using System;

namespace FinTrack.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when user attempts unauthorized access to expense.
    /// </summary>
    public class UnauthorizedExpenseAccessException : Exception
    {
        /// <summary>Error code for API responses.</summary>
        public string Code { get; } = "UNAUTHORIZED_ACCESS";

        /// <summary>HTTP status code (403).</summary>
        public int HttpStatusCode { get; } = 403;

        /// <summary>
        /// Creates an UnauthorizedExpenseAccessException.
        /// </summary>
        /// <param name="message">Error message.</param>
        public UnauthorizedExpenseAccessException(string message) : base(message) { }

        /// <summary>
        /// Creates an UnauthorizedExpenseAccessException with inner exception.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Inner exception.</param>
        public UnauthorizedExpenseAccessException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
