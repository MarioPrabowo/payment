using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Persistence.InMemory
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddInMemoryServices([NotNullAttribute] this IServiceCollection serviceCollection)
		{
			if (!serviceCollection.Any(s => s.ServiceType.IsAssignableFrom(typeof(PaymentDbContext))))
			{
				serviceCollection.AddDbContext<PaymentDbContext>((IServiceProvider prov, DbContextOptionsBuilder optionsBuilder) =>
				{
					if (!optionsBuilder.IsConfigured)
					{
						optionsBuilder.UseInMemoryDatabase(databaseName: "PaymentInMemoryDB");
					}
				});
			}

			serviceCollection.TryAddScoped<IPaymentRepository, InMemoryPaymentRepository>();
			serviceCollection.TryAddScoped<IStaffRepository, InMemoryStaffRepository>();
			serviceCollection.TryAddScoped<ICustomerRepository, InMemoryCustomerRepository>();


			return serviceCollection;
		}
	}
}
