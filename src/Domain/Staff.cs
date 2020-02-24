using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
	/// <summary>
	/// Staff contains no navigational property to Payment as one staff can process massive number of payments through the entire time they are with the company
	/// so loading them into memory through navigation property can cause issues. If we need to get payments that staff processed,
	/// getting them through queries would be best.
	/// </summary>
	public class Staff
	{
		public Guid ID { get; set; }
		public string Surname { get; set; }
		public string GivenNames { get; set; }
		public string Email { get; set; }
		/// <summary>
		/// Soft-delete to prevent orphan payments
		/// </summary>
		public bool IsDeleted { get; set; }
	}
}
