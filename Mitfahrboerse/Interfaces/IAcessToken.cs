namespace Mitfahrboerse.Interfaces
{
    public interface IAccessToken
    {
        Task<string> GetAccessTokenAsync(string[] scopes);
        Task<HttpClient> GetAuthorizedClientAsync(string[] scopes);
    }
}
