using System;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;

namespace Moneybox.Tests
{
    public class WithdrawMoneyTest : BaseTest
    {
        private Mock<INotificationService> _notificationServiceMock;
        private Mock<IAccountRepository> _accountRepositoryMock;
        private WithdrawMoney _withdrawMoney;
        private Guid _fromAccount;

        /// <summary>
        /// Setup the test class
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _notificationServiceMock = new Mock<INotificationService>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _withdrawMoney = new WithdrawMoney(_accountRepositoryMock.Object, _notificationServiceMock.Object);
            _fromAccount = Guid.NewGuid();
        }

        [Test]
        public void TestWithdrawMoney_OnInvalidUser_ThrowsException()
        {
            // Execute it:
            var result = Assert.Throws<InvalidOperationException>(() => _withdrawMoney.Execute(_fromAccount, 50));
            Assert.That(result.Message == "Cannot withdraw money from an nonexistent account.");
        }

        [Test]
        public void TestTransferMoney_OnLimitedBalance_ThrowsException()
        {
            // Setup from account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_fromAccount))
                .Returns(GetAccount(50));
            // Execute it:
            var result = Assert.Throws<InvalidOperationException>(() => _withdrawMoney.Execute(_fromAccount, 500));
            Assert.That(result.Message == "Insufficient funds to make transfer");
        }

        [Test]
        public void TestTransferMoney_IsApproachingLowFunds_SendsNotification()
        {
            // Setup from account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_fromAccount))
                .Returns(GetAccount(750));
            // Execute it:
            _withdrawMoney.Execute(_fromAccount, 500);
            // Verify:
            _notificationServiceMock.Verify(n => n.NotifyFundsLow(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void TestTransferMoney_IsSuccessful()
        {
            var account = GetAccount(750);
            // Setup from account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_fromAccount))
                .Returns(account);
            // Execute it:
            _withdrawMoney.Execute(_fromAccount, 500);
            // Check that balance have been deducted:
            Assert.That(account.Balance == 250);
        }
    }
}