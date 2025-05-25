using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PortfolioWebApp.Hubs.Connection;
using PortfolioWebApp.Models.Entities;
using PortfolioWebApp.Services;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Hubs;

[AllowAnonymous]
public class NotificationHub : Hub {
    
    private readonly ILogger<NotificationHub> _logger;
    private readonly INotificationConnectionStorage _storage;
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
    public async Task SendFriendRequest(FriendShipRequestDto request) {
        var senderUser = _userService.FindUserByName(request.from);
        var sender = senderUser.UserName;
        
        if (string.IsNullOrEmpty(sender) || sender == "Anonymous") {
            _logger.LogError("Failed to retrieve user name from context (ConnectionId: {connectionId})", Context.ConnectionId);
            throw new Exception("User not authenticated.");
        }
        
        var targetUser = _userService.FindUserByName(request.to);
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
        
        await Clients.User(targetUser.Id.ToString()).SendAsync("ReceiveFriendRequest", request);
        
        // tells the sender that he can now refresh his friend requests / friend list
        _logger.LogInformation("Sending 'SendFriendRequestAck' Event to user: {from}", sender);
        await Clients.User(senderUser.Id.ToString()).SendAsync("SendFriendRequestAck", request);
    }

    // missing security check: request.from = session.user & is there a request in the db for that answer ?
    public async Task SendFriendRequestAnswer(FriendShipRequestAnswerDto answer) {

        var from = _userService.FindUserByName(answer.request.from);
        var to = _userService.FindUserByName(answer.request.to);
        
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
        await Clients.User(from.Id.ToString()).SendAsync("ReceiveFriendRequestAnswer", answer);
        
        // tells the sender that he can now refresh his friend requests / friend list
        _logger.LogInformation("Sending 'SendFriendRequestAnswerAck' Event to user: {from}", from.UserName);
        await Clients.User(to.Id.ToString()).SendAsync("SendFriendRequestAnswerAck", answer);
    }

    // missing security check: request.from = session.user ?
    public async Task SendFriendRequestCancellation(FriendShipRequestDto request) {
        var from = _userService.FindUserByName(request.from);
        var to = _userService.FindUserByName(request.to);
        _friendRequestService.Delete(request);
        
        _logger.LogInformation("Sending 'ReceiveFriendRequestCancellation' Event to users: {from}, {to}", request.from, request.to);;
        await Clients.Users(from.Id.ToString(), to.Id.ToString()).SendAsync("ReceiveFriendRequestCancellation", request);
    }
}