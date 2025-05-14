using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using PortfolioWebApp.Models;

namespace PortfolioWebApp.Hubs;

public class DirectChatHub(ILogger<GlobalChatHub> logger) : Hub {
    
    public static ConcurrentDictionary<string, string> CircuitToConnectionMap = new(); // CircuitId -> ConnectionId

    public override Task OnDisconnectedAsync(Exception? exception) {
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";
        
        logger.LogInformation("User disconnected: {Username} (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
    
    public override Task OnConnectedAsync() {
        // debug logs
        var username = Context.User?.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : "Anonymous";
        logger.LogInformation("User connected: {Username} (ConnectionId: {ConnectionId})", username, Context.ConnectionId);
        
        // save the circuitId & connectionId
        var circuitId = Context.GetHttpContext()?.Request.Query["circuitId"].ToString(); // von Client übergeben
        if (!string.IsNullOrEmpty(circuitId)) {
            CircuitToConnectionMap[circuitId] = Context.ConnectionId;
        }
        
        return base.OnConnectedAsync();
    }
    
    
    
}