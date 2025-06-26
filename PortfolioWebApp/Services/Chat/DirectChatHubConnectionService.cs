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
    private readonly IDirectChatStorageService  _storageService;
    
    // The subscriber (Chats.razor) uses these events to check,
    // if the message belongs to the active chat (if there is an active chat)
    // or if it belongs to a chatPreview and rerender either the
    // corresponding chat preview or rerender the active chat.
    
    // tells subscriber(i.e. Chats.razor) that a message was received and added to the DirectChatStorageService
    public event Action<DirectMessageDto>? ReceiveMessage;
    // tells subscriber(i.e. Chats.razor) that a message was sent successfully
    public event Action<DirectMessageDto>? MessageSentAcknowledgement;
    // tells subscriber(i.e. Chats.razor) that a message(s) was delivered updated in the DirectChatStorageService
    public event Action<MessageDeliveredDto>? MessageDelivered;
    // tells subscriber(i.e. Chats.razor) that a message(s) was read updated in the DirectChatStorageService
    public event Action<MessageReadDto>? MessageRead;
    
    private HubConnection? _hubConnection;

    public DirectChatHubConnectionService(
        NavigationManager navigation, 
        IConfiguration config,
        IHttpContextAccessor httpContextAccessor,
        ILogger<DirectChatHubConnectionService> logger,
        IDirectChatStorageService storageService
    ) {
        _config = config;
        _httpContextAccessor = httpContextAccessor;
        _navigation = navigation;
        _logger = logger;
        _storageService = storageService;
        _logger.LogInformation("DirectChatHubConnectionService initialized");
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
            if (_httpContextAccessor.HttpContext is null) {
                _logger.LogWarning("User not logged in anymore. Closing the Hub Connection");
                await _hubConnection.DisposeAsync();
                return;
            }
            _storageService.HandleReceiveMessage(message.Payload); // add incoming message to storage
            ReceiveMessage?.Invoke(message.Payload);
            _hubConnection.SendHubEventAsync(new ServerEvents.MessageDeliveredEvent(
                new MessageDeliveredDto(
                    [message.Payload.MessageId],
                    DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                )
            ));
        });
        _hubConnection.OnHubEvent<ClientEvents.MessageAcknowledgedEvent>(async (message) => {
            if (_httpContextAccessor.HttpContext is null) {
                _logger.LogWarning("User not logged in anymore. Closing the Hub Connection");
                await _hubConnection.DisposeAsync();
                return;
            }
            _storageService.HandleMessageSentAcknowledgement(message.Payload); // add outgoing message to storage
            MessageSentAcknowledgement?.Invoke(message.Payload);
        });
        _hubConnection.OnHubEvent<ClientEvents.MessageDeliveredEvent>(async (message) => {
            if (_httpContextAccessor.HttpContext is null) {
                _logger.LogWarning("User not logged in anymore. Closing the Hub Connection");
                await _hubConnection.DisposeAsync();
                return;
            }
            _storageService.HandleMessageDelivered(message.Payload); // update delivery date of existing messages in storage
            MessageDelivered?.Invoke(message.Payload);
        });
        _hubConnection.OnHubEvent<ClientEvents.MessageReadEvent>(async (message) => {
            if (_httpContextAccessor.HttpContext is null) {
                _logger.LogWarning("User not logged in anymore. Closing the Hub Connection");
                await _hubConnection.DisposeAsync();
                return;
            }
            _storageService.HandleMessageRead(message.Payload); // update read date pf existing messages in storage
            MessageRead?.Invoke(message.Payload);
        });
        
        await _hubConnection.StartAsync();
    }

    public bool IsConnected() {
        return _hubConnection is { State: HubConnectionState.Connected };
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