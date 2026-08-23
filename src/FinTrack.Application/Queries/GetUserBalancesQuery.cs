using MediatR;
using FinTrack.Application.DTOs;

namespace FinTrack.Application.Queries
{
    /// <summary>
    /// Query to retrieve pending balances for a user.
    /// </summary>
    public class GetUserBalancesQuery : IRequest<List<UserBalanceDto>>
    {
        /// <summary>User ID to get balances for.</summary>
        public string UserId { get; set; } = string.Empty;
    }
}
