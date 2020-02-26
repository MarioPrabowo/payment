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
	public class ProcessPaymentRequestTests
	{
		private readonly Mock<IPaymentRepository> _paymentRepo;
		private readonly Mock<ICustomerRepository> _customerRepo;
		private readonly Mock<IStaffRepository> _staffRepo;
		private readonly Mock<IDateProvider> _dateProvider;
		private DateTime _utcNow;
		private ProcessPaymentRequest _request;
		private ProcessPaymentRequest.Handler _handler;

		public ProcessPaymentRequestTests()
		{
			_utcNow = DateTime.UtcNow;
			_request = new ProcessPaymentRequest
			{
				Payment = new Payment()
				{
					ID = Guid.NewGuid(),
					CustomerID = Guid.NewGuid(),
					Amount = 100,
					ApproverID = Guid.NewGuid(),
					Comment = "test",
					PaymentStatus = PaymentStatus.Pending,
					PaymentDateUtc = DateTime.UtcNow,
					RequestedDateUtc = DateTime.UtcNow.AddDays(2)
				}
			};

			_paymentRepo = new Mock<IPaymentRepository>();
			_customerRepo = new Mock<ICustomerRepository>();
			_staffRepo = new Mock<IStaffRepository>();
			_dateProvider = new Mock<IDateProvider>();

			_staffRepo.Setup(s => s.GetStaffAsync(It.IsAny<Guid>())).ReturnsAsync(new Staff());
			_customerRepo.Setup(c => c.GetCustomerAsync(It.IsAny<Guid>())).ReturnsAsync(new Customer());
			_paymentRepo.Setup(p => p.GetPaymentAsync(It.IsAny<Guid>())).ReturnsAsync(_request.Payment);
			_dateProvider.Setup(p => p.GetUtcNow()).Returns(_utcNow);
			_handler = new ProcessPaymentRequest.Handler(_paymentRepo.Object, _customerRepo.Object, _staffRepo.Object, _dateProvider.Object);
		}

		[Fact]
		public async Task GivenNoApprover_WhenProcessPayment_ThenThrowException()
		{
			// Arrange
			_request.Payment.ApproverID = Guid.Empty;

			// Act + Assert
			await Assert.ThrowsAsync<ApproverMissingException>(() => _handler.Handle(_request, new CancellationToken()));
		}

		[Fact]
		public async Task GivenApproverNotFound_WhenProcessPayment_ThenThrowException()
		{
			// Arrange
			_staffRepo.Setup(s => s.GetStaffAsync(It.IsAny<Guid>())).ReturnsAsync(null as Staff);

			// Act + Assert
			await Assert.ThrowsAsync<StaffNotFoundException>(() => _handler.Handle(_request, new CancellationToken()));
		}

		[Fact]
		public async Task GivenPaymentNotFound_WhenProcessPayment_ThenThrowException()
		{
			// Arrange
			_paymentRepo.Setup(p => p.GetPaymentAsync(It.IsAny<Guid>())).ReturnsAsync(null as Payment);

			// Act + Assert
			await Assert.ThrowsAsync<PaymentNotFoundException>(() => _handler.Handle(_request, new CancellationToken()));
		}

		[Fact]
		public async Task GivenCustomerNotFound_WhenProcessPayment_ThenThrowException()
		{
			// Arrange
			_customerRepo.Setup(c => c.GetCustomerAsync(It.IsAny<Guid>())).ReturnsAsync(null as Customer);

			// Act + Assert
			await Assert.ThrowsAsync<CustomerNotFoundException>(() => _handler.Handle(_request, new CancellationToken()));
		}

		[Theory]
		[InlineData(PaymentStatus.Closed)]
		[InlineData(PaymentStatus.Processed)]
		public async Task GivenExistingPaymentNotInPendingStatus_WhenProcessPayment_ThenThrowException(PaymentStatus status)
		{
			// Arrange
			_request.Payment.PaymentStatus = status;

			// Act + Assert
			await Assert.ThrowsAsync<UnableToProcessNonPendingPaymentException>(() => _handler.Handle(_request, new CancellationToken()));
		}

		[Fact]
		public async Task GivenPaymentIsClosed_WhenProcessPayment_ThenPaymentIsClosedAndCustomerBalanceReadjusted()
		{
			// Arrange
			_request.Payment = _request.Payment.Adapt<Payment>();// create a copy so it won't affect mock setup for payment repo
			_request.Payment.PaymentStatus = PaymentStatus.Closed;

			var customer = new Customer() { CurrentBalance = 0};
			_customerRepo.Setup(c => c.GetCustomerAsync(It.IsAny<Guid>())).ReturnsAsync(customer);

			var expectedPayment = _request.Payment.Adapt<Payment>();
			expectedPayment.ProcessedDateUtc = _utcNow;

			var expectedCustomer = customer.Adapt<Customer>();
			expectedCustomer.CurrentBalance = customer.CurrentBalance + expectedPayment.Amount;

			// Act
			var result = await _handler.Handle(_request, new CancellationToken());

			// Assert
			result.Should().BeEquivalentTo(expectedPayment);
			_paymentRepo.Verify(p => p.UpdatePaymentAsync(It.Is<Payment>(payment => payment.IsEquivalentTo(expectedPayment))));
			_customerRepo.Verify(c => c.UpdateCustomerAsync(It.Is<Customer>(cust => cust.IsEquivalentTo(expectedCustomer))));	
		}

		[Fact]
		public async Task GivenPaymentIsProcessed_WhenProcessPayment_ThenPaymentIsProcessed()
		{
			// Arrange
			_request.Payment = _request.Payment.Adapt<Payment>();// create a copy so it won't affect mock setup for payment repo
			_request.Payment.PaymentStatus = PaymentStatus.Processed;

			var customer = new Customer() { CurrentBalance = 0 };
			_customerRepo.Setup(c => c.GetCustomerAsync(It.IsAny<Guid>())).ReturnsAsync(customer);

			var expectedPayment = _request.Payment.Adapt<Payment>();
			expectedPayment.ProcessedDateUtc = _utcNow;
			expectedPayment.Comment = Payment.ProcessedComment;

			// Act
			var result = await _handler.Handle(_request, new CancellationToken());

			// Assert
			result.Should().BeEquivalentTo(expectedPayment);
			_paymentRepo.Verify(p => p.UpdatePaymentAsync(It.Is<Payment>(payment => payment.IsEquivalentTo(expectedPayment))));
			_customerRepo.Verify(c => c.UpdateCustomerAsync(It.Is<Customer>(cust => cust.IsEquivalentTo(customer))), Times.Never);
		}
	}
}
