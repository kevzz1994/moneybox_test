using System;
using Moneybox.App.Domain;

namespace Moneybox.Tests
{
    /// <summary>
    /// Base test class with shared helper methods
    /// </summary>
    public class BaseTest
    {
        /// <summary>
        ///     Creates an account object
        /// </summary>
        /// <param name="balance">
        ///     The balance for the account
        /// </param>
        /// <returns>
        ///     The <see cref="Account"/>
        /// </returns>
        protected Account GetAccount(decimal balance)
        {
            return new Account
            {
                Id = Guid.NewGuid(),
                User = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Kevin Schaap",
                    Email = "kschaap1994@gmail.com"
                },
                Balance = balance
            };
        }
    }
}