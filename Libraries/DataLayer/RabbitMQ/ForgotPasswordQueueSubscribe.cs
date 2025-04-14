using System;
using System.Collections.Generic;
using System.Text.Json;
using Common.Email;
using Common.UniqueIdentifiers;
using DataLayer.Mongo.Repositories;
using DataLayer.RabbitMQ.QueueMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static Common.UniqueIdentifiers.Generator;

namespace DataLayer.RabbitMQ
{
    public class ForgotPasswordQueueSubscribe
    {
        private readonly IModel Channel;
        private readonly EventingBasicConsumer Consumer;
        private readonly IUserRepository _userRepository;
        public ForgotPasswordQueueSubscribe(RabbitMQConnection rabbitMqConnection, IUserRepository userRepository)
        {
            this.Channel = rabbitMqConnection.Connection.CreateModel();
            this.Channel.QueueDeclare(
                queue: RabbitMqConstants.Queues.ForgotPassword,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            this.Consumer = new EventingBasicConsumer(this.Channel);
            this.Consumer.Received += ForgotPasswordQueueMessageReceived;
            this.Channel.BasicConsume(queue: RabbitMqConstants.Queues.ForgotPassword, autoAck: false, consumer: this.Consumer);
            this._userRepository = userRepository;
        }

        private async void ForgotPasswordQueueMessageReceived(object? sender, BasicDeliverEventArgs e)
        {
            ForgotPasswordQueueMessage message = JsonSerializer.Deserialize<ForgotPasswordQueueMessage>(e.Body.ToArray());
            EmailToken emailToken = new Generator().GenerateEmailToken();
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("Resender");
                var htmlContent = "If you did not ask to reset this password please delete this email.</br>" + String.Format("<a href='" + Environment.GetEnvironmentVariable("Domain") + "/#/forgot-password/reset?id={0}&token={1}'>Click here to reset your password.</a>", message.UserId, emailToken.UrlSignature);
                EmailRequestBody body = new EmailRequestBody()
                {
                    From = new string("support@cryptographicapiservices.com"),
                    To = new List<string>() { new string(message.UserEmail) },
                    Subject = "Forgot Password - Cryptographic API Services",
                    Html = htmlContent
                };
                bool result = await EmailSender.SendEmail(apiKey, body);
                if (result)
                {
                    await this._userRepository.UpdateUsersForgotPasswordToReset(message.UserId, emailToken.Base64HashedToken, emailToken.Base64PublicKey, emailToken.UrlSignature);
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
