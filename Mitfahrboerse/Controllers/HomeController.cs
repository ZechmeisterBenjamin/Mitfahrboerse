using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Mitfahrboerse.Models;
using RestSharp;

namespace Mitfahrboerse.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index(string code)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            var client = new RestClient("https://login.microsoftonline.com/common/oauth2/v2.0/token");
            var request = new RestRequest();
            request.Method = Method.Get;
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("code", code);
            request.AddParameter("redirected_uri", "https://localhost:7292/Home/Index");

            request.AddParameter("client_id", "4ccc8eb2-2902-4558-ba37-d2eb842e35b3");
            request.AddParameter("client_secret", "YjC8Q~nLRXDy-__eJwGm5e7hkvYOJU4sbBRiea4N");

            RestResponse response = client.Execute(request);
            var content = response.Content;
            var res = JObject.Parse(content);
            
            
            var client2 = new RestClient("https://graph.microsoft.com/v1.0/me");
            client2.AddDefaultHeader("Authorization", "Bearer" + res["access_token"]);
            request = new RestRequest();
            request.Method = Method.Get;
            var response2 = client.Execute(request);

            var content2 = response2.Content;

            var useremail = JObject.Parse(content2);
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}