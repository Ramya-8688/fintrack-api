using System;

namespace FinTrack.Domain.Entities
{
    /// <summary>
    /// Represents a FinTrack user account.
    /// </summary>
    public class User
    {
        /// <summary>Unique user identifier.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>User's email address (unique).</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>User's full name.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Hashed password (bcrypt).</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Account creation date (UTC).</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>Last login date (UTC).</summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>Whether the account is active.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Validates required user fields.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Id)
                && !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(FullName)
                && !string.IsNullOrWhiteSpace(PasswordHash);
        }
    }
}
