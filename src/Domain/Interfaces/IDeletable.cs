using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
	/// <summary>
	/// This interface is mainly used for maintaining consistency at the moment.
	/// In the future, we might want to clean up all deleted records after a certain amount of time (e.g. 7 years),
	/// or at least wiped out all privacy data. Having consistent property naming will help with that.
	/// </summary>
	public interface IDeletable
	{
		bool IsDeleted { get; set; }
	}
}
