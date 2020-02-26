using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestUtils
{
	public class DbContextCreator
	{
		private readonly string _dbName;

		public DbContextCreator()
		{
			// Unique DB name per tests to avoid data getting mixed up
			_dbName = Guid.NewGuid().ToString();
		}

		public void Setup(IServiceCollection services)
		{
			// Remove the app's DbContext registration.
			services.Remove<DbContextOptions<PaymentDbContext>>();

			// Add ApplicationDbContext using unique in-memory database for testing.
			services.AddDbContext<PaymentDbContext>((IServiceProvider prov, DbContextOptionsBuilder optionsBuilder) =>
			{
				if (!optionsBuilder.IsConfigured)
				{
					optionsBuilder.UseInMemoryDatabase(databaseName: _dbName);
				}
			});
		}

		public PaymentDbContext CreateDbContext()
		{
			DbContextOptions<PaymentDbContext> options = new DbContextOptionsBuilder<PaymentDbContext>()
				.UseInMemoryDatabase(databaseName: _dbName)
				.Options;

			return new PaymentDbContext(options);
		}
	}
}
