namespace FinTrack.Application.DTOs
{
    /// <summary>
    /// Data transfer object for User entity.
    /// </summary>
    public class UserDto
    {
        /// <summary>User ID.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Full name.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Account creation date.</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>Whether account is active.</summary>
        public bool IsActive { get; set; }
    }
}
