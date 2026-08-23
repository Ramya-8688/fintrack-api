using FluentValidation;
using FinTrack.Application.Commands;

namespace FinTrack.Application.Validators
{
    /// <summary>
    /// Validator for CreateExpenseCommand.
    /// </summary>
    public class CreateExpenseValidator : AbstractValidator<CreateExpenseCommand>
    {
        /// <summary>
        /// Configures validation rules.
        /// </summary>
        public CreateExpenseValidator()
        {
            RuleFor(x => x.CreatorId)
                .NotEmpty().WithMessage("Creator ID is required")
                .Length(1, 100).WithMessage("Creator ID must be between 1 and 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .Length(3, 500).WithMessage("Description must be between 3 and 500 characters");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0")
                .LessThanOrEqualTo(10000000).WithMessage("Amount cannot exceed $10,000,000")
                .DecimalPrecision(2).WithMessage("Amount cannot have more than 2 decimal places");

            RuleFor(x => x.SplitType)
                .NotEmpty().WithMessage("Split type is required")
                .Must(x => new[] { "Equal", "Custom" }.Contains(x))
                .WithMessage("Split type must be Equal or Custom");

            RuleFor(x => x.Participants)
                .NotEmpty().WithMessage("Participants list cannot be empty")
                .Must(p => p.Count >= 2).WithMessage("Expense must have at least 2 participants")
                .Must(p => p.Count <= 100).WithMessage("Expense cannot have more than 100 participants")
                .Must((cmd, participants) => ValidateParticipantsUnique(participants))
                .WithMessage("Duplicate participants not allowed")
                .Must((cmd, participants) => ValidateCustomShares(cmd.SplitType, cmd.TotalAmount, participants))
                .WithMessage("For Custom split, all participants must have share amount and sum must equal total");
        }

        private static bool ValidateParticipantsUnique(List<ParticipantDto> participants)
        {
            return participants.Select(p => p.UserId).Distinct().Count() == participants.Count;
        }

        private static bool ValidateCustomShares(string splitType, decimal totalAmount, List<ParticipantDto> participants)
        {
            if (splitType != "Custom")
                return true;

            if (participants.Any(p => p.ShareAmount <= 0))
                return false;

            var sum = participants.Sum(p => p.ShareAmount);
            return Math.Abs(sum - totalAmount) < 0.01m;
        }
    }
}
