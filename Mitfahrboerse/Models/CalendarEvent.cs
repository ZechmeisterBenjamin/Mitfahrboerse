using Mitfahrboerse.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace Mitfahrboerse.Models
{
    public class CalendarEvent
    {
        protected readonly IAccessToken _accessToken;

        public CalendarEvent(IAccessToken accessToken)
        {
            _accessToken = accessToken;
        }
        
        public async Task CreateEventAsync()
        {
            string[] scopes = { "User.Read", "profile", "Calendars.ReadWrite" };
            var client = await _accessToken.GetAuthorizedClientAsync(scopes);

            var jsongContent = new
            {
                subject = "Testfahrt",
                start = DateTime.Now,
                end = DateTime.Now
            };

            var jsonString = JsonConvert.SerializeObject(jsongContent);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://graph.microsoft.com/v1.0/me/events", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
        } 
    }
}
