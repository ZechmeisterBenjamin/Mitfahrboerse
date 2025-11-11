using Mitfahrboerse.Interfaces;

namespace Mitfahrboerse.Models
{
    public class CalendarEvent
    {
        protected readonly IAccessToken _accessToken;
        
        public async Task CreateEvent()
        {
            string[] scopes = { "User.Read", "profile" };
            var accessToken = await _accessToken.GetAccessTokenAsync(scopes);

            string subject = "Test";
            DateTime startDate = DateTime.Now;
            
        } 
    }
}
