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
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public Task<Payment> Create(Payment payment)
        {
            return _mediator.Send(new CreatePaymentRequest
            {
                Payment = payment
            });
        }

        [HttpPatch("{id}")]
        public Task<Payment> Process(Guid id, Payment payment)
        {
            if (id != payment.ID) throw new IdMismatchException();

            return _mediator.Send(new ProcessPaymentRequest
            {
                Payment = payment
            });
        }
    }
}