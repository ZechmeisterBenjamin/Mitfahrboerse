using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using Microsoft.Identity.Web;
using System.Text.Json;
using Mitfahrboerse.Models;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Controllers
{
    // Basecontroller hilft, dass man auf jedem Controller jederzeit auf das Microsoft Konto zugreifen kann
    public class BaseController : Controller
    {
        protected readonly ILogger _logger; // Wird für Fehlermelungen verwendet
        protected readonly IAccessToken _accessToken; // Schnittstelle um den AcessToken zu erhalten
        private readonly MitfahrboerseDbContext _context;
        protected string personId;

        public BaseController(ILogger logger, IAccessToken accessToken, MitfahrboerseDbContext context)
        {
            _logger = logger;
            _accessToken = accessToken;
            _context = context;
        }

        public IActionResult Login()
        {
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Start", "Home"),
                },
                OpenIdConnectDefaults.AuthenticationScheme);
        }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isIndexPage = context.RouteData.Values["action"]?.ToString() == "Index";
            var hasVoucher = context.HttpContext.Request.Query.ContainsKey("vouchercode");

            if (isIndexPage && hasVoucher)
            {
                await next(); 
                return;
            }
            try
            {
                string[] scopes = { "User.Read", "profile" }; 

                var accessToken = await _accessToken.GetAccessTokenAsync(scopes); 
                ViewData["Token"] = accessToken;

                var client =
                    await _accessToken.GetAuthorizedClientAsync(scopes); 

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

                var doc = JsonDocument.Parse(content);
                personId = doc.RootElement.GetProperty("id").GetString();
                string firstname = doc.RootElement.GetProperty("givenName").GetString();
                string lastname = doc.RootElement.GetProperty("surname").GetString();
                string email = doc.RootElement.GetProperty("mail").GetString();
                string class_ = doc.RootElement.GetProperty("jobTitle").GetString();

                if (!_context.t_People.Any(p => p.PersonId == personId))
                {
                    bool isAdmin = (class_ == "Lehrer");
                    _context.t_People.Add(new t_Person { PersonId = personId, FirstName = firstname, LastName = lastname, Email = email, Class = class_, IsAdmin = isAdmin});
                    _context.SaveChanges();
                }
                var person = _context.t_People.Where(p => p.PersonId == personId).FirstOrDefaultAsync().Result;
                ViewData["CoinBalance"] = person.Points;
                ViewData["SelectedDesign"] = person.Design;


                /*
                bool isAdminUser = (class_ == "Lehrer");

                if (person.IsAdmin != isAdminUser)
                {
                    person.IsAdmin = isAdminUser;
                    _context.SaveChanges();
                }
                */


                // ViewData["IsAdmin"] nimmt nur den Wert aus der Datenbank
                ViewData["IsAdmin"] = person.IsAdmin;
            }
            catch
            {
                context.Result = Challenge(
                    new AuthenticationProperties
                    {
                        RedirectUri = Url.Action("Start", "Home")  
                    },
                    OpenIdConnectDefaults.AuthenticationScheme);
                return;
            }


            await next();
        }
    }
}
