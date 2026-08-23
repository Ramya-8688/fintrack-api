namespace FinTrack.Application.DTOs
{
    /// <summary>
    /// Data transfer object for user balance information.
    /// </summary>
    public class UserBalanceDto
    {
        /// <summary>Other user ID.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Net amount: positive = user is owed, negative = user owes.</summary>
        public decimal NetAmount { get; set; }
    }
}
