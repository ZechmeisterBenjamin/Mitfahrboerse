using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace Mitfahrboerse.Models
{
    public class AccessToken
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpClientFactory _httpClientFactory;

        public AccessToken(ITokenAcquisition tokenAcquisition, IHttpClientFactory httpClientFactory)
        {
            _tokenAcquisition = tokenAcquisition;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetAccessTokenAsync(string[] scopes)
        {
            return await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
        }

        public async Task<HttpClient> GetAuthorizedClientAsync(string[] scopes)
        {
            var accessToken = await GetAccessTokenAsync(scopes);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            return client;
        }
    }

}
