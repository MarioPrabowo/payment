using Domain;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Persistence.InMemory
{
	internal class InMemoryCustomerRepository: ICustomerRepository
	{
		private PaymentDbContext _ctx;
		public InMemoryCustomerRepository(PaymentDbContext ctx)
		{
			_ctx = ctx;
		}

		public async Task<Customer> CreateCustomerAsync(Customer customer)
		{
			_ctx.Add(customer);
			await _ctx.SaveChangesAsync();

			return customer;
		}

		public async Task DeleteCustomerAsync(Guid customerID)
		{
			var customer = new Customer() { ID = customerID };
			_ctx.Customer.Attach(customer);
			customer.IsDeleted = true;
			await _ctx.SaveChangesAsync();
		}

		public async Task<Customer> GetCustomerAsync(Guid customerID)
		{
			return await (from c in _ctx.Customer
						  where c.ID == customerID
						  select c).AsNoTracking().FirstOrDefaultAsync();
		}

		public async Task<List<Customer>> GetCustomerListAsync()
		{
			return await (from c in _ctx.Customer
						  select c).AsNoTracking().ToListAsync();
		}

		public async Task AdjustBalanceAsync(Guid customerID, decimal adjustmentAmount)
		{
			await ValidateDeletion(customerID);

			var customer = await this.GetCustomerAsync(customerID);
			customer.CurrentBalance += adjustmentAmount;
			_ctx.Update(customer);
			await _ctx.SaveChangesAsync();
		}

		public async Task<Customer> UpdateCustomerAsync(Customer customer)
		{
			await ValidateDeletion(customer.ID);

			_ctx.Update(customer);
			await _ctx.SaveChangesAsync();

			return customer;
		}

		private async Task ValidateDeletion(Guid customerID)
		{
			var isDeleted = await (from c in _ctx.Customer
								   where c.ID == customerID
								   select c.IsDeleted).FirstOrDefaultAsync();

			if (isDeleted)
			{
				throw new UnableToUpdateDeletedRecordsException();
			}
		}
	}
}
