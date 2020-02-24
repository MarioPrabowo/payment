using Domain;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Persistence.InMemory
{
	internal class InMemoryPaymentRepository: IPaymentRepository
	{
		private PaymentDbContext _ctx;
		public InMemoryPaymentRepository(PaymentDbContext ctx)
		{
			_ctx = ctx;
		}

		public async Task<Payment> CreatePaymentAsync(Payment payment)
		{
			_ctx.Add(payment);
			await _ctx.SaveChangesAsync();

			return payment;
		}

		public async Task<List<Payment>> GetPaymentListAsync(Guid customerID, int skip, int take)
		{
			return await (from p in _ctx.Payment
						  where p.RequesterID == customerID
						  select p).OrderByDescending(p => p.PaymentDateUtc).Skip(skip).Take(take).ToListAsync();
		}

		public async Task<Payment> GetPaymentAsync(Guid paymentID)
		{
			return await (from p in _ctx.Payment
						  where p.ID == paymentID
						  select p).FirstOrDefaultAsync();
		}

		public async Task<Payment> UpdatePaymentAsync(Payment payment)
		{
			_ctx.Add(payment);
			
			await _ctx.SaveChangesAsync();

			return payment;
		}
	}
}
