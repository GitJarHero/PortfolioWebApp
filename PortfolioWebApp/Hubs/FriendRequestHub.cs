using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Hubs;

[AllowAnonymous]
public class FriendRequestHub : Hub {
    
    private readonly ILogger<GlobalChatHub> _logger;

    public FriendRequestHub(ILogger<GlobalChatHub> logger) {
        _logger = logger;
    }
    
    public override Task OnDisconnectedAsync(Exception? exception) {
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";
        _logger.LogInformation("[FriendRequestHub]: {Username} disconnected (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public override Task OnConnectedAsync() {
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";

        _logger.LogInformation("[FriendRequestHub]: {Username} connected (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public async Task SendFriendRequest(FriendShipRequestDto request) {
        var sender = Context.User?.Identity?.Name;
        if (string.IsNullOrEmpty(sender)) {
            _logger.LogError("[FriendRequestHub]: Failed to retrieve user name from context (ConnectionId: {ConnectionId})", Context.ConnectionId);
            throw new Exception("User not authenticated.");
        }
        
        // use FriendShipRequestService to save the firendship request.

        await Clients.User(request.to).SendAsync("ReceiveFriendRequest", request);
    }    
}