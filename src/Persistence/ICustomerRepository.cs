using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Persistence
{
	public interface ICustomerRepository
	{
		Task<Customer> CreateCustomerAsync(Customer customer);
		Task<Customer> UpdateCustomerAsync(Customer customer);
		Task DeleteCustomerAsync(Guid customerID);
		Task<Customer> GetCustomerAsync(Guid customerID);
		Task<List<Customer>> GetCustomerListAsync();
		Task AdjustBalanceAsync(Guid customerID, decimal adjustmentAmount);
	}
}
