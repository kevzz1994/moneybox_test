using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            // Get the user from:
            var from = accountRepository.GetAccountById(fromAccountId);
            // Get the user to transfer the amount for:
            var to = accountRepository.GetAccountById(toAccountId);
            // Validate users:
            if (from == null || to == null)
            {
                throw new InvalidOperationException("Cannot transfer to accounts that dont exist.");
            }
            // Validate if it comes from same account:
            if (fromAccountId == toAccountId)
            {
                throw new InvalidOperationException("Cannot transfer to your own account.");
            }
            // Withdraw:
            from.Withdraw(amount);
            // Deposit to account:
            to.Deposit(amount);
            // Check if we need to send a notification to the user:
            if (from.IsApproachingLowFunds())
            {
                notificationService.NotifyFundsLow(from.User.Email);
            }
            // Check if we need to send a notification to the user:
            if (to.IsApproachingPayLimit())
            {
                notificationService.NotifyApproachingPayInLimit(to.User.Email);
            }
            // Update accounts:
            accountRepository.Update(from);
            accountRepository.Update(to);
        }
    }
}