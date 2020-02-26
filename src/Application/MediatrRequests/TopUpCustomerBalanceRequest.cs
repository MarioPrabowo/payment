using MediatR;
using Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application
{
	public class TopUpCustomerBalanceRequest : IRequest
	{
		public TopUpCustomerBalanceDto TopUpCustomerBalanceDto { get; set; }

		public class Handler : IRequestHandler<TopUpCustomerBalanceRequest, Unit>
		{
			private readonly ICustomerRepository _customerRepo;

			public Handler(ICustomerRepository customerRepo)
			{
				_customerRepo = customerRepo;
			}

			public async Task<Unit> Handle(TopUpCustomerBalanceRequest request, CancellationToken cancellationToken)
			{
				// Make sure top up amount is greater than zero
				if(request.TopUpCustomerBalanceDto.TopUpAmount <= 0)
				{
					throw new AmountMustBeGreaterThanZeroException();
				}

				// Top up
				var customer = await _customerRepo.GetCustomerAsync(request.TopUpCustomerBalanceDto.CustomerID);
				if(customer == null)
				{
					throw new CustomerNotFoundException();
				}

				customer.CurrentBalance += request.TopUpCustomerBalanceDto.TopUpAmount;
				await _customerRepo.UpdateCustomerAsync(customer);

				return Unit.Value;
			}
		}
	}
}
