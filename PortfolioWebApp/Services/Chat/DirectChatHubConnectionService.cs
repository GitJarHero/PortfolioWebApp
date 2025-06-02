using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using ClientEvents = PortfolioWebApp.Shared.HubEvents.DirectChat.Client;
using ServerEvents = PortfolioWebApp.Shared.HubEvents.DirectChat.Server;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services.Chat;

public class DirectChatHubConnectionService : IDirectChatHubConnectionService {

    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly NavigationManager _navigation;
    private readonly ILogger<DirectChatHubConnectionService> _logger;
    
    public event Action<DirectMessageDto>? ReceiveMessage;
    public event Action<DirectMessageDto>? MessageSentAcknowledgement;
    public event Action<MessageDeliveredDto>? MessageDelivered;
    public event Action<MessageReadDto>? MessageRead;
    
    private HubConnection? _hubConnection;

    public DirectChatHubConnectionService(
        NavigationManager navigation, 
        IConfiguration config,
        IHttpContextAccessor httpContextAccessor,
        ILogger<DirectChatHubConnectionService> logger
    ) {
        _config = config;
        _httpContextAccessor = httpContextAccessor;
        _navigation = navigation;
        _logger = logger;
    }
    
    public async Task Connect() {
        var url = _config["SignalR:Url:DirectChatHub"];
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri(url), options => {
                // add the auth_cookie defined in program.cs so the hub can identify the user.
                options.Cookies.Add( 
                    new Uri(_navigation.BaseUri), 
                    new System.Net.Cookie("auth_cookie", _httpContextAccessor.HttpContext!.Request.Cookies
                        .FirstOrDefault(c => c.Key == "auth_cookie").Value)
                );
            })
            .Build();
        
        _hubConnection.On<DirectMessageDto>(ClientEvents.MessageReceived, (message) => {
            ReceiveMessage?.Invoke(message);
        });
        _hubConnection.On<DirectMessageDto>(ClientEvents.MessageAcknowledged, (message) => {
            MessageSentAcknowledgement?.Invoke(message);
        });
        _hubConnection.On<MessageDeliveredDto>(ClientEvents.MessageDelivered, (message) => {
            MessageDelivered?.Invoke(message);
        });
        _hubConnection.On<MessageReadDto>(ClientEvents.MessageRead, (message) => {
            MessageRead?.Invoke(message);
        });
        
        await _hubConnection.StartAsync();
    }
    
    public async Task Disconnect() {
        if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected) {
            await _hubConnection.DisposeAsync();
        }
    }
    
    public async Task Send(DirectMessageDto message) {
        if (_hubConnection is { State: HubConnectionState.Connected }) {
            await _hubConnection.SendAsync(ServerEvents.Send, message);
        } 
        else {
            throw new Exception("Cannot send message. Not connected");
        }
    }
    
    public async Task SendMessageDelivered(MessageDeliveredDto dto) {
        if (_hubConnection is { State: HubConnectionState.Connected }) {
            await _hubConnection.SendAsync(ServerEvents.SendDelivered, dto);
        } 
        else {
            throw new Exception("Cannot send message. Not connected");
        }
    }
    
    public async Task SendMessageRead(MessageReadDto dto) {
        if (_hubConnection is { State: HubConnectionState.Connected }) {
            await _hubConnection.SendAsync(ServerEvents.SendRead, dto);
        } 
        else {
            throw new Exception("Cannot send message. Not connected");
        }
    }
}