using Common.Email;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static Common.UniqueIdentifiers.Generator;
using System.Collections.Generic;
using System;
using DataLayer.RabbitMQ.QueueMessages;
using System.Text.Json;

namespace DataLayer.RabbitMQ
{
    public class CreditCardInformationChangedQueueSubscribe
    {
        private readonly IModel Channel;
        private readonly EventingBasicConsumer Consumer;
        public CreditCardInformationChangedQueueSubscribe(RabbitMQConnection rabbitMqConnection)
        {
            this.Channel = rabbitMqConnection.Connection.CreateModel();
            this.Channel.QueueDeclare(
                queue: RabbitMqConstants.Queues.CCInformationChanged,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            this.Consumer = new EventingBasicConsumer(this.Channel);
            this.Consumer.Received += CreditCardInformationChangedMessageReceived;
            this.Channel.BasicConsume(queue: RabbitMqConstants.Queues.CCInformationChanged, autoAck: false, consumer: this.Consumer);
        }

        private async void CreditCardInformationChangedMessageReceived(object? sender, BasicDeliverEventArgs e)
        {
            CreditCardInformationChangedQueueMessage message = JsonSerializer.Deserialize<CreditCardInformationChangedQueueMessage>(e.Body.ToArray());
            var apiKey = Environment.GetEnvironmentVariable("Resender");
            var htmlContent = "We noticed that you changed your credit card information recently. If this wasn't you we recommend changing your password " + String.Format("<a href='" + Environment.GetEnvironmentVariable("Domain") + "/#/forgot-password'>here</a>");
            EmailRequestBody body = new EmailRequestBody()
            {
                From = "support@cryptographicapiservices.com",
                To = new List<string>() { message.UserEmail },
                Subject = "Credit Card Changed - Cryptographic API Services",
                Html = htmlContent
            };
            bool result = await EmailSender.SendEmail(apiKey, body);
            if (result)
            {
                this.Channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            }
        }
    }
}