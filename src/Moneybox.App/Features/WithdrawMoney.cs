using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            // Get the user from:
            var from = accountRepository.GetAccountById(fromAccountId);
            // Validate the user:
            if (from == null)
            {
                throw new InvalidOperationException("Cannot withdraw money from an nonexistent account.");
            }
            // Withdraw:
            from.Withdraw(amount);
            // Check if we need to send a notification to the user:
            if (from.IsApproachingLowFunds())
            {
                notificationService.NotifyFundsLow(from.User.Email);
            }
        }
    }
}