using Domain;
using FluentAssertions;
using Infrastructure;
using Mapster;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using TestUtils;
using Xunit;

namespace Presentation.PaymentApi.Integration.Tests
{
	public class PaymentsApiTests : IClassFixture<WebApplicationFactory<Startup>>
	{
		private readonly WebApplicationFactory<Startup> _factory;
		private readonly DbContextCreator _dbContextCreator;
		private readonly DateTime _utcNow;
		private readonly HttpClient _client;
		private const string Url = "Payments/";
		

		public PaymentsApiTests(WebApplicationFactory<Startup> factory)
		{
			_factory = factory;
			_dbContextCreator = new DbContextCreator();
			_utcNow = DateTime.UtcNow;
			_client = _factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					_dbContextCreator.Setup(services);
					services.AddSingleton<IDateProvider>(Mock.Of<IDateProvider>(p => p.GetUtcNow() == _utcNow));
				});
			}).CreateClient();
		}

		[Fact]
		public async Task GivenCustomerExists_WhenCreatePayment_ThenPaymentCreated()
		{
			// Arrange
			var customer = new Customer()
			{
				ID = Guid.NewGuid(),
				CurrentBalance = 200
			};

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(customer);
				await ctx.SaveChangesAsync();
			}

			var payment = new Payment()
			{
				ID = Guid.NewGuid(),
				Amount = 100,
				PaymentDateUtc = DateTime.UtcNow.AddDays(5),
				CustomerID = customer.ID
			};

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, Url);
			message.Content = new ObjectContent<Payment>(payment, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var result = await response.Content.ReadAsAsync<Payment>();

			// Assert
			response.EnsureSuccessStatusCode();
			Assert.Equal(_utcNow, result.RequestedDateUtc);
			Assert.Equal(payment.ID, result.ID);
			Assert.Equal(payment.Amount, result.Amount);
			Assert.Equal(payment.PaymentDateUtc, result.PaymentDateUtc);
			Assert.Equal(payment.CustomerID, result.CustomerID);
			Assert.Equal(PaymentStatus.Pending, result.PaymentStatus);

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				var paymentInDb = await ctx.Payment.FirstOrDefaultAsync();
				result.Should().BeEquivalentTo(paymentInDb, options => options.Excluding(p => p.Customer));

				var customerInDb = await ctx.Customer.FirstOrDefaultAsync();
				Assert.Equal(customer.CurrentBalance - payment.Amount, customerInDb.CurrentBalance);
			}
		}

		[Fact]
		public async Task GivenCustomerAndStaffExist_WhenProcessPayment_ThenPaymentProcessed()
		{
			// Arrange
			var customer = new Customer()
			{
				ID = Guid.NewGuid(),
				CurrentBalance = 200
			};

			var approver = new Staff()
			{
				ID = Guid.NewGuid()
			};

			var payment = new Payment()
			{
				ID = Guid.NewGuid(),
				Amount = 100,
				PaymentDateUtc = DateTime.UtcNow.AddDays(-5),
				RequestedDateUtc = DateTime.UtcNow.AddDays(-5),
				CustomerID = customer.ID,
				PaymentStatus = PaymentStatus.Pending
			};

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(customer);
				ctx.Add(approver);
				ctx.Add(payment);
				await ctx.SaveChangesAsync();
			}

			// Shallow copy payment
			var updatedPayment = payment.Adapt<Payment>();
			updatedPayment.Customer = null;
			updatedPayment.ApproverID = approver.ID;
			updatedPayment.PaymentStatus = PaymentStatus.Processed;

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Patch, Url + payment.ID.ToString());
			message.Content = new ObjectContent<Payment>(updatedPayment, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var result = await response.Content.ReadAsAsync<Payment>();

			// Assert
			response.EnsureSuccessStatusCode();
			result.Should().BeEquivalentTo(updatedPayment, options => options.Excluding(p => p.ProcessedDateUtc)
																		.Excluding(p => p.Comment)
																		.Excluding(p => p.Customer)
																		.Excluding(p => p.Approver));
			Assert.Equal(_utcNow, result.ProcessedDateUtc);
			Assert.Equal(Payment.ProcessedComment, result.Comment);

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				var paymentInDb = await ctx.Payment.FirstOrDefaultAsync();
				result.Should().BeEquivalentTo(paymentInDb, options => options.Excluding(p => p.Customer).Excluding(p => p.Approver));

				var customerInDb = await ctx.Customer.FirstOrDefaultAsync();
				Assert.Equal(customer.CurrentBalance, customerInDb.CurrentBalance);
			}
		}
	}
}
