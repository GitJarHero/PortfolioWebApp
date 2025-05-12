using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace PortfolioWebApp.Services;

public class GlobalChatService {
    
    private readonly AppDbContext _dbContext;

    public GlobalChatService(AppDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task SendMessage(GlobalMessage message) {
        // TODO: Optional: Send message to SignalR hub here
        await SaveMessage(message);
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