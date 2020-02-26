using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.PaymentApi
{
	public class DevelopmentExceptionResult: ProductionExceptionResult
	{
		public string BaseException { get; set; }
		public string StackTrace { get; set; }
	}
}
