using Application;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.PaymentApi
{
    [StandardApiController]
    public class CustomersController : ControllerBase
    {
        [HttpPost]
        public Task<Customer> CreateCustomer(Customer customer, [FromServices] ICustomerRepository customerRepo)
        {
            return customerRepo.CreateCustomerAsync(customer);
        }

        [HttpPatch("{id}")]
        public Task<Customer> UpdateCustomer(Guid id, Customer customer, [FromServices] ICustomerRepository customerRepo)
        {
            if (id != customer.ID) throw new IdMismatchException();

            return customerRepo.UpdateCustomerAsync(customer);
        }

        [HttpDelete("{id}")]
        public Task DeleteCustomer(Guid id,[FromServices] ICustomerRepository customerRepo)
        {
            return customerRepo.DeleteCustomerAsync(id);
        }

        [HttpGet]
        public Task<List<Customer>> GetCustomers([FromServices] ICustomerRepository customerRepo)
        {
            return customerRepo.GetCustomerListAsync();
        }

        [HttpGet("{id}")]
        public Task<Customer> GetCustomer(Guid id, [FromServices] ICustomerRepository customerRepo)
        {
            return customerRepo.GetCustomerAsync(id);
        }

        [HttpPut("{id}/CurrentBalance")]
        public async Task TopUpBalance(Guid id, TopUpCustomerBalanceDto topUpDto, [FromServices] IMediator mediator)
        {
            if (id != topUpDto.CustomerID) throw new IdMismatchException();

            await mediator.Send(new TopUpCustomerBalanceRequest
            {
                TopUpCustomerBalanceDto = topUpDto
            });
        }

        [HttpGet("{id}/Payments")]
        public Task<List<Payment>> GetPayments(Guid id, [FromQuery] int skip, [FromQuery] int take, [FromServices] IPaymentRepository paymentRepo)
        {
            return paymentRepo.GetPaymentListAsync(id, skip, take);
        }
    }
}