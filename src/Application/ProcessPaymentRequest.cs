using Domain;
using MediatR;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application
{
	public class ProcessPaymentRequest : IRequest<Payment>
	{
		public Payment Payment { get; set; }

		public class Handler : IRequestHandler<ProcessPaymentRequest, Payment>
		{
			private readonly IPaymentRepository _paymentRepo;
			private readonly ICustomerRepository _customerRepo;

			public Handler(IPaymentRepository paymentRepo, ICustomerRepository customerRepo)
			{
				_paymentRepo = paymentRepo;
				_customerRepo = customerRepo;
			}

			public async Task<Payment> Handle(ProcessPaymentRequest request, CancellationToken cancellationToken)
			{
				// Check payment status, make sure it's pending
				var payment = await _paymentRepo.GetPaymentAsync(request.Payment.ID);
				if(payment.PaymentStatus != PaymentStatus.Pending)
				{
					throw new UnableToProcessNonPendingPaymentException();
				}

				// Give default comment if payment is processed
				if(request.Payment.PaymentStatus == PaymentStatus.Processed)
				{
					request.Payment.Comment = Payment.ProcessedComment;
				}

				// Process payment
				request.Payment.ProcessedDateUtc = DateTime.UtcNow;
				await _paymentRepo.UpdatePaymentAsync(request.Payment);

				// If processed, deduct customer's balance
				await _customerRepo.AdjustBalanceAsync(request.Payment.RequesterID, -request.Payment.Amount);

				return request.Payment;
			}
		}
	}
}
