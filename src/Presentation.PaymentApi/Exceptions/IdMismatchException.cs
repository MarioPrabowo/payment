using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.PaymentApi
{
	public class IdMismatchException : BusinessLogicException
	{
		public IdMismatchException()
		{
		}
	}
}
