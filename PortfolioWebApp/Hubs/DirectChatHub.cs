using Microsoft.AspNetCore.SignalR;
using PortfolioWebApp.Hubs.Connection;
using PortfolioWebApp.Models.Entities;
using PortfolioWebApp.Repositories;
using PortfolioWebApp.Services;
using PortfolioWebApp.Shared;

using MessageAcknowledgedEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Client.MessageAcknowledgedEvent;
using MessageReceivedEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Client.MessageReceivedEvent;
using ServerMessageDeliveredEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Server.MessageDeliveredEvent;
using ClientMessageDeliveredEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Client.MessageDeliveredEvent;
using SendMessageEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Server.SendMessageEvent;
using ServerMessageReadEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Server.MessageReadEvent;
using ClientMessageReadEvent = PortfolioWebApp.Shared.HubEvents.DirectChat.Client.MessageReadEvent;

namespace PortfolioWebApp.Hubs;

/// <summary>
/// Hub for handling real-time direct messaging functionality between users.
/// </summary>
public class DirectChatHub : Hub
{
    private readonly ILogger<DirectChatHub> _logger;
    private readonly IDirectChatConnectionStorage _storage;
    private readonly DirectMessageRepository _directMessageRepository;
    private readonly UserService _userService;

    public DirectChatHub(
        ILogger<DirectChatHub> logger,
        IDirectChatConnectionStorage storage,
        DirectMessageRepository directMessageRepository,
        UserService userService)
    {
        _logger = logger;
        _storage = storage; // Currently unused, reserved for potential connection tracking
        _directMessageRepository = directMessageRepository;
        _userService = userService;
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// Logs the disconnect event for debugging or audit purposes.
    /// </summary>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User?.Identity?.IsAuthenticated == true
            ? Context.User.Identity.Name
            : "Anonymous";

        _logger.LogDebug("User disconnected: {Username} (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// Logs the connection event for debugging or audit purposes.
    /// </summary>
    public override Task OnConnectedAsync()
    {
        var username = Context.User?.Identity?.IsAuthenticated == true
            ? Context.User.Identity.Name
            : "Anonymous";

        _logger.LogDebug("User connected: {Username} (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    /// <summary>
    /// Handles a request to send a direct message from one user to another.
    /// Persists the message and notifies both sender and recipient via SignalR.
    /// </summary>
    public async Task SendMessage(SendMessageEvent evnt)
    {
        _logger.LogDebug("SendMessageEvent fired");

        var fromUser = _userService.FindUserByName(evnt.Payload.From.username);
        var toUser = _userService.FindUserByName(evnt.Payload.To.username);

        var messageEntity = new DirectMessage
        {
            Content = evnt.Payload.Content,
            Created = evnt.Payload.Created,
            FromUser = fromUser,
            ToUser = toUser,
            Delivered = null,
            Read = null
        };

        var savedMessageEntity = await _directMessageRepository.SaveAsync(messageEntity);

        // add missing user data to the event payload
        evnt = new SendMessageEvent(Payload: evnt.Payload with {
            From = new UserDto(fromUser.UserName, fromUser.Id, fromUser.ProfileColor),
            MessageId = savedMessageEntity.Id
        });
        

        // Notify the recipient that a message was received
        await Clients.User(evnt.Payload.To.id.ToString())
            .SendHubEventAsync(new MessageReceivedEvent(evnt.Payload));

        // Acknowledge to the sender that the message was accepted
        await Clients.User(evnt.Payload.From.id.ToString())
            .SendHubEventAsync(new MessageAcknowledgedEvent(evnt.Payload));
    }

    /// <summary>
    /// Called when a client opens the app and marks previously undelivered messages as delivered.
    /// Updates delivery timestamps in the database and notifies each unique sender.
    /// </summary>
    public async Task SendMessageDelivered(ServerMessageDeliveredEvent evnt)
    {
        var messageEntities = await _directMessageRepository.GetAllByIdAsync(evnt.Payload.DeliveredMessages);

        foreach (var msg in messageEntities)
        {
            msg.Delivered = evnt.Payload.TimeStamp;
        }

        await _directMessageRepository.SaveAllAsync(messageEntities);

        // Group messages by sender to notify each one individually
        var groupedBySender = messageEntities.GroupBy(m => m.FromUser.Id);

        foreach (var group in groupedBySender)
        {
            var deliveredMessageIds = group.Select(m => m.Id).ToList();
            var dto = evnt.Payload with { DeliveredMessages = deliveredMessageIds };

            await Clients.User(group.Key.ToString())
                .SendHubEventAsync(new ClientMessageDeliveredEvent(dto));
        }
    }

    /// <summary>
    /// Called when a client opens a chat with unread messages or when receiving a message in an opened chat
    /// Marks the corresponding message(s) as read and notifies the sender.
    /// </summary>
    public async Task SendMessageRead(ServerMessageReadEvent evnt)
    {
        var messageEntities = await _directMessageRepository.GetAllByIdAsync(evnt.Payload.ReadMessages);

        foreach (var msg in messageEntities)
        {
            msg.Read = evnt.Payload.TimeStamp;
        }

        await _directMessageRepository.SaveAllAsync(messageEntities);

        /*
         * Since you can only read in one chat at a time, the event
         * only contains messages from that chat. Therefore, all
         * messages have the same sender and target.
         */
        var senderOfMessages = messageEntities.First().FromUser;

        await Clients.User(senderOfMessages.Id.ToString())
            .SendHubEventAsync(new ClientMessageReadEvent(evnt.Payload));
    }
}
