using System;

namespace FinTrack.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when expense validation fails.
    /// </summary>
    public class InvalidExpenseException : Exception
    {
        /// <summary>Error code for API responses.</summary>
        public string Code { get; } = "INVALID_EXPENSE";

        /// <summary>HTTP status code (400).</summary>
        public int HttpStatusCode { get; } = 400;

        /// <summary>
        /// Creates an InvalidExpenseException.
        /// </summary>
        /// <param name="message">Error message.</param>
        public InvalidExpenseException(string message) : base(message) { }

        /// <summary>
        /// Creates an InvalidExpenseException with inner exception.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Inner exception.</param>
        public InvalidExpenseException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
