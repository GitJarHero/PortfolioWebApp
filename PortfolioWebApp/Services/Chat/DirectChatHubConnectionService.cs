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
        var url = _config["SignalR:ConnectionUrl:DirectChatHub"];
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
        
        
        _hubConnection.OnHubEvent<ClientEvents.MessageReceivedEvent>(async (message) => {
            ReceiveMessage?.Invoke(message.Payload);
        });
        _hubConnection.OnHubEvent<ClientEvents.MessageAcknowledgedEvent>(async (message) => {
            MessageSentAcknowledgement?.Invoke(message.Payload);
        });
        _hubConnection.OnHubEvent<ClientEvents.MessageDeliveredEvent>(async (message) => {
            MessageDelivered?.Invoke(message.Payload);
        });
        _hubConnection.OnHubEvent<ClientEvents.MessageReadEvent>(async (message) => {
            MessageRead?.Invoke(message.Payload);
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
            await _hubConnection.SendHubEventAsync(new ServerEvents.SendMessageEvent(message));
        } 
        else {
            throw new Exception("Cannot send message. Not connected");
        }
    }
    
    public async Task SendMessageDelivered(MessageDeliveredDto dto) {
        if (_hubConnection is { State: HubConnectionState.Connected }) {
            await _hubConnection.SendHubEventAsync(new ServerEvents.MessageDeliveredEvent(dto));
        } 
        else {
            throw new Exception("Cannot send message. Not connected");
        }
    }
    
    public async Task SendMessageRead(MessageReadDto dto) {
        if (_hubConnection is { State: HubConnectionState.Connected }) {
            await _hubConnection.SendHubEventAsync(new ServerEvents.MessageReadEvent(dto));
        } 
        else {
            throw new Exception("Cannot send message. Not connected");
        }
    }
}