using System.Collections.Generic;

namespace Common.Email
{
    public class EmailRequestBody
    {
        public EmailAddress From { get; set; }
        public List<EmailAddress> To { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string Html { get; set; }
    }

    public class EmailAddress
    {
        public string Email { get; set; }

        public EmailAddress(string email)
        {
            this.Email = email;
        }
    }
}
