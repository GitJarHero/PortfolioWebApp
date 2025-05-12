

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PortfolioWebApp.Hubs;

[AllowAnonymous]
public class GlobalChatHub : Hub
{
    public override Task OnDisconnectedAsync(Exception? exception) {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat() {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
    }

    public async Task BroadcastMessage(string user, string message) {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}