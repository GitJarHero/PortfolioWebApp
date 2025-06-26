using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;

namespace PortfolioWebApp.Services
{
    public interface IUserRegistrationService 
    {
        Task<bool> RegisterAsync(string username, string password);
    }

    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IPasswordHashingService _passwordHashingService;

        
        public UserRegistrationService(
            AppDbContext dbContext, 
            IPasswordHashingService passwordHashingService)
        {
            _dbContext = dbContext;
            _passwordHashingService = passwordHashingService;
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            if (await _dbContext.Users.AnyAsync(u => u.UserName == username))
            {
                return false;
            }

            var newUser = new User
            {
                UserName = username,
                Password = _passwordHashingService.Hash(password),
                State = State.Active
            };
            // Get the USER role
            var userRole = await _dbContext.UserRoles.FirstOrDefaultAsync(r => r.Name == "user");
            if (userRole == null) {
                throw new InvalidOperationException("USER role not found in database");
            }

            newUser.Roles = new List<UserRole> { userRole };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        
    }
}