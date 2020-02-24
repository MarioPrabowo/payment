using Domain;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Persistence.InMemory
{
	internal class InMemoryStaffRepository : IStaffRepository
	{
		private PaymentDbContext _ctx;
		public InMemoryStaffRepository(PaymentDbContext ctx)
		{
			_ctx = ctx;
		}

		public async Task<Staff> CreateStaffAsync(Staff staff)
		{
			_ctx.Add(staff);
			await _ctx.SaveChangesAsync();

			return staff;
		}

		public async Task DeleteStaffAsync(Guid staffID)
		{
			var staff = new Staff() { ID = staffID };
			_ctx.Staff.Attach(staff);
			staff.IsDeleted = true;
			await _ctx.SaveChangesAsync();
		}

		public async Task<Staff> GetStaffAsync(Guid staffID)
		{
			return await (from s in _ctx.Staff
						  where s.ID == staffID
						  select s).FirstOrDefaultAsync();
		}

		public async Task<List<Staff>> GetStaffListAsync()
		{
			return await (from s in _ctx.Staff
						  select s).ToListAsync();
		}

		public async Task<Staff> UpdateStaffAsync(Staff staff)
		{
			var isDeleted = await (from s in _ctx.Staff
							 where s.ID == staff.ID
							 select s.IsDeleted).FirstOrDefaultAsync();

			if(isDeleted)
			{
				throw new UnableToUpdateDeletedRecordsException();
			}

			_ctx.Update(staff);
			await _ctx.SaveChangesAsync();

			return staff;
		}
	}
}
