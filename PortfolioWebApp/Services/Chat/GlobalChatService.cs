using Microsoft.AspNetCore.SignalR;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Hubs;

namespace PortfolioWebApp.Services;

public class GlobalChatService {
    
    private readonly AppDbContext _dbContext;
    private readonly IHubContext<GlobalChatHub> _globalChatHubContext;

    public GlobalChatService(AppDbContext dbContext, IHubContext<GlobalChatHub> hubContext) {
        _dbContext = dbContext;
        _globalChatHubContext = hubContext;
    }

    public async Task SendMessage(GlobalMessage message) {
        await SaveMessage(message);
        await _globalChatHubContext.Clients.All.SendAsync("ReceiveMessage", message.User.UserName, message.Content);
    }

    public async Task<List<GlobalMessage>> LoadLatestMessages(int n) {
        return await _dbContext.GlobalMessages
            .Include(m => m.User)
            .OrderByDescending(m => m.Created)
            .Take(n)
            .ToListAsync();
    }

    private async Task SaveMessage(GlobalMessage message) {
        _dbContext.GlobalMessages.Add(message);
        await _dbContext.SaveChangesAsync();
    }
}