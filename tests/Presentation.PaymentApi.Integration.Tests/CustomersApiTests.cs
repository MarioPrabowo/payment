using Application;
using Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
	public class CustomersApiTests
	: IClassFixture<WebApplicationFactory<Startup>>
	{
		private readonly WebApplicationFactory<Startup> _factory;
		private readonly DbContextCreator _dbContextCreator;
		private readonly HttpClient _client;
		private const string Url = "Customers/";


		public CustomersApiTests(WebApplicationFactory<Startup> factory)
		{
			_factory = factory;
			_dbContextCreator = new DbContextCreator();
			_client = _factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					_dbContextCreator.Setup(services);
				});
			}).CreateClient();
		}

		[Fact]
		public async Task GivenCustomerExists_WhenTopUpCustomerBalance_ThenBalanceToppedUp()
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

			var dto = new TopUpCustomerBalanceDto()
			{
				CustomerID = customer.ID,
				TopUpAmount = 100
			};

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Put, Url + customer.ID.ToString() + "/CurrentBalance");
			message.Content = new ObjectContent<TopUpCustomerBalanceDto>(dto, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);

			// Assert
			response.EnsureSuccessStatusCode();

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				var customerInDb = await ctx.Customer.FirstOrDefaultAsync();
				Assert.Equal(customer.CurrentBalance + dto.TopUpAmount, customerInDb.CurrentBalance);
			}
		}
	}
}
