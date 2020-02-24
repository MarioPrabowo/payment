using System;
using System.Collections.Generic;
using System.Text;

namespace Application
{
	public class TopUpCustomerBalanceDto
	{
		public Guid CustomerID { get; set; }
		public decimal TopUpAmount { get; set; }
	}
}
