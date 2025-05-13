
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PortfolioWebApp.Components;
using PortfolioWebApp.Hubs;
using PortfolioWebApp.Models;
using PortfolioWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication("auth_cookie")
    .AddCookie("auth_cookie",options =>
    {
        options.Cookie.Name = "auth_cookie";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Error/Forbidden";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        
    });

builder.Services.AddAuthorization(options => 
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();


builder.Services.AddMudServices();

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<IUserLoginService, UserLoginService>();
builder.Services.AddScoped<UserLoginService>();
builder.Services.AddScoped<FriendshipService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<CircuitHandler, TrackingCircuitHandler>();
builder.Services.AddScoped<GlobalChatService>();

builder.Logging.AddConsole();
builder.Services.AddAntiforgery();

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        [ "application/octet-stream" ]);
});


var app = builder.Build();

app.UseResponseCompression();

app.UseExceptionHandler("/Error/ServerError");
app.UseStatusCodePagesWithReExecute("/error/{0}");

if (!app.Environment.IsDevelopment()) {
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/", context => {
    context.Response.Redirect("/Home");
    return Task.CompletedTask;
});

app.MapGet("/Account/Logout", async (HttpContext context) =>
{
    await context.SignOutAsync("auth_cookie");
    return Results.Redirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<GlobalChatHub>("/globalchathub");

app.Run();