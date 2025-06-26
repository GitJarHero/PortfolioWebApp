using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;

namespace PortfolioWebApp.Repositories;

public class DirectMessageRepository(AppDbContext context)
    : Repository<DirectMessage, int>(context), IDirectMessageRepository {
    
    public async Task<List<DirectMessage>> GetAllByUserAsync(int userId) {
        return await IncludeNavigationProperties(_dbSet)
            .Where(m => m.FromUser.Id == userId || m.ToUser.Id == userId)
            .ToListAsync();
    }
    
}