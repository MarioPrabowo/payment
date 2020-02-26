using Application;
using Domain;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Persistence;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using TestUtils;
using Xunit;

namespace Presentation.PaymentApi.Tests
{
	public class CustomersApiTests : IClassFixture<WebApplicationFactory<Startup>>
	{
		private readonly WebApplicationFactory<Startup> _factory;
		private readonly HttpClient _client;
		private readonly Mock<ICustomerRepository> _customerRepo;
		private readonly Mock<IPaymentRepository> _paymentRepo;
		private readonly Mock<IMediator> _mediator;
		private const string Url = "Customers/";


		public CustomersApiTests(WebApplicationFactory<Startup> factory)
		{
			_customerRepo = new Mock<ICustomerRepository>();
			_paymentRepo = new Mock<IPaymentRepository>();
			_mediator = new Mock<IMediator>();

			_factory = factory;
			_client = _factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton<ICustomerRepository>(_customerRepo.Object);
					services.AddSingleton<IPaymentRepository>(_paymentRepo.Object);
					services.AddSingleton<IMediator>(_mediator.Object);
				});
			}).CreateClient();
		}

		[Fact]
		public async Task WhenCreateCustomer_ThenCustomerCreated()
		{
			// Arrange
			var customer = new Customer()
			{
				ID = Guid.NewGuid(),
				CurrentBalance = 200,
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			_customerRepo.Setup(c => c.CreateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(customer);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, Url);
			message.Content = new ObjectContent<Customer>(customer, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<Customer>();

			// Assert
			response.EnsureSuccessStatusCode();
			_customerRepo.Verify(c => c.CreateCustomerAsync(It.Is<Customer>(c => c.IsEquivalentTo(customer))), Times.Once);
			returned.Should().BeEquivalentTo(customer);
		}

		[Fact]
		public async Task GivenIdInParamDoesNotMatchIdInContent_WhenUpdateCustomer_ThenReturnBadRequest()
		{
			// Arrange
			var customer = new Customer()
			{
				ID = Guid.NewGuid(),
				CurrentBalance = 200,
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			_customerRepo.Setup(c => c.UpdateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(customer);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Patch, Url + Guid.NewGuid());
			message.Content = new ObjectContent<Customer>(customer, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<ProductionExceptionResult>();

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			_customerRepo.Verify(c => c.UpdateCustomerAsync(It.Is<Customer>(c => c.IsEquivalentTo(customer))), Times.Never);
			Assert.Equal(nameof(IdMismatchException), returned.ExceptionType);
		}

		[Fact]
		public async Task GivenIdInParamMatchIdInContent_WhenUpdateCustomer_ThenCustomerUpdated()
		{
			// Arrange
			var customer = new Customer()
			{
				ID = Guid.NewGuid(),
				CurrentBalance = 200,
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			_customerRepo.Setup(c => c.UpdateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(customer);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Patch, Url + customer.ID);
			message.Content = new ObjectContent<Customer>(customer, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<Customer>();

			// Assert
			response.EnsureSuccessStatusCode();
			_customerRepo.Verify(c => c.UpdateCustomerAsync(It.Is<Customer>(c => c.IsEquivalentTo(customer))), Times.Once);
			returned.Should().BeEquivalentTo(customer);
		}

		[Fact]
		public async Task WhenDeleteCustomer_ThenCustomerDeleted()
		{
			// Arrange
			var customerID = Guid.NewGuid();

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Delete, Url + customerID);
			var response = await _client.SendAsync(message);

			// Assert
			response.EnsureSuccessStatusCode();
			_customerRepo.Verify(c => c.DeleteCustomerAsync(It.Is<Guid>(id => id == customerID)), Times.Once);
		}

		[Fact]
		public async Task WhenGetCustomers_ThenReturnAllCustomers()
		{
			// Arrange
			var customers = new List<Customer>(){
				new Customer()
				{
					ID = Guid.NewGuid(),
					CurrentBalance = 200,
					Email = "test@email.com",
					Surname = "Test",
					GivenNames = "Person"
				},
				new Customer()
				{
					ID = Guid.NewGuid(),
					CurrentBalance = 100,
					Email = "test2@email.com",
					Surname = "Test2",
					GivenNames = "Person2"
				}
			};

			_customerRepo.Setup(c => c.GetCustomerListAsync()).ReturnsAsync(customers);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, Url);
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<List<Customer>>();

			// Assert
			response.EnsureSuccessStatusCode();
			_customerRepo.Verify(c => c.GetCustomerListAsync(), Times.Once);
			returned.Should().BeEquivalentTo(customers);
		}

		[Fact]
		public async Task WhenGetCustomer_ThenReturnRequestedCustomer()
		{
			// Arrange
			var customer = new Customer()
			{
				ID = Guid.NewGuid(),
				CurrentBalance = 200,
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			_customerRepo.Setup(c => c.GetCustomerAsync(It.IsAny<Guid>())).ReturnsAsync(customer);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, Url + customer.ID);
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<Customer>();

			// Assert
			response.EnsureSuccessStatusCode();
			_customerRepo.Verify(c => c.GetCustomerAsync(It.Is<Guid>(id => id == customer.ID)), Times.Once);
			returned.Should().BeEquivalentTo(customer);
		}

		[Fact]
		public async Task GivenIdInParamDoesNotMatchIdInContent_WhenTopUpBalance_ThenReturnBadRequest()
		{
			// Arrange
			var dto = new TopUpCustomerBalanceDto()
			{
				CustomerID = Guid.NewGuid(),
				TopUpAmount = 100
			};

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Put, Url + Guid.NewGuid() + "/CurrentBalance");
			message.Content = new ObjectContent<TopUpCustomerBalanceDto>(dto, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<ProductionExceptionResult>();

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			_mediator.Verify(m => m.Send(It.Is<TopUpCustomerBalanceRequest>(r => r.TopUpCustomerBalanceDto.IsEquivalentTo(dto)), It.IsAny<CancellationToken>()), Times.Never);
			Assert.Equal(nameof(IdMismatchException), returned.ExceptionType);
		}

		[Fact]
		public async Task GivenIdInParamMatchIdInContent_WhenTopUpCustomerBalance_ThenBalanceToppedUp()
		{
			// Arrange
			var dto = new TopUpCustomerBalanceDto()
			{
				CustomerID = Guid.NewGuid(),
				TopUpAmount = 100
			};

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Put, Url + dto.CustomerID + "/CurrentBalance");
			message.Content = new ObjectContent<TopUpCustomerBalanceDto>(dto, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);

			// Assert
			response.EnsureSuccessStatusCode();
			_mediator.Verify(m => m.Send(It.Is<TopUpCustomerBalanceRequest>(r => r.TopUpCustomerBalanceDto.IsEquivalentTo(dto)), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task WhenGetPayments_ThenReturnCustomerPayments()
		{
			var customerID = Guid.NewGuid();
			var skip = 2;
			var take = 2;
			var payments = new List<Payment>(){
				new Payment()
				{
					ID = Guid.NewGuid(),
					CustomerID = customerID,
					Amount = 100,
					ApproverID = Guid.NewGuid(),
					Comment = "test 1",
					PaymentStatus = PaymentStatus.Pending,
					PaymentDateUtc = DateTime.UtcNow,
					ProcessedDateUtc = DateTime.UtcNow.AddDays(1),
					 RequestedDateUtc = DateTime.UtcNow.AddDays(2)
				},
				new Payment()
				{
					ID = Guid.NewGuid(),
					CustomerID = customerID,
					Amount = 200,
					ApproverID = Guid.NewGuid(),
					Comment = "test 2",
					PaymentStatus = PaymentStatus.Processed,
					PaymentDateUtc = DateTime.UtcNow,
					ProcessedDateUtc = DateTime.UtcNow.AddDays(1),
					 RequestedDateUtc = DateTime.UtcNow.AddDays(2)
				}
			};

			_paymentRepo.Setup(c => c.GetPaymentListAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(payments);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, $"{Url}{customerID}/Payments?skip={skip}&take={take}");
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<List<Payment>>();

			// Assert
			response.EnsureSuccessStatusCode();
			_paymentRepo.Verify(p => p.GetPaymentListAsync(It.Is<Guid>(id => id == customerID), It.Is<int>(s => s == skip), It.Is<int>(t => t == take)), Times.Once);
			returned.Should().BeEquivalentTo(payments);
		}
	}
}
