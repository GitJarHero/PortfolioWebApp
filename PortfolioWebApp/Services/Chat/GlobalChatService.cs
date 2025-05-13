using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using PortfolioWebApp.Components.Pages.Home;
using PortfolioWebApp.Hubs;

namespace PortfolioWebApp.Services.Chat;

public class GlobalChatService {
    
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _config;
    private NavigationManager _navigation;

    private HubConnection? hubConnection;
    
    public event Func<Home.GlobalChatMessageDto, Task>? OnMessageCallback;


    
    public GlobalChatService(HttpClient httpClient, 
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration config,
                                NavigationManager navigation) {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _config = config;
        _navigation = navigation;
        
        if (_httpClient.BaseAddress == null) {
            _httpClient.BaseAddress = new Uri("http://localhost:5120");
        }
        
        var context = _httpContextAccessor.HttpContext;
        var cookie = context?.Request?.Cookies["auth_cookie"];
        if (!string.IsNullOrEmpty(cookie)) {
           _httpClient.DefaultRequestHeaders.Add("Cookie", $"auth_cookie={cookie}");
        }
    }


    public async Task Connect() {
        var url = _config["SignalR:Url:GlobalChatHub"];
        hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri(url), options => {
                // add the auth_cookie defined in program.cs so the hub can identify the user.
                options.Cookies.Add( 
                    new Uri(_navigation.BaseUri), 
                    new System.Net.Cookie("auth_cookie", _httpContextAccessor.HttpContext!.Request.Cookies
                        .FirstOrDefault(c => c.Key == "auth_cookie").Value)
                );
            })
            .Build();


        hubConnection.On<Home.GlobalChatMessageDto>("ReceiveMessage", async (message) => {
            if (OnMessageCallback != null) {
                await OnMessageCallback.Invoke(message);
            }
        });

        await hubConnection.StartAsync();
        await hubConnection.SendAsync("JoinChat");
    }
    

    public async Task SendMessage(Home.GlobalChatMessageDto message) {
        if (hubConnection!.State == HubConnectionState.Connected) {
            await hubConnection.SendAsync("BroadcastMessage", message);    
        }
        else {
            Console.WriteLine("Failed to send message. Not connected to hub!");
        }
    }

    public async Task<List<Home.GlobalChatMessageDto>> LoadLatestMessages() {
        
        var response = await _httpClient.GetAsync("/api/globalchat");
        var messages = await response.Content.ReadFromJsonAsync<List<Home.GlobalChatMessageDto>>();
       
        return messages ?? new List<Home.GlobalChatMessageDto>();
    }
}
