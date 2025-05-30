
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PortfolioWebApp.Components;
using PortfolioWebApp.Components.Pages.Home;
using PortfolioWebApp.Hubs;
using PortfolioWebApp.Hubs.Connection;
using PortfolioWebApp.Models;
using Serilog;
using PortfolioWebApp.Services;
using PortfolioWebApp.Services.Chat;
using PortfolioWebApp.Shared;

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
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
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
builder.Services.AddHttpClient();


builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();


builder.Services.AddMudServices();

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<IUserLoginService, UserLoginService>();
builder.Services.AddScoped<UserLoginService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<FriendshipService>();
builder.Services.AddScoped<FriendRequestService>();
builder.Services.AddScoped<FriendRequestClientService>();
builder.Services.AddScoped<GlobalChatService>();
builder.Services.AddScoped<IDirectChatService, DirectChatService>();

builder.Services.AddSingleton<CircuitHandler, TrackingCircuitHandler>();

builder.Services.AddSingleton<INotificationConnectionStorage, UserConnectionStorage>(); // NotificationHub uses INotificationConnectionStorage
builder.Services.AddSingleton<IDirectChatConnectionStorage, UserConnectionStorage>();   // DirectChatHub uses IDirectChatConnectionStorage

builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});


var efLoggingEnabled = builder.Configuration.GetValue<bool>("EFLogging:EnableDbCommandLogging");
if (!efLoggingEnabled) {
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
}


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

app.MapGet("/Account/Logout", async (HttpContext context) => {
    await context.SignOutAsync("auth_cookie");
    return Results.Redirect("/");
});

app.MapGet("/api/globalchat", async (AppDbContext dbContext) => {
    var messages = await dbContext.GlobalMessages
        .Include(m => m.User)
        .OrderBy(m => m.Created)
        .Take(50)
        .ToListAsync();
    var messageDtos = messages.Select(m => new GlobalChatMessageDto (
        new UserDto(m.User.UserName, m.User.Id, m.User.ProfileColor),
        m.Content,
        m.Created
    ));
    return Results.Json(messageDtos);
});

app.MapGet("/api/directchat", async (
    HttpContext http,
    AppDbContext dbContext,
    string chatPartner
) => {
    var user = http.User.Identity?.Name;

    if (user == chatPartner) {
        return Results.StatusCode(400); // can't request a chat with yourself
    }

    var messages = dbContext.DirectMessages
        .Where(msg =>
            (msg.FromUser.UserName.Equals(chatPartner) && msg.ToUser.UserName.Equals(user)) ||
            (msg.FromUser.UserName.Equals(user) && msg.ToUser.UserName.Equals(chatPartner))
        )
        .Include(m => m.FromUser)
        .Include(m => m.ToUser)
        .OrderBy(m => m.Created)
        .ToList();

    var messageDtos = messages.Select(m => new DirectMessageDto(
        m.FromUser.UserName,
        m.ToUser.UserName,
        m.Content,
        m.Created,
        m.Read,
        m.Delivered
    ));
    
    return Results.Json(messageDtos);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<GlobalChatHub>("/globalchathub");
app.MapHub<NotificationHub>("/notificationhub");

app.Run();