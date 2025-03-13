using DataLayer.RabbitMQ.QueueMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Text.Json;

namespace DataLayer.RabbitMQ
{
    public class EmergencyKitQueueSubscribe
    {
        private readonly IModel Channel;
        private readonly EventingBasicConsumer Consumer;

        public EmergencyKitQueueSubscribe(RabbitMQConnection rabbitMqConnection)
        {
            this.Channel = rabbitMqConnection.Connection.CreateModel();
            this.Channel.QueueDeclare(
                queue: RabbitMqConstants.Queues.EmergencyKit,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            this.Consumer = new EventingBasicConsumer(this.Channel);
            this.Consumer.Received += EmergencyKitMessageReceived;
            this.Channel.BasicConsume(queue: RabbitMqConstants.Queues.EmergencyKit, autoAck: false, consumer: this.Consumer);
        }

        public async void EmergencyKitMessageReceived(object? sender, BasicDeliverEventArgs e)
        {
            EmergencyKitSendQueueMessage message = JsonSerializer.Deserialize<EmergencyKitSendQueueMessage>(e.Body.ToArray());
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SendGridKey");
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("mikemulchrone987@gmail.com", "Mike Mulchrone");
                var subject = "Emergency Kit - Encryption API Services";
                var to = new EmailAddress(message.UserEmail);
                var htmlContent = "<html>" +
                    "<body>" +
                        "This is your emergency kit for account recovery if you completely forgot your password. Please store it in a safe place. Thanks for registering. <br>" + String.Format("Key: <b>{0}</b>", message.EncappedKey) +
                    "</body>" +
                    "</html>";
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
