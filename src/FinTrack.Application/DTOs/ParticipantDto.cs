namespace FinTrack.Application.DTOs
{
    /// <summary>
    /// Data transfer object for ExpenseParticipant entity.
    /// </summary>
    public class ParticipantDto
    {
        /// <summary>Participant user ID.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Amount the participant owes or is owed.</summary>
        public decimal ShareAmount { get; set; }

        /// <summary>Participant status.</summary>
        public string? Status { get; set; }
    }
}
