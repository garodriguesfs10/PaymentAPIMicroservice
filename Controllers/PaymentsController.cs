using DevFreela.Payments.API.Models;
using DevFreela.Payments.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevFreela.Payments.API.Controllers
{
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentInfoInputModel paymentInfoInputModel) 
        {
            var result = await _paymentService.Process(paymentInfoInputModel);

            if (!result) 
            {
                return BadRequest();
            }

            return NoContent();

        }
    }
}
