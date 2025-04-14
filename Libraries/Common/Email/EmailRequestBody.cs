using System.Collections.Generic;

namespace Common.Email
{
    public class EmailRequestBody
    {
        public string From { get; set; }
        public List<string> To { get; set; }
        public string Subject { get; set; }
        public string Html { get; set; }
    }
}
