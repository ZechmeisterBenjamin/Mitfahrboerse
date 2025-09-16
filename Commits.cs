"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "Domain": "htlwy.onmicrosoft.com",
  "TenantId": "6dd5291a-610e-4172-a7b6-9a7dc57e9a2a",
  "ClientId": "4ccc8eb2-2902-4558-ba37-d2eb842e35b3",
  "ClientSecret": "YjC8Q~nLRXDy-__eJwGm5e7hkvYOJU4sbBRiea4N"
}

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(
        new string[] { "User.Read" })  
    .AddInMemoryTokenCaches();


// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});


public class HomeController : Controller
{
  private readonly ILogger<HomeController> _logger;
  private IHttpClientFactory _httpClientFactory;
  private readonly ITokenAcquisition _tokenAcquisition;

  public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, ITokenAcquisition tokenAcquisition)
  {
    _logger = logger;
    _httpClientFactory = httpClientFactory;
    _tokenAcquisition = tokenAcquisition;
  }

  public IActionResult Login()
  {
    return Challenge(
        new AuthenticationProperties
        {
          RedirectUri = Url.Action("Index", "Home")
        },
        OpenIdConnectDefaults.AuthenticationScheme);
  }


  [Authorize]
  public async Task<IActionResult> Index(string code)
  {
    try
    {
      string[] scopes = { "User.Read" };
      var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
      ViewData["Token"] = accessToken;

      var client = _httpClientFactory.CreateClient();
      client.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", accessToken);

      var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
      var content = await response.Content.ReadAsStringAsync();

      ViewData["GraphResult"] = content;
      return View();
    }
    catch (MicrosoftIdentityWebChallengeUserException)
    {
      return Challenge(
          new AuthenticationProperties
          {
            RedirectUri = Url.Action("Index", "Home")
          },
          OpenIdConnectDefaults.AuthenticationScheme);
    }
  }

  public IActionResult Logout()
  {
    return SignOut(
        new AuthenticationProperties { RedirectUri = Url.Action("Index", "Home") },
        OpenIdConnectDefaults.AuthenticationScheme,
        CookieAuthenticationDefaults.AuthenticationScheme);
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

@{
    ViewData["Title"] = "Home Page";
}

<h2>Willkommen!</h2>

<p>Graph API Result:</p>
<pre>@ViewData["GraphResult"]</pre>

<a asp-action="Logout" > Abmelden </ a >

Alle wichtigen Daten von Microsoft azure in appsettings.josn übertragen