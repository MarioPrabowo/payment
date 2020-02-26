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
	public class PaymentsApiTests : IClassFixture<WebApplicationFactory<Startup>>
	{
		private readonly WebApplicationFactory<Startup> _factory;
		private readonly HttpClient _client;
		private readonly Mock<IMediator> _mediator;
		private const string Url = "Payments/";


		public PaymentsApiTests(WebApplicationFactory<Startup> factory)
		{
			_mediator = new Mock<IMediator>();

			_factory = factory;
			_client = _factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton<IMediator>(_mediator.Object);
				});
			}).CreateClient();
		}

		[Fact]
		public async Task WhenCreatePayment_ThenPaymentCreated()
		{
			// Arrange
			var payment = new Payment()
			{
				ID = Guid.NewGuid(),
				CustomerID = Guid.NewGuid(),
				Amount = 100,
				ApproverID = Guid.NewGuid(),
				Comment = "test",
				PaymentStatus = PaymentStatus.Pending,
				PaymentDateUtc = DateTime.UtcNow,
				ProcessedDateUtc = DateTime.UtcNow.AddDays(1),
				RequestedDateUtc = DateTime.UtcNow.AddDays(2)
			};
			_mediator.Setup(m => m.Send(It.IsAny<CreatePaymentRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(payment);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, Url);
			message.Content = new ObjectContent<Payment>(payment, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<Payment>();

			// Assert
			response.EnsureSuccessStatusCode();
			_mediator.Verify(m => m.Send(It.Is<CreatePaymentRequest>(r => r.Payment.IsEquivalentTo(payment)), It.IsAny<CancellationToken>()), Times.Once);
			returned.Should().BeEquivalentTo(payment);
		}

		[Fact]
		public async Task GivenIdInParamDoesNotMatchIdInContent_WhenProcessPayment_ThenPaymentProcessed()
		{
			// Arrange
			var payment = new Payment()
			{
				ID = Guid.NewGuid(),
				CustomerID = Guid.NewGuid(),
				Amount = 100,
				ApproverID = Guid.NewGuid(),
				Comment = "test",
				PaymentStatus = PaymentStatus.Pending,
				PaymentDateUtc = DateTime.UtcNow,
				ProcessedDateUtc = DateTime.UtcNow.AddDays(1),
				RequestedDateUtc = DateTime.UtcNow.AddDays(2)
			};
			_mediator.Setup(m => m.Send(It.IsAny<ProcessPaymentRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(payment);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Patch, Url + Guid.NewGuid());
			message.Content = new ObjectContent<Payment>(payment, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<ProductionExceptionResult>();

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			_mediator.Verify(m => m.Send(It.Is<ProcessPaymentRequest>(r => r.Payment.IsEquivalentTo(payment)), It.IsAny<CancellationToken>()), Times.Never);
			Assert.Equal(nameof(IdMismatchException), returned.ExceptionType);
		}

		[Fact]
		public async Task GivenIdInParamMatchIdInContent_WhenProcessPayment_ThenPaymentProcessed()
		{
			// Arrange
			var payment = new Payment()
			{
				ID = Guid.NewGuid(),
				CustomerID = Guid.NewGuid(),
				Amount = 100,
				ApproverID = Guid.NewGuid(),
				Comment = "test",
				PaymentStatus = PaymentStatus.Pending,
				PaymentDateUtc = DateTime.UtcNow,
				ProcessedDateUtc = DateTime.UtcNow.AddDays(1),
				RequestedDateUtc = DateTime.UtcNow.AddDays(2)
			};
			_mediator.Setup(m => m.Send(It.IsAny<ProcessPaymentRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(payment);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Patch, Url + payment.ID);
			message.Content = new ObjectContent<Payment>(payment, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<Payment>();

			// Assert
			response.EnsureSuccessStatusCode();
			_mediator.Verify(m => m.Send(It.Is<ProcessPaymentRequest>(r => r.Payment.IsEquivalentTo(payment)), It.IsAny<CancellationToken>()), Times.Once);
			returned.Should().BeEquivalentTo(payment);
		}
	}
}
