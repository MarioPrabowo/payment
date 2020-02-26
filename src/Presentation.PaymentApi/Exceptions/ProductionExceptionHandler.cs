using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.PaymentApi
{
	public class ProductionExceptionHandler : ExceptionHandlerBase
	{
		protected override object GetExceptionDetails(Exception ex)
		{
			return new ProductionExceptionResult
			{
				ExceptionType = ex.GetType().Name,
				ExceptionMessage = ex.Message
			};
		}
	}
}
