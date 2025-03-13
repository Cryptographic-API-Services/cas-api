using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Common.Email
{
    public static class SmtpClientSender
    {
        public async static Task SendMailMessage(MailMessage mail)
        {
            using SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            string email = Environment.GetEnvironmentVariable("Email");
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(email, Environment.GetEnvironmentVariable("EmailPass"));
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(mail);
        }
    }
}
