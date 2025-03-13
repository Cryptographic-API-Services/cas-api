using DataLayer.RabbitMQ.QueueMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
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
                var apiKey = Environment.GetEnvironmentVariable("SendGridKey");
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("mikemulchrone987@gmail.com", "Mike Mulchrone");
                var subject = "Email 2FA - Encryption API Services";
                var to = new EmailAddress(message.UserEmail);
                var htmlContent = String.Format("Your login code is: <b>{0}</b>", message.HotpCode);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                var response = await client.SendEmailAsync(msg);
                if (response.IsSuccessStatusCode)
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
