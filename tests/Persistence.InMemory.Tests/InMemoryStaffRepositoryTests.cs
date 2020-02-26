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
	public class InMemoryStaffRepositoryTests
	{
		private readonly DbContextCreator _dbContextCreator;
		private readonly InMemoryStaffRepository _repo;

		public InMemoryStaffRepositoryTests()
		{
			_dbContextCreator = new DbContextCreator();
			_repo = new InMemoryStaffRepository(_dbContextCreator.CreateDbContext());
		}

		[Fact]
		public async Task WhenCreateStaffAsync_ThenStaffCreated()
		{
			// Arrange 
			var staff = new Staff()
			{
				ID = Guid.NewGuid(),
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			// Act
			var result = await _repo.CreateStaffAsync(staff);

			// Assert
			result.Should().BeEquivalentTo(staff);
			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				var staffInDb = await ctx.Staff.FirstOrDefaultAsync();
				staffInDb.Should().BeEquivalentTo(staff);
			}
		}

		[Fact]
		public async Task WhenDeleteStaffAsync_ThenStaffMarkedAsDeleted()
		{
			// Arrange 
			var staff = new Staff()
			{
				ID = Guid.NewGuid(),
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(staff);
				await ctx.SaveChangesAsync();
			}

			// Act
			await _repo.DeleteStaffAsync(staff.ID);

			// Assert
			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				// Deleted item shouldn't show when queried normally
				Assert.Null(await ctx.Staff.FirstOrDefaultAsync());

				var staffInDb = (await ctx.Staff.IgnoreQueryFilters().FirstOrDefaultAsync());
				staffInDb.Should().BeEquivalentTo(staff, options => options.Excluding(c => c.IsDeleted));
				Assert.True(staffInDb.IsDeleted);
			}
		}

		[Fact]
		public async Task WhenGetStaffAsync_ThenReturnStaff()
		{
			// Arrange 
			var staff = new Staff()
			{
				ID = Guid.NewGuid(),
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(staff);
				await ctx.SaveChangesAsync();
			}

			// Act
			var result = await _repo.GetStaffAsync(staff.ID);

			// Assert
			result.Should().BeEquivalentTo(staff);
		}

		[Fact]
		public async Task WhenGetStaffListAsync_ThenReturnStaffList()
		{
			// Arrange
			var staffs = new List<Staff>(){
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

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.AddRange(staffs);
				await ctx.SaveChangesAsync();
			}

			// Act
			var result = await _repo.GetStaffListAsync();

			// Assert
			result.Should().BeEquivalentTo(staffs);
		}

		[Fact]
		public async Task GivenStaffIsDeleted_WhenUpdateStaffAsync_ThenThrowException()
		{
			// Arrange 
			var staff = new Staff()
			{
				ID = Guid.NewGuid(),
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person",
				IsDeleted = true
			};

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(staff);
				await ctx.SaveChangesAsync();
			}

			var updatedStaff = staff.Adapt<Staff>();
			updatedStaff.Surname = "Updated";

			// Act + Assert
			await Assert.ThrowsAsync<UnableToUpdateDeletedRecordsException>(() => _repo.UpdateStaffAsync(updatedStaff));
		}

		[Fact]
		public async Task GivenStaffIsNotDeleted_WhenUpdateStaffAsync_ThenStaffUpdated()
		{
			// Arrange 
			var staff = new Staff()
			{
				ID = Guid.NewGuid(),
				Email = "test@email.com",
				Surname = "Test",
				GivenNames = "Person"
			};

			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				ctx.Add(staff);
				await ctx.SaveChangesAsync();
			}

			var updatedStaff = staff.Adapt<Staff>();
			updatedStaff.Surname = "Updated";

			// Act
			var result = await _repo.UpdateStaffAsync(updatedStaff);

			//Assert
			result.Should().BeEquivalentTo(updatedStaff);
			using (var ctx = _dbContextCreator.CreateDbContext())
			{
				var staffInDb = await ctx.Staff.FirstOrDefaultAsync();
				staffInDb.Should().BeEquivalentTo(updatedStaff);
			}
		}
	}
}
