using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using PortfolioWebApp.Hubs.Connection;
using PortfolioWebApp.Models;

namespace PortfolioWebApp.Hubs;

public class DirectChatHub : Hub {

    private readonly ILogger<DirectChatHub> _logger;
    private readonly IDirectChatConnectionStorage _storage;
    
    public DirectChatHub(ILogger<DirectChatHub> logger, IDirectChatConnectionStorage storage) {
        _logger = logger;
        _storage = storage;
    }

    public override Task OnDisconnectedAsync(Exception? exception) {
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";
        
        _logger.LogInformation("User disconnected: {Username} (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
    
    public override Task OnConnectedAsync() {
        // debug logs
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";
        _logger.LogInformation("User connected: {Username} (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        
        return base.OnConnectedAsync();
    }
    
}