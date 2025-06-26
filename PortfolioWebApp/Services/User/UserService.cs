using Microsoft.AspNetCore.Components;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;

namespace PortfolioWebApp.Services;

public class UserService {
    
    private readonly AppDbContext _dbContext;
    
    public UserService(AppDbContext dbContext) {
        _dbContext = dbContext;
    }

    public List<User> FindUsersByNameQuery(string name) {
        return _dbContext.Users
            .Where(u => u.UserName.ToLower().Contains(name.ToLower())).ToList();
    }

    public User FindUserByName(string name) {
        return _dbContext.Users.FirstOrDefault(u => u.UserName.ToLower() == name.ToLower())?? 
               throw new Exception("User not found: " + name);;
    }
    
}