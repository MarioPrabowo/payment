using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace Presentation.PaymentApi
{
    [StandardApiController]
    public class StaffController : ControllerBase
    {
        [HttpPost]
        public Task<Staff> CreateStaff(Staff staff, [FromServices] IStaffRepository staffRepo)
        {
            return staffRepo.CreateStaffAsync(staff);
        }

        [HttpPatch("{id}")]
        public Task<Staff> UpdateStaff(Guid id, Staff staff, [FromServices] IStaffRepository staffRepo)
        {
            if (id != staff.ID) throw new IdMismatchException();

            return staffRepo.UpdateStaffAsync(staff);
        }

        [HttpDelete("{id}")]
        public Task DeleteStaff(Guid id,[FromServices] IStaffRepository staffRepo)
        {
            return staffRepo.DeleteStaffAsync(id);
        }

        [HttpGet]
        public Task<List<Staff>> GetStaffList([FromServices] IStaffRepository staffRepo)
        {
            return staffRepo.GetStaffListAsync();
        }

        [HttpGet("{id}")]
        public Task<Staff> GetStaff(Guid id, [FromServices] IStaffRepository staffRepo)
        {
            return staffRepo.GetStaffAsync(id);
        }
    }
}