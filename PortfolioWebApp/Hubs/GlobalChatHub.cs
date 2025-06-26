using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MessageReceivedEvent = PortfolioWebApp.Shared.HubEvents.GlobalChat.Client.MessageReceivedEvent;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Hubs;

public class GlobalChatHub : Hub {
    
    private readonly AppDbContext _dbContext;
    private readonly ILogger<GlobalChatHub> _logger;

    public GlobalChatHub(AppDbContext dbContext, ILogger<GlobalChatHub> logger) {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override Task OnDisconnectedAsync(Exception? exception) {
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";
        
        _logger.LogInformation("User disconnected: {Username} (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public override Task OnConnectedAsync() {
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";

        _logger.LogInformation("User connected: {Username} (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        return base.OnConnectedAsync();
    }



    public async Task BroadcastMessage(HubEvents.GlobalChat.Server.BroadCastEvent e) {
        var userName = Context.User?.Identity?.Name;
        var messageDto = e.Payload;
        
        if (string.IsNullOrEmpty(userName)) {
            _logger.LogError("Failed to retrieve user name from context (ConnectionId: {ConnectionId})", Context.ConnectionId);
            throw new Exception("User not authenticated.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);

        if (user == null) {
            _logger.LogError("User not found in database: {UserName} (ConnectionId: {ConnectionId})", userName, Context.ConnectionId);
            throw new Exception("User not found.");
        }

        var message = new GlobalMessage {
            Content = messageDto.Content,
            Created = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            User = user,
            State = State.Active
        };

        _dbContext.GlobalMessages.Add(message);
        await _dbContext.SaveChangesAsync();
        
        // add missing data to the UserDto
        var completeUserDto = new UserDto(message.User.UserName, user.Id, user.ProfileColor);
        messageDto = messageDto with { User = completeUserDto };

        _logger.LogDebug("messageDto is: " + messageDto);
        await Clients.All.SendHubEventAsync(new MessageReceivedEvent(messageDto));
    }
}
