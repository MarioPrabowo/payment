using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
	public class SystemDateProvider : IDateProvider
	{
		public DateTime GetUtcNow()
		{
			return DateTime.UtcNow;
		}
	}
}
