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
    public class ActivateUserQueueSubscribe
    {
        private readonly IModel Channel;
        private readonly EventingBasicConsumer Consumer;
        private readonly IUserRepository _userRepository;
        public ActivateUserQueueSubscribe(RabbitMQConnection rabbitMqConnection, IUserRepository userRepository)
        {
            this.Channel = rabbitMqConnection.Connection.CreateModel();
            this.Channel.QueueDeclare(
                queue: RabbitMqConstants.Queues.ActivateUser,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            this.Consumer = new EventingBasicConsumer(this.Channel);
            this.Consumer.Received += ActivateUserQueueMessageReceived;
            this.Channel.BasicConsume(queue: RabbitMqConstants.Queues.ActivateUser, autoAck: false, consumer: this.Consumer);
            this._userRepository = userRepository;
        }

        private async void ActivateUserQueueMessageReceived(object? sender, BasicDeliverEventArgs e)
        {
            ActivateUserQueueMessage message = JsonSerializer.Deserialize<ActivateUserQueueMessage>(e.Body.ToArray());
            EmailToken emailToken = new Generator().GenerateEmailToken();
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("EmailApi");
                var htmlContent = "We are excited to have you here </br>" + String.Format("<a href='" + Environment.GetEnvironmentVariable("Domain") + "/#/activate?id={0}&token={1}'>Click here to activate</a>", message.UserId, emailToken.UrlSignature);
                EmailRequestBody body = new EmailRequestBody()
                {
                    From = new EmailAddress("support@cryptographicapiservices.com"),
                    To = new List<EmailAddress>() { new EmailAddress(message.UserEmail) },
                    Subject = "Account Activation - Cryptographic API Services",
                    Html = htmlContent
                };
                bool result = await EmailSender.SendEmail(apiKey, body);
                if (result)
                {
                    await this._userRepository.UpdateUsersRsaKeyPairsAndToken(message.UserId, emailToken.Base64PublicKey, emailToken.Base64HashedToken, emailToken.UrlSignature);
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
