using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
	/// <summary>
	/// Date provider is injected using dependency injection so it is easy to test.
	/// Otherwise, it will not be possible to assert date fields correctly as DateTime.UtcNow will always return different result.
	/// </summary>
	public interface IDateProvider
	{
		DateTime GetUtcNow();
	}
}
