﻿using Common.Email;
using Common.UniqueIdentifiers;
using DataLayer.Mongo.Repositories;
using DataLayer.RabbitMQ.QueueMessages;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Net.Mail;
using static Common.UniqueIdentifiers.Generator;
using System.Text.Json;

namespace DataLayer.RabbitMQ
{
    public class EmergencyKitQueueSubscribe
    {

        private readonly IModel Channel;
        private readonly EventingBasicConsumer Consumer;
        private readonly IUserRepository _userRepository;
        public EmergencyKitQueueSubscribe(RabbitMQConnection rabbitMqConnection, IUserRepository userRepository)
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
            this._userRepository = userRepository;
        }

        private async void EmergencyKitMessageReceived(object? sender, BasicDeliverEventArgs e)
        {
            EmergencyKitQueueMessage message = JsonSerializer.Deserialize<EmergencyKitQueueMessage>(e.Body.ToArray());
            try
            {
                using MailMessage mail = new MailMessage();
                mail.From = new MailAddress("support@encryptionapiservices.com");
                mail.To.Add(message.UserEmail);
                mail.Subject = "Emergency Kit - Cryptographic API Services";
                mail.Body = "This emergency kit was generated by Cryptographic API Services. Another one will not be generated, please store in a safe place like your cloud provider.<br/>" +
                            String.Format("Your secret key is: <b>{0}</b>", message.AesKey);
                mail.IsBodyHtml = true;
                SmtpClientSender.SendMailMessage(mail);
                this.Channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}