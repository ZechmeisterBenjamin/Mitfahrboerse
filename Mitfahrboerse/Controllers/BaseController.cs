using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mitfahrboerse.Interfaces;
using Microsoft.Identity.Web;

namespace Mitfahrboerse.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILogger _logger;
        protected readonly IAccessToken _accessToken;

        public BaseController(ILogger logger, IAccessToken accessToken)
        {
            _logger = logger;
            _accessToken = accessToken;
        }

        public IActionResult Login()
        {
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Ride")
                },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                string[] scopes = { "User.Read", "profile" };

                var accessToken = await _accessToken.GetAccessTokenAsync(scopes);
                ViewData["Token"] = accessToken;

                var client = await _accessToken.GetAuthorizedClientAsync(scopes);

                var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
                var content = await response.Content.ReadAsStringAsync();
                ViewData["GraphResult"] = content;

                var photoResponse = await client.GetAsync("https://graph.microsoft.com/v1.0/me/photo/$value");
                if (photoResponse.IsSuccessStatusCode)
                {
                    var imageData = await photoResponse.Content.ReadAsByteArrayAsync();
                    string base64 = Convert.ToBase64String(imageData);
                    ViewData["ProfilePicture"] = $"data:image/jpeg;base64,{base64}";
                }
                else
                {
                    ViewData["ProfilePicture"] = Url.Content("~/Pics/Profile.jpg");
                }
            }
            catch
            {
                context.Result = Challenge(
                    new AuthenticationProperties
                    {
                        RedirectUri = Url.Action("Index", "Ride")
                    },
                    OpenIdConnectDefaults.AuthenticationScheme);
                return; 
            }

            await next();
        }
    }
}
