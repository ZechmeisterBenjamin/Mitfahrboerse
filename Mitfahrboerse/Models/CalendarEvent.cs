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

        public async Task<string> CreateRideEventAsync(string subject, DateTime rideDateTime, string startLocation, string endLocation)
        {
            string[] scopes = { "Calendars.ReadWrite" };
            var client = await _accessToken.GetAuthorizedClientAsync(scopes);

            var eventjson = new
            {
                subject = subject,
                body = new { contentType = "HTML", content = $"Fahrt von {startLocation} nach {endLocation}" },
                start = new { dateTime = rideDateTime.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "W. Europe Standard Time" },
                end = new { dateTime = rideDateTime.AddMinutes(30).ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "W. Europe Standard Time" },
                location = new { displayName = startLocation }
            };

            var jsonString = JsonConvert.SerializeObject(eventjson);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://graph.microsoft.com/v1.0/me/events", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                dynamic createdEvent = JsonConvert.DeserializeObject(responseData);
                return createdEvent.id; 
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Fehler beim Erstellen! {error}");
        }

        public async Task DeleteEventAsync(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return;

            string[] scopes = { "Calendars.ReadWrite" };
            var client = await _accessToken.GetAuthorizedClientAsync(scopes);

            var response = await client.DeleteAsync($"https://graph.microsoft.com/v1.0/me/events/{eventId}");

            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Fehler beim Löschen! {error}");
            }
        }
    }
}
