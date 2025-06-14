using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services;

using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

using ServerEvents = HubEvents.FriendRequest.Server;
using ClientEvents = HubEvents.FriendRequest.Client;

public class FriendRequestClientService
{
    private readonly NavigationManager _navigation;
    private readonly IJSRuntime _jsRuntime;
    private HubConnection? _hubConnection;
    private IConfiguration _config;
    private readonly ILogger<FriendRequestClientService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public event Func<FriendShipRequestDto, Task>? ReceiveFriendRequestEventCallback;
    public event Func<FriendShipRequestDto, Task>? SendFriendRequestAckEventCallback;
    public event Func<FriendShipRequestAnswerDto, Task>? ReceiveFriendRequestAnswerEventCallback;
    public event Func<FriendShipRequestAnswerDto, Task>? SendFriendRequestAnswerAckEventCallback;
    public event Func<FriendShipRequestDto, Task>? ReceiveFriendRequestCancellationEventCallback;

    public FriendRequestClientService(NavigationManager navigation,
                                      IJSRuntime jsRuntime,
                                      IConfiguration config,
                                      IHttpContextAccessor httpContextAccessor,
                                      ILogger<FriendRequestClientService> logger)
    {
        _navigation = navigation;
        _jsRuntime = jsRuntime;
        _config = config;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        var url = _config["SignalR:ConnectionUrl:NotificationHub"];
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri(url), options =>
            {
                options.Cookies.Add(
                    new Uri(_navigation.BaseUri),
                    new System.Net.Cookie("auth_cookie", _httpContextAccessor.HttpContext!.Request.Cookies
                        .FirstOrDefault(c => c.Key == "auth_cookie").Value)
                );
            })
            .Build();

        _hubConnection.OnHubEvent<ClientEvents.ReceiveFriendRequestEvent>(async evnt =>
        {
            var request = evnt.Payload;
            _logger.LogInformation("Got a 'ReceiveFriendRequest' Event. Request is: {request}", request.ToString());
            if (ReceiveFriendRequestEventCallback != null)
                await ReceiveFriendRequestEventCallback.Invoke(request);
        });

        _hubConnection.OnHubEvent<ClientEvents.FriendRequestSentAcknowledgedEvent>(async evnt =>
        {
            var request = evnt.Payload;
            _logger.LogInformation("Got a 'SendFriendRequestAck' Event. Acknowledged request is: {request}", request.ToString());
            if (SendFriendRequestAckEventCallback != null)
                await SendFriendRequestAckEventCallback.Invoke(request);
        });

        _hubConnection.OnHubEvent<ClientEvents.ReceiveFriendRequestAnswerEvent>(async evnt =>
        {
            var answer = evnt.Payload;
            _logger.LogInformation("Got a 'ReceiveFriendRequest' Event. Answer is: {answer}", answer.ToString());
            if (ReceiveFriendRequestAnswerEventCallback != null)
                await ReceiveFriendRequestAnswerEventCallback.Invoke(answer);
        });

        _hubConnection.OnHubEvent<ClientEvents.FriendRequestAnswerAcknowledgedEvent>(async evnt =>
        {
            var answer = evnt.Payload;
            _logger.LogInformation("Got a 'SendFriendRequestAnswerAck' Event. Acknowledged answer is: {answer}", answer.ToString());
            if (SendFriendRequestAnswerAckEventCallback != null)
                await SendFriendRequestAnswerAckEventCallback.Invoke(answer);
        });

        _hubConnection.OnHubEvent<ClientEvents.ReceiveFriendRequestCancellationEvent>(async evnt =>
        {
            var cancellation = evnt.Payload;
            _logger.LogInformation("Got a 'ReceiveFriendRequestCancellation' Event. Cancellation is: {request}", cancellation.ToString());
            if (ReceiveFriendRequestCancellationEventCallback != null)
                await ReceiveFriendRequestCancellationEventCallback.Invoke(cancellation);
        });

        await _hubConnection.StartAsync();
    }

    public async Task SendRequestTo(string targetUser)
    {
        var myName = _httpContextAccessor.HttpContext?.User.Identity?.Name;
        var me = new UserDto(myName, 0, "");
        var target = new UserDto(targetUser, 0, "");
        var requestDto = new FriendShipRequestDto(me, target, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified));

        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendHubEventAsync(new ServerEvents.SendFriendRequestEvent(requestDto));
        }
    }

    public async Task AnswerFriendRequest(FriendShipRequestDto request, bool accept)
    {
        var answer = new FriendShipRequestAnswerDto(request, accept);
        if (_hubConnection is { State: HubConnectionState.Connected })
        {
            await _hubConnection.SendHubEventAsync(new ServerEvents.SendFriendRequestAnswerEvent(answer));
        }
    }

    public async Task CancelFriendRequest(FriendShipRequestDto request)
    {
        if (_hubConnection is { State: HubConnectionState.Connected })
        {
            await _hubConnection.SendHubEventAsync(new ServerEvents.SendFriendRequestCancellationEvent(request));
        }
    }
}
