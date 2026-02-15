using Microsoft.AspNetCore.SignalR;

namespace Mitfahrboerse.Services
{
    public class AzureAdUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value 
                   ?? connection.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }
    }
}