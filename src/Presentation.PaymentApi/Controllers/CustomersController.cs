using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace Presentation.PaymentApi
{
    [StandardApiController]
    public class CustomersController : ControllerBase
    {
        [HttpPost]
        public Task<Customer> Create(Customer customer, [FromServices] ICustomerRepository customerRepo)
        {
            return customerRepo.CreateCustomerAsync(customer);
        }

        [HttpPatch("{id}")]
        public Task<Customer> Update(Guid id, Customer customer, [FromServices] ICustomerRepository customerRepo)
        {
            if (id != customer.ID) throw new IdMismatchException(id, customer.ID);

            return customerRepo.UpdateCustomerAsync(customer);
        }

        [HttpDelete("{id}")]
        public Task Delete(Guid id,[FromServices] ICustomerRepository customerRepo)
        {
            return customerRepo.DeleteCustomerAsync(id);
        }

        [HttpGet]
        public Task<List<Customer>> Get([FromServices] ICustomerRepository customerRepo)
        {
            return customerRepo.GetCustomerListAsync();
        }

        [HttpGet("{id}")]
        public Task<Customer> Get(Guid id, [FromServices] ICustomerRepository customerRepo)
        {
            return customerRepo.GetCustomerAsync(id);
        }

        [HttpPut("{id}/CurrentBalance")]
        public async Task TopUpBalance(Guid id, TopUpCustomerBalanceDto topUpDto, [FromServices] IMediator mediator)
        {
            if (id != topUpDto.CustomerID) throw new IdMismatchException(id, topUpDto.CustomerID);

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