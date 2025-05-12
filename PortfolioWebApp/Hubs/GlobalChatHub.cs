using Microsoft.AspNetCore.SignalR;

namespace PortfolioWebApp.Hubs;

public class GlobalChatHub : Hub {
    
    public override Task OnDisconnectedAsync(Exception? exception) {
        var username = Context.User?.Identity?.IsAuthenticated == true
            ? Context.User.Identity.Name
            : "Anonymous";

        Console.WriteLine($"User disconnected: {username} (ConnectionId: {Context.ConnectionId})");
        return base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat() {
        var username = Context.User?.Identity?.IsAuthenticated == true
            ? Context.User.Identity.Name
            : "Anonymous";

        Console.WriteLine($"User connected: {username} (ConnectionId: {Context.ConnectionId})");
    }

    public async Task BroadcastMessage(string fromUser, string message) {
        await Clients.All.SendAsync("ReceiveMessage", fromUser, message);
    }
}