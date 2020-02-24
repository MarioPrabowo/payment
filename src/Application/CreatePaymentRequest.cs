using Domain;
using MediatR;
using Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application
{
	public class CreatePaymentRequest : IRequest<Payment>
	{
		public Payment Payment { get; set; }

		public class Handler : IRequestHandler<CreatePaymentRequest, Payment>
		{
			private readonly IPaymentRepository _paymentRepo;
			private readonly ICustomerRepository _customerRepo;

			public Handler(IPaymentRepository paymentRepo, ICustomerRepository customerRepo)
			{
				_paymentRepo = paymentRepo;
				_customerRepo = customerRepo;
			}

			public async Task<Payment> Handle(CreatePaymentRequest request, CancellationToken cancellationToken)
			{
				// Make sure the payment amount is positive
				if (request.Payment.Amount <= 0)
				{
					throw new AmountMustBeGreaterThanZeroException();
				}

				request.Payment.RequestedDateUtc = DateTime.UtcNow;
				

				// Check customer balance
				var customer = await _customerRepo.GetCustomerAsync(request.Payment.RequesterID);

				if (customer.CurrentBalance < request.Payment.Amount)
				{
					request.Payment.ProcessedDateUtc = DateTime.UtcNow;
					request.Payment.PaymentStatus = PaymentStatus.Closed;
					request.Payment.Comment = Payment.InsufficientFundComment;
				}
				else
				{
					// Create payment
					request.Payment.PaymentStatus = PaymentStatus.Pending;
				}

				await _paymentRepo.CreatePaymentAsync(request.Payment);

				return request.Payment;
			}
		}
	}
}
