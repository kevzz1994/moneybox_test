using System;

namespace Moneybox.App.Domain
{
    public class Account
    {
        /// <summary>
        /// Class constants
        /// </summary>
        public const decimal PayInLimit = 4000m;
        public const decimal NotificationLimit = 500m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        /// <summary>
        ///     Withdraws an amount from the current account
        /// </summary>
        /// <param name="amount">
        ///     The amount to withdraw
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     Throws exception if the user has insufficient funds to make the transfer.
        /// </exception>
        public void Withdraw(decimal amount)
        {
            if (Balance < amount)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }
            // Deduce balance:
            Balance -= amount;
            // Add to the withdraw amount:
            Withdrawn -= amount;
        }

        /// <summary>
        ///     Deposit an amount to the current account.
        /// </summary>
        /// <param name="amount">
        ///     The amount to deposit
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     Throw an exception if the account has reached the limit.
        /// </exception>
        public void Deposit(decimal amount)
        {
            // Add to paid in:
            PaidIn += amount;
            // Check if limit reached:
            if (PaidIn >= PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }
            // Add to balance:
            Balance += amount;
        }

        /// <summary>
        ///     If the current account is approaching low funds
        /// </summary>
        /// <returns>
        ///     Returns <code>true</code> if so,
        ///     <code>false</code> otherwise.
        /// </returns>
        public bool IsApproachingLowFunds()
        {
            return Balance < NotificationLimit;
        }
    
        /// <summary>
        ///     If the current account is approaching the pay in limit
        /// </summary>
        /// <returns>
        ///     Returns <code>true</code> if so,
        ///     <code>false</code> otherwise.
        /// </returns>
        public bool IsApproachingPayLimit()
        {
            return PayInLimit - PaidIn < NotificationLimit;
        }
    }
}