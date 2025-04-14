using Common.Email;
using Common.UniqueIdentifiers;
using DataLayer.Mongo.Repositories;
using DataLayer.RabbitMQ.QueueMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text.Json;
using static Common.UniqueIdentifiers.Generator;

namespace DataLayer.RabbitMQ
{
    public class LockedOutUserQueueSubscribe
    {
        private readonly IModel Channel;
        private readonly EventingBasicConsumer Consumer;
        private readonly IUserRepository userRepo;
        public LockedOutUserQueueSubscribe(
            RabbitMQConnection rabbitMqConnection,
            IUserRepository userRepository
            )
        {
            this.Channel = rabbitMqConnection.Connection.CreateModel();
            this.Channel.QueueDeclare(
                queue: RabbitMqConstants.Queues.LockedOutUsers,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            this.Consumer = new EventingBasicConsumer(this.Channel);
            this.Consumer.Received += LockedOutUserMessageReceived;
            this.Channel.BasicConsume(queue: RabbitMqConstants.Queues.LockedOutUsers, autoAck: false, consumer: this.Consumer);
            this.userRepo = userRepository;
        }

        private async void LockedOutUserMessageReceived(object? sender, BasicDeliverEventArgs e)
        {
            try
            {
                LockedOutUserQueueMessage message = JsonSerializer.Deserialize<LockedOutUserQueueMessage>(e.Body.ToArray());
                EmailToken emailToken = new Generator().GenerateEmailToken();
                var apiKey = Environment.GetEnvironmentVariable("Resender");
                var htmlContent = "Your account has been locked out due to many failed login attempts.</br>" + String.Format("To unlock your account click <a href='" + Environment.GetEnvironmentVariable("Domain") + "/#/unlock-account?id={0}&token={1}'>here</a>.", message.UserId, emailToken.UrlSignature);
                EmailRequestBody body = new EmailRequestBody()
                {
                    From = "support@cryptographicapiservices.com",
                    To = new List<string>() { new string(message.UserEmail) },
                    Subject = "Locked Out User Account - Cryptographic API Services",
                    Html = htmlContent
                };
                bool result = await EmailSender.SendEmail(apiKey, body);
                if (result)
                {
                    await this.userRepo.UpdateLockedOutUsersToken(message.UserId, emailToken.Base64HashedToken, emailToken.Base64PublicKey);
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
