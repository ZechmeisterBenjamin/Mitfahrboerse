namespace Mitfahrboerse.Interfaces
{
    // Schnittstelle, zum Abrufen vom AccessToken
    public interface IAccessToken
    {
        // Gibt AcessToken für Benutzer zurück
        Task<string> GetAccessTokenAsync(string[] scopes);
        // Erstellt HTTP Client mit BearerToken im Header
        Task<HttpClient> GetAuthorizedClientAsync(string[] scopes);
    }
}
