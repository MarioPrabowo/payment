using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
	/// <summary>
	/// Customer contains no navigational property to Payment as one customer can have massive number of payments through the entire time they are with the company
	/// so loading them into memory through navigation property can cause issues. If we need to get customer's payments,
	/// getting them through queries would be best.
	/// </summary>
	public class Customer: IDeletable
	{
		public Guid ID { get; set; }
		public string Surname { get; set; }
		public string GivenNames { get; set; }
		public string Email { get; set; }
		public decimal CurrentBalance { get; set; }
		/// <summary>
		/// Soft-delete to prevent orphan payments
		/// </summary>
		public bool IsDeleted { get; set; }
	}
}
