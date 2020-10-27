using System;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;

namespace Moneybox.Tests
{
    public class TransferMoneyTest : BaseTest
    {
        private Mock<INotificationService> _notificationServiceMock;
        private Mock<IAccountRepository> _accountRepositoryMock;
        private TransferMoney _transferMoney;
        private Guid _fromAccount;
        private Guid _toAccount;

        /// <summary>
        /// Setup the test class
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _notificationServiceMock = new Mock<INotificationService>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _transferMoney = new TransferMoney(_accountRepositoryMock.Object, _notificationServiceMock.Object);
            _fromAccount = Guid.NewGuid();
            _toAccount = Guid.NewGuid();
        }

        [Test]
        public void TestTransferMoney_OnInvalidUsers_ThrowsException()
        {
            // Execute it:
            var result = Assert.Throws<InvalidOperationException>(() => _transferMoney.Execute(_fromAccount, _toAccount, 50));
            Assert.That(result.Message == "Cannot transfer to accounts that dont exist.");
        }

        [Test]
        public void TestTransferMoney_OnSameAccount_ThrowsException()
        {
            // Setup from account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_fromAccount))
                .Returns(GetAccount(50));
            // Setup to account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_fromAccount))
                .Returns(GetAccount(50));
            // Execute it:
            var result = Assert.Throws<InvalidOperationException>(() => _transferMoney.Execute(_fromAccount, _fromAccount, 50));
            Assert.That(result.Message == "Cannot transfer to your own account.");
        }

        [Test]
        public void TestTransferMoney_OnInsufficientBalance_ThrowsException()
        {
            // Setup from account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_fromAccount))
                .Returns(GetAccount(50));
            // Setup to account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_toAccount))
                .Returns(GetAccount(50));
            // Execute it:
            var result = Assert.Throws<InvalidOperationException>(() => _transferMoney.Execute(_fromAccount, _toAccount, 500));
            Assert.That(result.Message == "Insufficient funds to make transfer");
        }
        
        [Test]
        public void TestTransferMoney_WithPaidInLimitReached_ThrowsException()
        {
            // Setup from account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_fromAccount))
                .Returns(GetAccount(50));
            // Setup to account mock:
            var toAccount = GetAccount(50);
            toAccount.PaidIn = 5000m;
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_toAccount))
                .Returns(toAccount);
            // Execute it:
            var result = Assert.Throws<InvalidOperationException>(() => _transferMoney.Execute(_fromAccount, _toAccount, 50));
            Assert.That(result.Message == "Account pay in limit reached");
        }
        
        [Test]
        public void TestTransferMoney_IsApproachingLowFunds_SendsNotification()
        {
            var fromAccount = GetAccount(1000);
            // Setup from account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_fromAccount))
                .Returns(fromAccount);
            // Setup to account mock:
            var toAccount = GetAccount(1000);
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_toAccount))
                .Returns(toAccount);
            // Execute it:
            _transferMoney.Execute(_fromAccount, _toAccount, 750);
            // Verify:
            _notificationServiceMock.Verify(n => n.NotifyFundsLow(It.IsAny<string>()), Times.Once);
        }
        
        [Test]
        public void TestTransferMoney_IsApproachingPayLimit_SendsNotification()
        {
            var fromAccount = GetAccount(10000);
            // Setup from account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_fromAccount))
                .Returns(fromAccount);
            // Setup to account mock:
            var toAccount = GetAccount(1000);
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_toAccount))
                .Returns(toAccount);
            // Execute it:
            _transferMoney.Execute(_fromAccount, _toAccount, 3750);
            // Verify:
            _notificationServiceMock.Verify(n => n.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Once);
        }
        
        [Test]
        public void TestTransferMoney_IsSuccessful()
        {
            var fromAccount = GetAccount(1000);
            // Setup from account mock:
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_fromAccount))
                .Returns(fromAccount);
            // Setup to account mock:
            var toAccount = GetAccount(1000);
            _accountRepositoryMock
                .Setup(a => a.GetAccountById(_toAccount))
                .Returns(toAccount);
            // Execute it:
            _transferMoney.Execute(_fromAccount, _toAccount, 750);
            // Check that balance actually deducted:
            Assert.That(fromAccount.Balance == 250);
            // Check that balance has been added:
            Assert.That(toAccount.Balance == 1750);
        }
    }
}