using Domain;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestUtils;
using Xunit;

namespace Persistence.InMemory.Tests
{
	public class InMemoryCustomerRepositoryTests
	{
		private readonly DbContextCreator _dbContextCreator;
		private readonly InMemoryCustomerRepository _repo;

		public InMemoryCustomerRepositoryTests()
		{
			_dbContextCreator = new DbContextCreator();
			_repo = new InMemoryCustomerRepository(_dbContextCreator.CreateDbContext());
		}

		[Fact]
		public async Task WhenCreateCustomerAsync_ThenCustomerCreated()
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

			// Act
			var result = await _repo.CreateCustomerAsync(customer);

			// Assert
			result.Should().BeEquivalentTo(customer);
			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				var customerInDb = await ctx.Customer.FirstOrDefaultAsync();
				customerInDb.Should().BeEquivalentTo(customer);
			}
		}

		[Fact]
		public async Task WhenDeleteCustomerAsync_ThenCustomerMarkedAsDeleted()
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

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(customer);
				await ctx.SaveChangesAsync();
			}

			// Act
			await _repo.DeleteCustomerAsync(customer.ID);

			// Assert
			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				// Deleted item shouldn't show when queried normally
				Assert.Null(await ctx.Customer.FirstOrDefaultAsync());

				var customerInDb = (await ctx.Customer.IgnoreQueryFilters().FirstOrDefaultAsync());
				customerInDb.Should().BeEquivalentTo(customer, options => options.Excluding(c => c.IsDeleted));
				Assert.True(customerInDb.IsDeleted);
			}
		}

		[Fact]
		public async Task WhenGetCustomerAsync_ThenReturnCustomer()
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

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(customer);
				await ctx.SaveChangesAsync();
			}

			// Act
			var result = await _repo.GetCustomerAsync(customer.ID);

			// Assert
			result.Should().BeEquivalentTo(customer);
		}

		[Fact]
		public async Task WhenGetCustomerListAsync_ThenReturnCustomerList()
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

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.AddRange(customers);
				await ctx.SaveChangesAsync();
			}

			// Act
			var result = await _repo.GetCustomerListAsync();

			// Assert
			result.Should().BeEquivalentTo(customers);
		}

		[Fact]
		public async Task GivenCustomerIsDeleted_WhenUpdateCustomerAsync_ThenThrowException()
		{
			// Arrange 
			var customer = new Customer()
			{
				ID = Guid.NewGuid(),
				CurrentBalance = 200,
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person",
				IsDeleted = true
			};

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(customer);
				await ctx.SaveChangesAsync();
			}

			var updatedCustomer = customer.Adapt<Customer>();
			updatedCustomer.Surname = "Updated";

			// Act + Assert
			await Assert.ThrowsAsync<UnableToUpdateDeletedRecordsException>(()=> _repo.UpdateCustomerAsync(updatedCustomer));
		}

		[Fact]
		public async Task GivenCustomerIsNotDeleted_WhenUpdateCustomerAsync_ThenCustomerUpdated()
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

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(customer);
				await ctx.SaveChangesAsync();
			}

			var updatedCustomer = customer.Adapt<Customer>();
			updatedCustomer.Surname = "Updated";

			// Act
			var result = await _repo.UpdateCustomerAsync(updatedCustomer);

			//Assert
			result.Should().BeEquivalentTo(updatedCustomer);
			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				var customerInDb = await ctx.Customer.FirstOrDefaultAsync();
				customerInDb.Should().BeEquivalentTo(updatedCustomer);
			}
		}
	}
}
