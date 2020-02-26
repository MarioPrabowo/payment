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
	public class TopUpCustomerBalanceRequestTests
	{
		private readonly Mock<ICustomerRepository> _customerRepo;
		private TopUpCustomerBalanceRequest _request;
		private TopUpCustomerBalanceRequest.Handler _handler;

		public TopUpCustomerBalanceRequestTests()
		{
			_customerRepo = new Mock<ICustomerRepository>();
			
			_request = new TopUpCustomerBalanceRequest
			{
				TopUpCustomerBalanceDto = new TopUpCustomerBalanceDto
				{
					CustomerID = Guid.NewGuid(),
					TopUpAmount = 100
				}
			};
			_handler = new TopUpCustomerBalanceRequest.Handler( _customerRepo.Object);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-10)]
		public async Task GivenPaymentAmountIsLessThanOrEqualToZero_WhenTopUpCustomerBalance_ThenThrowException(int amount)
		{
			// Arrange
			_request.TopUpCustomerBalanceDto.TopUpAmount = amount;

			// Act + Assert
			await Assert.ThrowsAsync<AmountMustBeGreaterThanZeroException>(() => _handler.Handle(_request, new CancellationToken()));
		}

		[Fact]
		public async Task GivenCustomerNotFound_WhenTopUpCustomerBalance_ThenThrowException()
		{
			// Arrange
			_customerRepo.Setup(c => c.GetCustomerAsync(It.IsAny<Guid>())).ReturnsAsync(null as Customer);

			// Act + Assert
			await Assert.ThrowsAsync<CustomerNotFoundException>(() => _handler.Handle(_request, new CancellationToken()));
			_customerRepo.Verify(c => c.GetCustomerAsync(It.Is<Guid>(id => id == _request.TopUpCustomerBalanceDto.CustomerID)));
		}

		[Fact]
		public async Task GivenCustomerExists_WhenTopUpCustomerBalancet_ThenBalanceAdjusted()
		{
			// Arrange
			var customer = new Customer() { CurrentBalance = 0};
			_customerRepo.Setup(c => c.GetCustomerAsync(It.IsAny<Guid>())).ReturnsAsync(customer);

			var expectedCustomer = customer.Adapt<Customer>();
			expectedCustomer.CurrentBalance = customer.CurrentBalance + _request.TopUpCustomerBalanceDto.TopUpAmount;

			// Act
			await _handler.Handle(_request, new CancellationToken());

			// Assert
			_customerRepo.Verify(c => c.GetCustomerAsync(It.Is<Guid>(id => id == _request.TopUpCustomerBalanceDto.CustomerID)));
			_customerRepo.Verify(c => c.UpdateCustomerAsync(It.Is<Customer>(cust => cust.IsEquivalentTo(expectedCustomer))));
		}
	}
}
