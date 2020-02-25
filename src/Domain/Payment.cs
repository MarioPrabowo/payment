using System;

namespace Domain
{
	public class Payment
	{
		public static readonly string InsufficientFundComment = "Not enough funds";
		public static readonly string ProcessedComment = "Processed";

		public Guid ID { get; set; }
		public decimal Amount { get; set; }
		public DateTime PaymentDateUtc { get; set; }
		public DateTime RequestedDateUtc { get; set; }
		public DateTime? ProcessedDateUtc { get; set; }
		public PaymentStatus PaymentStatus { get; set; }
		public string Comment { get; set; }
		public Guid CustomerID { get; set; }
		public Customer Customer { get; set; }
		public Guid ApproverID { get; set; }
		public Staff Approver { get; set; }
	}
}
