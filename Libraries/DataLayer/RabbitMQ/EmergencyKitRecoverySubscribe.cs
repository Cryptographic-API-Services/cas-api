using System;
using System.Collections.Generic;
using System.Text.Json;
using Common.Email;
using DataLayer.RabbitMQ.QueueMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
                var apiKey = Environment.GetEnvironmentVariable("Resender");
                var htmlContent = String.Format("Your account has been unlocked and the new password is: <b>{0}</b>", message.NewPassword);
                EmailRequestBody body = new EmailRequestBody()
                {
                    From = new string("support@cryptographicapiservices.com"),
                    To = new List<string>() { new string(message.UserEmail) },
                    Subject = "Emergency Kit Recovery - Cryptographic API Services",
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
