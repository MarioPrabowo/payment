using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Persistence.InMemory
{
	public class PaymentDbContext : DbContext
	{
		public virtual DbSet<Payment> Payment { get; set; }
		public virtual DbSet<Staff> Staff { get; set; }
		public virtual DbSet<Customer> Customer { get; set; }

		public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Payment>(entity =>
			{
				entity.HasKey(e => e.ID);

				entity.Property(e => e.Amount).IsRequired();
				entity.Property(e => e.PaymentDateUtc).IsRequired();

				entity.HasOne(e => e.Requester)
					.WithMany()
					.HasForeignKey(e => e.RequesterID);

				entity.HasOne(e => e.Approver)
					.WithMany()
					.HasForeignKey(e => e.ApproverID);
			});

			modelBuilder.Entity<Staff>(entity =>
			{
				entity.HasKey(e => e.ID);

				entity.HasQueryFilter(e => !e.IsDeleted);
			});

			modelBuilder.Entity<Customer>(entity =>
			{
				entity.HasKey(e => e.ID);

				entity.HasQueryFilter(e => !e.IsDeleted);
			});

			ConvertDateFieldsToUtc(modelBuilder);
		}

		private void ConvertDateFieldsToUtc(ModelBuilder builder)
		{
			 var dateTimeValueConverter = new ValueConverter<DateTime, DateTime>(
							v => v,
							v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

			var nullableDateTimeValueConverter = new ValueConverter<DateTime?, DateTime?>(
							v => v,
							v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

			foreach (var entity in builder.Model.GetEntityTypes())
			{
				foreach (var property in entity.GetProperties())
				{
					if (property.ClrType == typeof(DateTime))
					{
						property.SetValueConverter(dateTimeValueConverter);
					}

					if (property.ClrType == typeof(DateTime?))
					{
						property.SetValueConverter(nullableDateTimeValueConverter);
					}
				}
			}
		}
	}
}
