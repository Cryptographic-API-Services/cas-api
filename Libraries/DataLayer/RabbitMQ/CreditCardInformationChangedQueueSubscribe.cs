using DataLayer.RabbitMQ.QueueMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
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
            var apiKey = Environment.GetEnvironmentVariable("SendGridKey");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("mikemulchrone987@gmail.com", "Mike Mulchrone");
            var subject = "Credit Card Changed - Encryption API Services";
            var to = new EmailAddress(message.UserEmail);
            var htmlContent = "We noticed that you changed your credit card information recently. If this wasn't you we recommend changing your password " + String.Format("<a href='" + Environment.GetEnvironmentVariable("Domain") + "/#/forgot-password'>here</a>");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            var response = await client.SendEmailAsync(msg);
            if (response.IsSuccessStatusCode)
            {
                this.Channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            }
        }
    }
}