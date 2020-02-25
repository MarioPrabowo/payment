using Domain;
using Infrastructure;
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
			private readonly IStaffRepository _staffRepo;
			private readonly IDateProvider _dateProvider;

			public Handler(IPaymentRepository paymentRepo, ICustomerRepository customerRepo, IStaffRepository staffRepo, IDateProvider dateProvider)
			{
				_paymentRepo = paymentRepo;
				_customerRepo = customerRepo;
				_staffRepo = staffRepo;
				_dateProvider = dateProvider;
			}

			public async Task<Payment> Handle(ProcessPaymentRequest request, CancellationToken cancellationToken)
			{
				// Check that the payment has been approved
				if(request.Payment.ApproverID == Guid.Empty)
				{
					throw new ApproverMissingException();
				}

				// Make sure that the approver exists and not deleted.
				// This check is required because DB constraints won't throw exception if the approver ID is marked as deleted.
				var approver = await _staffRepo.GetStaffAsync(request.Payment.ApproverID);
				if(approver == null)
				{
					throw new StaffNotFoundException();
				}

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
				request.Payment.ProcessedDateUtc = _dateProvider.GetUtcNow();
				await _paymentRepo.UpdatePaymentAsync(request.Payment);

				// If processed, deduct customer's balance
				await _customerRepo.AdjustBalanceAsync(request.Payment.CustomerID, -request.Payment.Amount);

				return request.Payment;
			}
		}
	}
}
