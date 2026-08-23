using MediatR;
using Microsoft.Extensions.Logging;
using FinTrack.Application.DTOs;
using FinTrack.Application.Queries;
using FinTrack.Domain.Interfaces;
using AutoMapper;

namespace FinTrack.Application.Handlers
{
    /// <summary>
    /// Handler for GetUserBalancesQuery.
    /// </summary>
    public class GetUserBalancesQueryHandler : IRequestHandler<GetUserBalancesQuery, List<UserBalanceDto>>
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly ILogger<GetUserBalancesQueryHandler> _logger;

        /// <summary>
        /// Initializes the handler with dependencies.
        /// </summary>
        public GetUserBalancesQueryHandler(
            IExpenseRepository expenseRepository,
            ILogger<GetUserBalancesQueryHandler> logger)
        {
            _expenseRepository = expenseRepository ?? throw new ArgumentNullException(nameof(expenseRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the get user balances query.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of balance DTOs showing who user owes/is owed.</returns>
        public async Task<List<UserBalanceDto>> Handle(GetUserBalancesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving balances for user {UserId}", request.UserId);

            var balances = await _expenseRepository.GetUserBalancesAsync(request.UserId, cancellationToken);

            var result = balances
                .Where(b => b.Value != 0)
                .Select(b => new UserBalanceDto
                {
                    UserId = b.Key,
                    NetAmount = b.Value
                })
                .OrderByDescending(b => Math.Abs(b.NetAmount))
                .ToList();

            _logger.LogInformation("Retrieved {BalanceCount} balance records for user {UserId}", result.Count, request.UserId);
            return result;
        }
    }
}
