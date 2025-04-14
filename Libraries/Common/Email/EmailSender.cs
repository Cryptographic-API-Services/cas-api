using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.Email
{
    public static class EmailSender
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public static async Task<bool> SendEmail(string apiKey, EmailRequestBody body)
        {
            var url = "https://api.resend.com/emails";
            string requestBody = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
