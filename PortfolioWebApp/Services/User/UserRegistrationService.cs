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
                State = State.Active,
                ProfileColor = GenerateRandomHexColor()
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
        
        private string GenerateRandomHexColor()
        {
            Random rand = new Random();
            while (true)
            {
                int r = rand.Next(0, 256);
                int g = rand.Next(0, 256);
                int b = rand.Next(0, 256);
                
                double luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;

                // colors should look okay in both dark and white mode
                if (luminance > 64 && luminance < 192)
                {
                    return $"#{r:X2}{g:X2}{b:X2}";
                }
            }
        }

        
    }
}