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
			return new
			{
				// textStatus is the standard AJAX response message
				textStatus = ex.Message,
				baseException = ex.GetBaseException().GetType().Name,
				stackTrace = ex.StackTrace,
			};
		}
	}
}
