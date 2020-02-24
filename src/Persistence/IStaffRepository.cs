using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
	public interface IStaffRepository
	{
		Task<Staff> CreateStaffAsync(Staff staff);
		Task<Staff> UpdateStaffAsync(Staff staff);
		Task DeleteStaffAsync(Guid staffID);
		Task<Staff> GetStaffAsync(Guid staffID);
		Task<List<Staff>> GetStaffListAsync();
	}
}
