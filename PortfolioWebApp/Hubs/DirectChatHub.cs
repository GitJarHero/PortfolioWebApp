using Microsoft.AspNetCore.SignalR;
using PortfolioWebApp.Hubs.Connection;
using PortfolioWebApp.Shared;

using MessageAcknowledgedEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Client.MessageAcknowledgedEvent;
using MessageReceivedEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Client.MessageReceivedEvent;
using ServerMessageDeliveredEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Server.MessageDeliveredEvent;
using ClientMessageDeliveredEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Client.MessageDeliveredEvent;
using SendMessageEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Server.SendMessageEvent;
using ServerMessageReadEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Server.MessageReadEvent;
using ClientMessageReadEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Client.MessageReadEvent;

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

    public async Task SendMessage(SendMessageEvent evnt) {
        
        // TODO persist the message but 'delivered' and 'read' dates are null
        
        await Clients.User(evnt.Payload.To.id.ToString())
            .SendHubEventAsync(new MessageReceivedEvent(evnt.Payload));
        await Clients.User(evnt.Payload.From.id.ToString())
            .SendHubEventAsync(new MessageAcknowledgedEvent(evnt.Payload));
    }

    public async Task SendMessageDelivered(ServerMessageDeliveredEvent evnt) {
        // TODO set the 'delivery' date and update the message in the db
        
        await Clients.User(evnt.Payload.SenderUserId.ToString())
            .SendHubEventAsync(new ClientMessageDeliveredEvent(evnt.Payload));
    }

    public async Task SendMessageRead(ServerMessageReadEvent evnt) {
        // TODO set the 'read' date and update the message in the db
        
        await Clients.User(evnt.Payload.SenderUserId.ToString())
            .SendHubEventAsync(new ClientMessageReadEvent(evnt.Payload));
    }

}