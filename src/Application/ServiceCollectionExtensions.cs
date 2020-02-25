using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Application
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection SetupMediatr([NotNullAttribute] this IServiceCollection serviceCollection)
		{
			serviceCollection.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);

			return serviceCollection;
		}
	}
}
