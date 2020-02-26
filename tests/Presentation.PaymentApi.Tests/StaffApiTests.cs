using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Persistence;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using TestUtils;
using Xunit;

namespace Presentation.PaymentApi.Tests
{
	public class StaffApiTests : IClassFixture<WebApplicationFactory<Startup>>
	{
		private readonly WebApplicationFactory<Startup> _factory;
		private readonly HttpClient _client;
		private readonly Mock<IStaffRepository> _staffRepo;
		private const string Url = "Staff/";


		public StaffApiTests(WebApplicationFactory<Startup> factory)
		{
			_staffRepo = new Mock<IStaffRepository>();

			_factory = factory;
			_client = _factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddSingleton<IStaffRepository>(_staffRepo.Object);
				});
			}).CreateClient();
		}

		[Fact]
		public async Task WhenCreateStaff_ThenStaffCreated()
		{
			// Arrange
			var staff = new Staff()
			{
				ID = Guid.NewGuid(),
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			_staffRepo.Setup(c => c.CreateStaffAsync(It.IsAny<Staff>())).ReturnsAsync(staff);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, Url);
			message.Content = new ObjectContent<Staff>(staff, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<Staff>();

			// Assert
			response.EnsureSuccessStatusCode();
			_staffRepo.Verify(c => c.CreateStaffAsync(It.Is<Staff>(c => c.IsEquivalentTo(staff))), Times.Once);
			returned.Should().BeEquivalentTo(staff);
		}

		[Fact]
		public async Task GivenIdInParamDoesNotMatchIdInContent_WhenUpdateStaff_ThenReturnBadRequest()
		{
			// Arrange
			var staff = new Staff()
			{
				ID = Guid.NewGuid(),
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			_staffRepo.Setup(c => c.UpdateStaffAsync(It.IsAny<Staff>())).ReturnsAsync(staff);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Patch, Url + Guid.NewGuid());
			message.Content = new ObjectContent<Staff>(staff, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<ProductionExceptionResult>();

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			_staffRepo.Verify(c => c.UpdateStaffAsync(It.Is<Staff>(c => c.IsEquivalentTo(staff))), Times.Never);
			Assert.Equal(nameof(IdMismatchException), returned.ExceptionType);
		}

		[Fact]
		public async Task GivenIdInParamMatchIdInContent_WhenUpdateStaff_ThenStaffUpdated()
		{
			// Arrange
			var staff = new Staff()
			{
				ID = Guid.NewGuid(),
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			_staffRepo.Setup(c => c.UpdateStaffAsync(It.IsAny<Staff>())).ReturnsAsync(staff);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Patch, Url + staff.ID);
			message.Content = new ObjectContent<Staff>(staff, new JsonMediaTypeFormatter());
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<Staff>();

			// Assert
			response.EnsureSuccessStatusCode();
			_staffRepo.Verify(c => c.UpdateStaffAsync(It.Is<Staff>(c => c.IsEquivalentTo(staff))), Times.Once);
			returned.Should().BeEquivalentTo(staff);
		}

		[Fact]
		public async Task WhenDeleteStaff_ThenStaffDeleted()
		{
			// Arrange
			var staffID = Guid.NewGuid();

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Delete, Url + staffID);
			var response = await _client.SendAsync(message);

			// Assert
			response.EnsureSuccessStatusCode();
			_staffRepo.Verify(c => c.DeleteStaffAsync(It.Is<Guid>(id => id == staffID)), Times.Once);
		}

		[Fact]
		public async Task WhenGetStaffList_ThenReturnAllStaff()
		{
			// Arrange
			var staffList = new List<Staff>(){
				new Staff()
				{
					ID = Guid.NewGuid(),
					Email = "test@email.com",
					Surname = "Test",
					GivenNames = "Person"
				},
				new Staff()
				{
					ID = Guid.NewGuid(),
					Email = "test2@email.com",
					Surname = "Test2",
					GivenNames = "Person2"
				}
			};

			_staffRepo.Setup(c => c.GetStaffListAsync()).ReturnsAsync(staffList);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, Url);
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<List<Staff>>();

			// Assert
			response.EnsureSuccessStatusCode();
			_staffRepo.Verify(c => c.GetStaffListAsync(), Times.Once);
			returned.Should().BeEquivalentTo(staffList);
		}

		[Fact]
		public async Task WhenGetStaff_ThenReturnRequestedStaff()
		{
			// Arrange
			var staff = new Staff()
			{
				ID = Guid.NewGuid(),
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			_staffRepo.Setup(c => c.GetStaffAsync(It.IsAny<Guid>())).ReturnsAsync(staff);

			// Act
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, Url + staff.ID);
			var response = await _client.SendAsync(message);
			var returned = await response.Content.ReadAsAsync<Staff>();

			// Assert
			response.EnsureSuccessStatusCode();
			_staffRepo.Verify(c => c.GetStaffAsync(It.Is<Guid>(id => id == staff.ID)), Times.Once);
			returned.Should().BeEquivalentTo(staff);
		}
	}
}
