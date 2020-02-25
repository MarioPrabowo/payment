using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.InMemory;

namespace Presentation.PaymentApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			this.Configuration = configuration;
			this.Env = env;
		}

		public IConfiguration Configuration { get; }
		public IWebHostEnvironment Env { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers(options => {
				if (this.Env.IsDevelopment())
				{
					options.Filters.Add<DevelopmentExceptionHandler>();
				}
				else
				{
					options.Filters.Add<ProductionExceptionHandler>();
				}
			});
			services.AddInMemoryDbServices();
			services.AddSingleton<IDateProvider, SystemDateProvider>();
			services.SetupMediatr();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
