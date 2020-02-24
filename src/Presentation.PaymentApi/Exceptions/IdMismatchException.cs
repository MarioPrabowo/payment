using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.PaymentApi
{
	public class IdMismatchException : Exception
	{
		public Guid ID { get; set; }
		public Guid ObjectID { get; set; }

		public IdMismatchException(Guid id, Guid objectID)
			: base($"Request failed because the url id {id} did not match the object id {objectID}")
		{
			this.ID = id;
			this.ObjectID = objectID;
		}
	}
}
