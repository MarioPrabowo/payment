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
	public class InMemoryPaymentRepositoryTests
	{
		private readonly DbContextCreator _dbContextCreator;
		private readonly InMemoryPaymentRepository _repo;

		public InMemoryPaymentRepositoryTests()
		{
			_dbContextCreator = new DbContextCreator();
			_repo = new InMemoryPaymentRepository(_dbContextCreator.CreateDbContext());
		}

		[Fact]
		public async Task WhenCreatePaymentAsync_ThenPaymentCreated()
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

			// Act
			var result = await _repo.CreatePaymentAsync(payment);

			// Assert
			result.Should().BeEquivalentTo(payment);
			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				var paymentInDb = await ctx.Payment.FirstOrDefaultAsync();
				paymentInDb.Should().BeEquivalentTo(payment);
			}
		}

		[Fact]
		public async Task WhenGetPaymentAsync_ThenReturnPayment()
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

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(payment);
				await ctx.SaveChangesAsync();
			}

			// Act
			var result = await _repo.GetPaymentAsync(payment.ID);

			// Assert
			result.Should().BeEquivalentTo(payment);
		}

		[Fact]
		public async Task WhenGetPaymentListAsync_ThenReturnPaymentListNewestFirst()
		{
			// Arrange
			var customerID = Guid.NewGuid();
			var payments = new List<Payment>(){
				new Payment()
				{
					ID = Guid.NewGuid(),
					CustomerID = customerID,
					Amount = 100,
					ApproverID = Guid.NewGuid(),
					Comment = "test",
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
					PaymentDateUtc = DateTime.UtcNow.AddDays(-1),
					ProcessedDateUtc = DateTime.UtcNow.AddDays(1),
					RequestedDateUtc = DateTime.UtcNow.AddDays(2)
				}
			};

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.AddRange(payments);
				await ctx.SaveChangesAsync();
			}

			// Act
			var result = await _repo.GetPaymentListAsync(customerID, 1, 1);

			// Assert
			Assert.Collection(result, p => p.Should().BeEquivalentTo(payments[1]));
		}

		[Fact]
		public async Task WhenUpdatePaymentAsync_ThenPaymentUpdated()
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
				RequestedDateUtc = DateTime.UtcNow.AddDays(2)
			};

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(payment);
				await ctx.SaveChangesAsync();
			}

			var updatedPayment = payment.Adapt<Payment>();
			updatedPayment.PaymentStatus = PaymentStatus.Closed;
			updatedPayment.ProcessedDateUtc = DateTime.UtcNow.AddDays(1);

			// Act
			var result = await _repo.UpdatePaymentAsync(updatedPayment);

			//Assert
			result.Should().BeEquivalentTo(updatedPayment);
			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				var paymentInDb = await ctx.Payment.FirstOrDefaultAsync();
				paymentInDb.Should().BeEquivalentTo(updatedPayment);
			}
		}
	}
}
