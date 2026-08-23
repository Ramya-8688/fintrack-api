using FluentValidation;
using FinTrack.Application.Commands;

namespace FinTrack.Application.Validators
{
    /// <summary>
    /// Validator for CreateTransactionCommand.
    /// </summary>
    public class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
    {
        /// <summary>
        /// Configures validation rules.
        /// </summary>
        public CreateTransactionValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .Length(1, 100).WithMessage("User ID must be between 1 and 100 characters");

            RuleFor(x => x.TransactionType)
                .NotEmpty().WithMessage("Transaction type is required")
                .Must(x => new[] { "Payment", "Refund", "Split" }.Contains(x))
                .WithMessage("Transaction type must be Payment, Refund, or Split");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0")
                .LessThanOrEqualTo(10000000).WithMessage("Amount cannot exceed $10,000,000")
                .DecimalPrecision(2).WithMessage("Amount cannot have more than 2 decimal places");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .Length(3, 500).WithMessage("Description must be between 3 and 500 characters");
        }
    }
}
