using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PortfolioWebApp.Hubs.Connection;
using PortfolioWebApp.Models.Entities;
using PortfolioWebApp.Services;
using PortfolioWebApp.Shared;
using ServerEvents = PortfolioWebApp.Shared.HubEvents.FriendRequest.Server;
using ClientEvents = PortfolioWebApp.Shared.HubEvents.FriendRequest.Client;

namespace PortfolioWebApp.Hubs;

[AllowAnonymous]
public class NotificationHub : Hub {
    
    private readonly ILogger<NotificationHub> _logger;
    private readonly INotificationConnectionStorage _storage;  // probably not needed since it's ok to send a message to an offline user
    private readonly FriendRequestService _friendRequestService;
    private readonly FriendshipService _friendshipService;
    private readonly UserService _userService;

    public NotificationHub( ILogger<NotificationHub> logger, 
                            INotificationConnectionStorage storage, 
                            FriendRequestService friendRequestService,
                            UserService userService,
                            FriendshipService friendshipService) {
        _logger = logger;
        _storage = storage;
        _friendRequestService = friendRequestService;
        _userService = userService;
        _friendshipService = friendshipService;
    }
    
    public override Task OnDisconnectedAsync(Exception? exception) {
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";
        _logger.LogInformation("{username} disconnected (ConnectionId: {connectionId})", username, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public override Task OnConnectedAsync() {
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";
        _logger.LogInformation("{username} connected (ConnectionId: {connectionId})", username, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    // missing security check: request.from = session.user ?
    public async Task SendFriendRequest(ServerEvents.SendFriendRequestEvent evnt) {
        var request = evnt.Payload;
        var senderUser = _userService.FindUserByName(request.from.username);
        var sender = senderUser.UserName;
        
        if (string.IsNullOrEmpty(sender) || sender == "Anonymous") {
            _logger.LogError("Failed to retrieve user name from context (ConnectionId: {connectionId})", Context.ConnectionId);
            throw new Exception("User not authenticated.");
        }
        
        var targetUser = _userService.FindUserByName(request.to.username);
        if (targetUser == null) {
            throw new Exception("Target User not found: " + request.to);
        }

        try {
            _friendRequestService.Save(request);
        } 
        catch (Exception e) {
            _logger.LogError(e.InnerException?.Message);
            return;
        }
        
        // add missing data to the request because the client filled the request with only usernames
        var from = new UserDto(request.from.username, senderUser.Id, senderUser.ProfileColor);
        var to = new UserDto(request.to.username, targetUser.Id, targetUser.ProfileColor);
        request = new FriendShipRequestDto(from, to, request.ceated);
        
        await Clients.User(targetUser.Id.ToString())
            .SendHubEventAsync(new ClientEvents.ReceiveFriendRequestEvent(request));
        
        // tells the sender that he can now refresh his friend requests / friend list
        _logger.LogInformation("Sending 'SendFriendRequestAck' Event to user: {from}", sender);
        await Clients.User(senderUser.Id.ToString())
            .SendHubEventAsync(new ClientEvents.FriendRequestSentAcknowledgedEvent(request));
    }

    // missing security check: request.from = session.user & is there a request in the db for that answer ?
    public async Task SendFriendRequestAnswer(ServerEvents.SendFriendRequestAnswerEvent evnt) {
        var answer = evnt.Payload;
        var from = _userService.FindUserByName(answer.request.from.username);
        var to = _userService.FindUserByName(answer.request.to.username);
        
        if (answer.accepted) {
            var friendShip = new Friendship {
                Created = answer.request.ceated,
                User1 = from,
                User2 = to,
            };
            _friendshipService.Save(friendShip);
            _logger.LogInformation("Friendrequest was accepted: from={from} to={to}", answer.request.from, answer.request.to);;;
        }
        _friendRequestService.Delete(answer.request);
        
        _logger.LogInformation("Sending 'ReceiveFriendRequestAnswer' Event to user: {to}", to.UserName);
        await Clients.User(from.Id.ToString())
            .SendHubEventAsync(new ClientEvents.ReceiveFriendRequestAnswerEvent(answer));
        
        // tells the sender that he can now refresh his friend requests / friend list
        _logger.LogInformation("Sending 'SendFriendRequestAnswerAck' Event to user: {from}", from.UserName);
        await Clients.User(to.Id.ToString())
            .SendHubEventAsync(new ClientEvents.FriendRequestAnswerAcknowledgedEvent(answer));
    }

    // missing security check: request.from = session.user ?
    public async Task SendFriendRequestCancellation(ServerEvents.SendFriendRequestCancellationEvent evnt) {
        var eventData = evnt.Payload;
        var from = _userService.FindUserByName(eventData.from.username);
        var to = _userService.FindUserByName(eventData.to.username);
        
        _friendRequestService.Delete(eventData);
        
        _logger.LogInformation("Sending a ReceiveFriendRequestCancellationEvent to users: {from}, {to}", eventData.from, eventData.to);;
        await Clients.Users(from.Id.ToString(), to.Id.ToString())
            .SendHubEventAsync(new ClientEvents.ReceiveFriendRequestCancellationEvent(eventData));
    }
}