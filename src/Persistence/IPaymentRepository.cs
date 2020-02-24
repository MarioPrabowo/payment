using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
	public interface IPaymentRepository
	{
		Task<Payment> CreatePaymentAsync(Payment payment);
		Task<Payment> UpdatePaymentAsync(Payment payment);
		Task<Payment> GetPaymentAsync(Guid paymentID);
		Task<List<Payment>> GetPaymentListAsync(Guid customerID, int skip, int take);
	}
}
