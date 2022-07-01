using DevFreela.Payments.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevFreela.Payments.API.Services
{
    public class PaymentService : IPaymentService
    {
        public Task<bool> Process(PaymentInfoInputModel paymentInfoInputModel)
        {
            // aqui seria um GatewayDePagamento
            
            return Task.FromResult(true);
        }
    }
}
