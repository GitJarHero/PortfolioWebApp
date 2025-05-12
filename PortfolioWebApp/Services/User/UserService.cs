using Microsoft.AspNetCore.Components;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;

namespace PortfolioWebApp.Services;

public class UserService {
    
    private readonly AppDbContext _dbContext;
    
    public UserService(AppDbContext dbContext) {
        _dbContext = dbContext;
    }

    public List<User> FindUsersByName(string name) {
        return _dbContext.Users
            .Where(u => u.UserName.ToLower().Contains(name.ToLower())).ToList();
    }
    
}