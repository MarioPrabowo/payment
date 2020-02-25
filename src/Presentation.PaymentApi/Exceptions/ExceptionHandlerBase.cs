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
	public abstract class ExceptionHandlerBase : IExceptionFilter
	{
		public void OnException(ExceptionContext context)
		{
			if (context.Exception == null)
				return;

			context.ExceptionHandled = true;
			context.Result = new JsonResult(GetExceptionDetails(context.Exception))
			{
				StatusCode = (context.Exception is BusinessLogicException)
					? StatusCodes.Status400BadRequest
					: StatusCodes.Status500InternalServerError
			};
		}

		protected abstract object GetExceptionDetails(Exception ex);
	}
}
