using DevFreela.Payments.API.Models;
using DevFreela.Payments.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DevFreela.Payments.API.Consumers
{
    public class ProcessPaymentConsumer : BackgroundService
    {
        private const string QUEUE = "Payments";
        private const string PAYMENT_APPROVED_QUEUE = "PaymentsApproved";
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        public ProcessPaymentConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory { HostName = "localhost" };

            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                    queue: QUEUE,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

            _channel.QueueDeclare(
                   queue: PAYMENT_APPROVED_QUEUE,
                   durable: false,
                   exclusive: false,
                   autoDelete: false,
                   arguments: null
               );

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel); // Solicita a entrega de mensagens de forma assincrona e fornece um retorno da chamada

            consumer.Received += (sender,eventArgs) => 
            {
                var byteArray = eventArgs.Body.ToArray();
                var paymentInfoJson = Encoding.UTF8.GetString(byteArray);
                
                var paymentInfo = JsonSerializer.Deserialize<PaymentInfoInputModel>(paymentInfoJson);

                ProcessPayment(paymentInfo);

                // colocando mensagem na fila de pagamentos Aprovados
                var paymentApproved = new PaymentApprovedIntegrationEvent(paymentInfo.IdProject);
                var paymentApprovedJson = JsonSerializer.Serialize(paymentApproved);
                var paymentApprovedBytes = Encoding.UTF8.GetBytes(paymentApprovedJson);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: PAYMENT_APPROVED_QUEUE,
                    basicProperties: null,
                    body: paymentApprovedBytes
                    );

                _channel.BasicAck(eventArgs.DeliveryTag,false); //diz pro broker que a mensagem foi recebida 
            };

            //autoack 2 parametro bool = indica se a mensagem que chegou ja diz que foi processada
            _channel.BasicConsume(QUEUE, false, consumer);

            return Task.CompletedTask;
        }

        public void ProcessPayment(PaymentInfoInputModel paymentInfo)
        {
            using (var scope = _serviceProvider.CreateScope()) 
            {
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                paymentService.Process(paymentInfo);
            }
        }
    }
}
