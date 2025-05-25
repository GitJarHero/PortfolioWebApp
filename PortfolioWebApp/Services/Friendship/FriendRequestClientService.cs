using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services;

using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

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

        _hubConnection.On("ReceiveFriendRequest", (FriendShipRequestDto request) => { 
            _logger.LogInformation("Got a 'ReceiveFriendRequest' Event. Request is: {request}", request.ToString());
            ReceiveFriendRequestEventCallback?.Invoke(request);
        });
        
        _hubConnection.On("SendFriendRequestAck", (FriendShipRequestDto request) => { 
            _logger.LogInformation("Got a 'SendFriendRequestAck' Event. Acknowledged request is: {request}", request.ToString());
            SendFriendRequestAckEventCallback?.Invoke(request);
        });
        
        _hubConnection.On("ReceiveFriendRequestAnswer", (FriendShipRequestAnswerDto answer) => { 
            _logger.LogInformation("Got a 'ReceiveFriendRequest' Event. Answer is: {answer}", answer.ToString());
            ReceiveFriendRequestAnswerEventCallback?.Invoke(answer);
        });
        
        _hubConnection.On("SendFriendRequestAnswerAck", (FriendShipRequestAnswerDto answer) => {
            _logger.LogInformation("Got a 'SendFriendRequestAnswerAck' Event. Acknowledged answer is: {answer}", answer.ToString());
            SendFriendRequestAnswerAckEventCallback?.Invoke(answer);
        });

        _hubConnection.On("ReceiveFriendRequestCancellation", (FriendShipRequestDto request) => {
            _logger.LogInformation("Got a 'ReceiveFriendRequestCancellation' Event. Cancellation is: {request}", request.ToString());
            ReceiveFriendRequestCancellationEventCallback?.Invoke(request);
        });

        await _hubConnection.StartAsync();
        _logger.LogDebug("Connected to Hub");
    }


    public async Task SendRequestTo(string targetUser) {
        var me = _httpContextAccessor.HttpContext?.User.Identity?.Name;
        var requestDto = new FriendShipRequestDto(me, targetUser, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified));
        
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected) {
            await _hubConnection.SendAsync("SendFriendRequest", requestDto);   
        }
    }
    
    public async Task AnswerFriendRequest(FriendShipRequestDto request, bool accept) {
        
        var answer = new FriendShipRequestAnswerDto(request, accept);
        
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected) {
            await _hubConnection.SendAsync("SendFriendRequestAnswer", answer);   
        }
        
    }
    
    public async Task CancelFriendRequest(FriendShipRequestDto request) {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected) {
            await _hubConnection.SendAsync("SendFriendRequestCancellation", request);
        }
    }
    
    
}
