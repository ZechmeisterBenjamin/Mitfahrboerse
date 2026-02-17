using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Web;
using Mitfahrboerse.Hubs;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(
       new string[] { "User.Read", "profile" })
       //new string[] { "User.Read", "profile", "Calendars.ReadWrite" })
    .AddInMemoryTokenCaches();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, Mitfahrboerse.Services.AzureAdUserIdProvider>();

// Add services to the container.


builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});



builder.Services.AddDbContext<MitfahrboerseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

builder.Services.AddScoped<IAccessToken, AccessToken>();
builder.Services.AddScoped<IBalanceService, BalanceService>();
builder.Services.AddScoped<IPointService, PointService>();
builder.Services.AddScoped<IRouteMatchService, RouteMatchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .AllowAnonymous();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Start}/")
    .WithStaticAssets();



app.MapHub<NotificationHub>("/notificationHub");
app.Run();
