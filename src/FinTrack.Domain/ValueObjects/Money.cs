using System;

namespace FinTrack.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing money with currency (USD only).
    /// Ensures financial precision with 2 decimal places.
    /// </summary>
    public class Money : IEquatable<Money>
    {
        /// <summary>Money amount.</summary>
        public decimal Amount { get; }

        /// <summary>Currency code (USD).</summary>
        public string Currency { get; private set; } = "USD";

        /// <summary>
        /// Creates a Money value object.
        /// </summary>
        /// <param name="amount">Amount in USD.</param>
        /// <exception cref="ArgumentException">Thrown if amount is negative or has >2 decimal places.</exception>
        public Money(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            // Check decimal places
            var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(amount)[3])[2];
            if (decimalPlaces > 2)
                throw new ArgumentException("Amount cannot have more than 2 decimal places.", nameof(amount));

            Amount = Math.Round(amount, 2);
        }

        /// <summary>Adds two Money values.</summary>
        public static Money operator +(Money left, Money right)
        {
            return new Money(left.Amount + right.Amount);
        }

        /// <summary>Subtracts two Money values.</summary>
        public static Money operator -(Money left, Money right)
        {
            return new Money(left.Amount - right.Amount);
        }

        /// <summary>Multiplies Money by a factor.</summary>
        public static Money operator *(Money money, decimal factor)
        {
            return new Money(money.Amount * factor);
        }

        /// <summary>Divides Money by a divisor.</summary>
        public static Money operator /(Money money, decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException();
            return new Money(money.Amount / divisor);
        }

        /// <summary>Checks equality of two Money values.</summary>
        public bool Equals(Money? other)
        {
            return other != null && Amount == other.Amount && Currency == other.Currency;
        }

        /// <summary>Checks equality with another object.</summary>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Money);
        }

        /// <summary>Gets hash code for Money value.</summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        /// <summary>String representation of Money value.</summary>
        public override string ToString() => $"{Currency} {Amount:F2}";
    }
}
