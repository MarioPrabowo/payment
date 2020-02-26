using Domain;
using FluentAssertions;
using Infrastructure;
using Mapster;
using Moq;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;
using TestUtils;
using Xunit;

namespace Application.Tests
{
	public class CreatePaymentRequestTests
	{
		private readonly Mock<IPaymentRepository> _paymentRepo;
		private readonly Mock<ICustomerRepository> _customerRepo;
		private readonly Mock<IDateProvider> _dateProvider;
		private DateTime _utcNow;
		private CreatePaymentRequest _request;
		private CreatePaymentRequest.Handler _handler;

		public CreatePaymentRequestTests()
		{
			_utcNow = DateTime.UtcNow;

			_paymentRepo = new Mock<IPaymentRepository>();
			_customerRepo = new Mock<ICustomerRepository>();
			_dateProvider = new Mock<IDateProvider>();

			_dateProvider.Setup(p => p.GetUtcNow()).Returns(_utcNow);
			
			_request = new CreatePaymentRequest
			{
				Payment = new Payment()
				{
					ID = Guid.NewGuid(),
					CustomerID = Guid.NewGuid(),
					Amount = 100,
					Comment = "test",
					PaymentStatus = PaymentStatus.Pending,
					PaymentDateUtc = DateTime.UtcNow,
					ProcessedDateUtc = DateTime.UtcNow.AddDays(1),
					RequestedDateUtc = DateTime.UtcNow.AddDays(2)
				}
			};
			_handler = new CreatePaymentRequest.Handler(_paymentRepo.Object, _customerRepo.Object, _dateProvider.Object);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-10)]
		public async Task GivenPaymentAmountIsLessThanOrEqualToZero_WhenCreatePayment_ThenThrowException(int amount)
		{
			// Arrange
			_request.Payment.Amount = amount;

			// Act + Assert
			await Assert.ThrowsAsync<AmountMustBeGreaterThanZeroException>(() => _handler.Handle(_request, new CancellationToken()));
		}

		[Fact]
		public async Task GivenPaymentHasApprover_WhenCreatePayment_ThenThrowException()
		{
			// Arrange
			_request.Payment.ApproverID = Guid.NewGuid();

			// Act + Assert
			await Assert.ThrowsAsync<UnexpectedApproverOnPaymentCreationException>(() => _handler.Handle(_request, new CancellationToken()));
		}

		[Fact]
		public async Task GivenCustomerNotFound_WhenCreatePayment_ThenThrowException()
		{
			// Arrange
			_customerRepo.Setup(c => c.GetCustomerAsync(It.IsAny<Guid>())).ReturnsAsync(null as Customer);

			// Act + Assert
			await Assert.ThrowsAsync<CustomerNotFoundException>(() => _handler.Handle(_request, new CancellationToken()));
			_customerRepo.Verify(c => c.GetCustomerAsync(It.Is<Guid>(id => id == _request.Payment.CustomerID)));
		}

		[Fact]
		public async Task GivenNotEnoughBalance_WhenCreatePayment_ThenClosePaymentImmediately()
		{
			// Arrange
			var customer = new Customer() { CurrentBalance = 0 };
			_customerRepo.Setup(c => c.GetCustomerAsync(It.IsAny<Guid>())).ReturnsAsync(customer);

			var expectedPayment = _request.Payment.Adapt<Payment>();
			expectedPayment.RequestedDateUtc = _utcNow;
			expectedPayment.ProcessedDateUtc = _utcNow;
			expectedPayment.PaymentStatus = PaymentStatus.Closed;
			expectedPayment.Comment = Payment.InsufficientFundComment;

			// Act
			var result = await  _handler.Handle(_request, new CancellationToken());

			// Assert
			result.Should().BeEquivalentTo(expectedPayment);
			_customerRepo.Verify(c => c.GetCustomerAsync(It.Is<Guid>(id => id == _request.Payment.CustomerID)));
			_customerRepo.Verify(c => c.UpdateCustomerAsync(It.Is<Customer>(cust => cust.IsEquivalentTo(customer))));
			_paymentRepo.Verify(p => p.CreatePaymentAsync(It.Is<Payment>(payment => payment.IsEquivalentTo(expectedPayment))));
		}

		[Fact]
		public async Task GivenCustomerHasEnoughBalance_WhenCreatePayment_ThenPaymentCreatedAndBalanceAdjusted()
		{
			// Arrange
			var customer = new Customer() { CurrentBalance = _request.Payment.Amount };
			_customerRepo.Setup(c => c.GetCustomerAsync(It.IsAny<Guid>())).ReturnsAsync(customer);

			var expectedPayment = _request.Payment.Adapt<Payment>();
			expectedPayment.RequestedDateUtc = _utcNow;
			expectedPayment.PaymentStatus = PaymentStatus.Pending;

			var expectedCustomer = customer.Adapt<Customer>();
			expectedCustomer.CurrentBalance = customer.CurrentBalance - expectedPayment.Amount;

			// Act
			var result = await _handler.Handle(_request, new CancellationToken());

			// Assert
			result.Should().BeEquivalentTo(expectedPayment);
			_customerRepo.Verify(c => c.GetCustomerAsync(It.Is<Guid>(id => id == _request.Payment.CustomerID)));
			_customerRepo.Verify(c => c.UpdateCustomerAsync(It.Is<Customer>(cust => cust.IsEquivalentTo(expectedCustomer))));
			_paymentRepo.Verify(p => p.CreatePaymentAsync(It.Is<Payment>(payment => payment.IsEquivalentTo(expectedPayment))));
		}
	}
}
