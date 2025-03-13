using DataLayer.RabbitMQ.QueueMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Text.Json;

namespace DataLayer.RabbitMQ
{
    public class EmergencyKitRecoverySubscribe
    {
        private readonly IModel Channel;
        private readonly EventingBasicConsumer Consumer;
        public EmergencyKitRecoverySubscribe(RabbitMQConnection rabbitMqConnection)
        {
            this.Channel = rabbitMqConnection.Connection.CreateModel();
            this.Channel.QueueDeclare(
                queue: RabbitMqConstants.Queues.EmergencyKitRecovery,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            this.Consumer = new EventingBasicConsumer(this.Channel);
            this.Consumer.Received += EmergencyKitRecoveryReceived;
            this.Channel.BasicConsume(queue: RabbitMqConstants.Queues.EmergencyKitRecovery, autoAck: false, consumer: this.Consumer);
        }

        public async void EmergencyKitRecoveryReceived(object? sender, BasicDeliverEventArgs e)
        {
            EmergencyKitRecoveryQueueMessage message = JsonSerializer.Deserialize<EmergencyKitRecoveryQueueMessage>(e.Body.ToArray());
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SendGridKey");
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("mikemulchrone987@gmail.com", "Mike Mulchrone");
                var subject = "Emergency Kit Recovery - Encryption API Services";
                var to = new EmailAddress(message.UserEmail);
                var htmlContent = String.Format("Your account has been unlocked and the new password is: <b>{0}</b>", message.NewPassword);
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
