using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.PaymentApi
{
	public class DevelopmentExceptionHandler : ExceptionHandlerBase
	{
		protected override object GetExceptionDetails(Exception ex)
		{
			return new DevelopmentExceptionResult
			{
				ExceptionType = ex.GetType().Name,
				ExceptionMessage = ex.Message,
				BaseException = ex.GetBaseException().GetType().Name,
				StackTrace = ex.StackTrace,
			};
		}
	}
}
