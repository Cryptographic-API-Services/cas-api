using Common.Email;
using DataLayer.RabbitMQ.QueueMessages;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DataLayer.RabbitMQ
{
    public class Email2FAHotpCodeQueueSubscribe
    {
        private readonly IModel Channel;
        private readonly EventingBasicConsumer Consumer;

        public Email2FAHotpCodeQueueSubscribe(RabbitMQConnection rabbitMqConnection)
        {
            this.Channel = rabbitMqConnection.Connection.CreateModel();
            this.Channel.QueueDeclare(
                queue: RabbitMqConstants.Queues.Email2FAHotpCode,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            this.Consumer = new EventingBasicConsumer(this.Channel);
            this.Consumer.Received += Email2FAHotpMessageReceived;
            this.Channel.BasicConsume(queue: RabbitMqConstants.Queues.Email2FAHotpCode, autoAck: false, consumer: this.Consumer);
        }

        private async void Email2FAHotpMessageReceived(object? sender, BasicDeliverEventArgs e)
        {
            try
            {
                Email2FAHotpCodeQueueMessage message = JsonSerializer.Deserialize<Email2FAHotpCodeQueueMessage>(e.Body.ToArray());
                var apiKey = Environment.GetEnvironmentVariable("EmailApi");
                var htmlContent = String.Format("Your login code is: <b>{0}</b>", message.HotpCode);
                EmailRequestBody body = new EmailRequestBody()
                {
                    From = new EmailAddress("support@cryptographicapiservices.com"),
                    To = new List<EmailAddress>() { new EmailAddress(message.UserEmail) },
                    Subject = "Email 2FA - Cryptographic API Services",
                    Html = htmlContent
                };
                bool result = await EmailSender.SendEmail(apiKey, body);
                if (result)
                {
                    this.Channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
