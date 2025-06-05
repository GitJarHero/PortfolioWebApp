using Microsoft.AspNetCore.Components;
using ServerEvents = PortfolioWebApp.Shared.HubEvents.GlobalChat.Server;
using ClientEvents = PortfolioWebApp.Shared.HubEvents.GlobalChat.Client;
using Microsoft.AspNetCore.SignalR.Client;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services.Chat;

public class GlobalChatService {
    
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _config;
    private readonly NavigationManager _navigation;

    private HubConnection? _hubConnection;
    
    public event Func<GlobalChatMessageDto, Task>? OnMessageCallback;


    
    public GlobalChatService(HttpClient httpClient, 
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration config,
                                NavigationManager navigation) {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _config = config;
        _navigation = navigation;
        
        if (_httpClient.BaseAddress == null) {
            _httpClient.BaseAddress = new Uri(_config["HttpClient:BaseAddress"] 
                ?? throw new InvalidOperationException("HttpClient:BaseAddress not configured"));
        }
        
        var context = _httpContextAccessor.HttpContext;
        var cookie = context?.Request.Cookies["auth_cookie"];
        if (!string.IsNullOrEmpty(cookie)) {
           _httpClient.DefaultRequestHeaders.Add("Cookie", $"auth_cookie={cookie}");
        }
    }


    public async Task Connect() {
        var url = _config["SignalR:ConnectionUrl:GlobalChatHub"];
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri(url), options => {
                // add the auth_cookie defined in program.cs so the hub can identify the user.
                options.Cookies.Add( 
                    new Uri(_navigation.BaseUri), 
                    new System.Net.Cookie("auth_cookie", _httpContextAccessor.HttpContext?.Request.Cookies
                        .FirstOrDefault(c => c.Key == "auth_cookie").Value)
                );
            })
            .Build();


        _hubConnection.OnHubEvent<ClientEvents.MessageReceivedEvent>(async (message) => {
            if (OnMessageCallback != null) {
                await OnMessageCallback.Invoke(message.Payload);
            }
        });

        await _hubConnection.StartAsync();
    }
    

    public async Task SendMessage(GlobalChatMessageDto message) {
        if (_hubConnection?.State == HubConnectionState.Connected) {
            await _hubConnection.SendHubEventAsync(new ServerEvents.BroadCastEvent(message));    
        }
        else {
            Console.WriteLine("Failed to send message. Not connected to hub!");
        }
    }

    public async Task Disconnect() {
        if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected) {
            await _hubConnection.DisposeAsync();
        }
    }
    
    public async Task<List<GlobalChatMessageDto>> LoadLatestMessages() {
        
        var response = await _httpClient.GetAsync(_config["API:GlobalChat"]);
        var messages = await response.Content.ReadFromJsonAsync<List<GlobalChatMessageDto>>();
       
        return messages ?? new List<GlobalChatMessageDto>();
    }
}
