using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using PortfolioWebApp.Hubs.Connection;
using PortfolioWebApp.Models;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Hubs;

public class DirectChatHub : Hub {

    private readonly ILogger<DirectChatHub> _logger;
    private readonly IDirectChatConnectionStorage _storage; // probably not needed since it's ok to send a message to an offline user
    
    public DirectChatHub(ILogger<DirectChatHub> logger, IDirectChatConnectionStorage storage) {
        _logger = logger;
        _storage = storage;
    }

    public override Task OnDisconnectedAsync(Exception? exception) {
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";
        _logger.LogDebug("User disconnected: {Username} (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
    
    public override Task OnConnectedAsync() {
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";
        _logger.LogDebug("User connected: {Username} (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public async Task SendMessage(DirectMessageDto message) {
        
        // TODO
        
       // await Clients.User(message.To.id.ToString()).SendAsync("ReceiveMessage", message);
       // await Clients.User(message.From.id.ToString()).SendAsync("MessageSentAcknowledgement", message);
    }

    public async Task SendMessageDelivered(MessageDeliveredDto message) {
        // TODO
    }

    public async Task SendMessageRead(MessageReadDto message) {
        // TODO
    }

}