using Domain;
using Infrastructure;
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
			private readonly IDateProvider _dateProvider;

			public Handler(IPaymentRepository paymentRepo, ICustomerRepository customerRepo, IDateProvider dateProvider)
			{
				_paymentRepo = paymentRepo;
				_customerRepo = customerRepo;
				_dateProvider = dateProvider;
			}

			public async Task<Payment> Handle(CreatePaymentRequest request, CancellationToken cancellationToken)
			{
				// Make sure the payment amount is positive
				if (request.Payment.Amount <= 0)
				{
					throw new AmountMustBeGreaterThanZeroException();
				}

				// Check that the payment has no approver
				if (request.Payment.ApproverID != Guid.Empty)
				{
					throw new UnexpectedApproverOnPaymentCreationException();
				}

				// Check customer balance
				var customer = await _customerRepo.GetCustomerAsync(request.Payment.CustomerID);

				if(customer == null)
				{
					throw new CustomerNotFoundException();
				}

				// Call it here so the utc time is consistent between requested and processed dates
				var utcNow = _dateProvider.GetUtcNow();

				request.Payment.RequestedDateUtc = utcNow;

				if (customer.CurrentBalance < request.Payment.Amount)
				{
					request.Payment.ProcessedDateUtc = utcNow;
					request.Payment.PaymentStatus = PaymentStatus.Closed;
					request.Payment.Comment = Payment.InsufficientFundComment;
				}
				else
				{
					// Create payment
					request.Payment.PaymentStatus = PaymentStatus.Pending;
				}

				await _paymentRepo.CreatePaymentAsync(request.Payment);

				// Adjust customer's balance
				customer.CurrentBalance -= request.Payment.Amount;
				await _customerRepo.UpdateCustomerAsync(customer);

				return request.Payment;
			}
		}
	}
}
