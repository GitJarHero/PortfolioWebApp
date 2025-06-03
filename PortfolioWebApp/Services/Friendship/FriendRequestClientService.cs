using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services;

using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

using ServerEvents = HubEvents.FriendRequest.Server;
using ClientEvents = HubEvents.FriendRequest.Client;

public class FriendRequestClientService {
    
    private readonly NavigationManager _navigation;
    private readonly IJSRuntime _jsRuntime;
    private HubConnection? _hubConnection;
    private IConfiguration _config;
    private readonly ILogger<FriendRequestClientService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    // invoked, when a friend request is received.
    public event Action<FriendShipRequestDto>? ReceiveFriendRequestEventCallback;
    
    // invoked, when the server acknowledges the friend request.
    public event Action<FriendShipRequestDto>? SendFriendRequestAckEventCallback;
    
    // invoked, when an answer to a friend request is received.
    public event Action<FriendShipRequestAnswerDto>? ReceiveFriendRequestAnswerEventCallback;
    
    // invoked, when the server acknowledges the answer to a friend request.
    public event Action<FriendShipRequestAnswerDto>? SendFriendRequestAnswerAckEventCallback;
    
    // invoked, when the server cancelled that friend request.
    public event Action<FriendShipRequestDto>? ReceiveFriendRequestCancellationEventCallback;

    public FriendRequestClientService(NavigationManager navigation, 
                                        IJSRuntime jsRuntime, 
                                        IConfiguration config,
                                        IHttpContextAccessor httpContextAccessor,
                                        ILogger<FriendRequestClientService> logger) {
        _navigation = navigation;
        _jsRuntime = jsRuntime;
        _config = config;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task StartAsync() {
        var url = _config["SignalR:Url:NotificationHub"];
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

        _hubConnection.OnHubEvent<ClientEvents.ReceiveFriendRequestEvent>(evnt => {
            var request = evnt.Payload;
            _logger.LogInformation("Got a 'ReceiveFriendRequest' Event. Request is: {request}", request.ToString());
            ReceiveFriendRequestEventCallback?.Invoke(request);
        });
        
        _hubConnection.OnHubEvent<ClientEvents.FriendRequestSentAcknowledgedEvent>(evnt => {
            var request = evnt.Payload;
            _logger.LogInformation("Got a 'SendFriendRequestAck' Event. Acknowledged request is: {request}", request.ToString());
            SendFriendRequestAckEventCallback?.Invoke(request);
        });
        
        _hubConnection.OnHubEvent<ClientEvents.ReceiveFriendRequestAnswerEvent>(evnt => {
            var answer = evnt.Payload;
            _logger.LogInformation("Got a 'ReceiveFriendRequest' Event. Answer is: {answer}", answer.ToString());
            ReceiveFriendRequestAnswerEventCallback?.Invoke(answer);
        });
        
        _hubConnection.OnHubEvent<ClientEvents.FriendRequestAnswerAcknowledgedEvent>(evnt => {
            var answer = evnt.Payload;
            _logger.LogInformation("Got a 'SendFriendRequestAnswerAck' Event. Acknowledged answer is: {answer}", answer.ToString());
            SendFriendRequestAnswerAckEventCallback?.Invoke(answer);
        });

        _hubConnection.OnHubEvent<ClientEvents.ReceiveFriendRequestCancellationEvent>(evnt => {
            var cancellation =  evnt.Payload;
            _logger.LogInformation("Got a 'ReceiveFriendRequestCancellation' Event. Cancellation is: {request}", cancellation.ToString());
            ReceiveFriendRequestCancellationEventCallback?.Invoke(cancellation);
        });

        await _hubConnection.StartAsync();
    }


    public async Task SendRequestTo(string targetUser) {
        var myName = _httpContextAccessor.HttpContext?.User.Identity?.Name;
        var me = new UserDto(myName,0,"");          // here we only know the names. the hub will fill the remaining parts of the dto and send a SendFriendRequestAck
        var target = new UserDto(targetUser,0,"");
        var requestDto = new FriendShipRequestDto(me, target, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified));
        
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected) {
            await _hubConnection.SendHubEventAsync(new ServerEvents.SendFriendRequestEvent(requestDto));   
        }
    }
    
    public async Task AnswerFriendRequest(FriendShipRequestDto request, bool accept) {
        
        var answer = new FriendShipRequestAnswerDto(request, accept);
        
        if (_hubConnection is { State: HubConnectionState.Connected }) {
            await _hubConnection.SendHubEventAsync(new ServerEvents.SendFriendRequestAnswerEvent(answer));   
        }
        
    }
    
    public async Task CancelFriendRequest(FriendShipRequestDto request) {
        if (_hubConnection is { State: HubConnectionState.Connected }) {
            await _hubConnection.SendHubEventAsync(new ServerEvents.SendFriendRequestCancellationEvent(request));
        }
    }
    
    
}
