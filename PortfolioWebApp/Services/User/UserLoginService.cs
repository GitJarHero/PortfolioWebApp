using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;

namespace PortfolioWebApp.Services
{
    public interface IUserLoginService
    {
        Task<User?> ValidateCredentialsAsync(string username, string password);
        Task<bool> LoginAsync(User? user, bool rememberMe);
    }

    public class UserLoginService : IUserLoginService
    {
        private readonly AppDbContext _dbContext;
        private readonly IPasswordHashingService _passwordHashingService;
        private CustomAuthenticationStateProvider _authenticationStateProvider;
        
        public UserLoginService(
            AppDbContext dbContext, 
            IPasswordHashingService passwordHashingService,
            CustomAuthenticationStateProvider authenticationStateProvider)
        {
            _dbContext = dbContext;
            _passwordHashingService = passwordHashingService;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<User?> ValidateCredentialsAsync(string username, string password) {
            var normalized = username.Trim().ToLower();

            var user = await _dbContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == normalized);

            if (user == null || user.State != State.Active) {
                return null;
            }

            if (!_passwordHashingService.Verify(password, user.Password)) {
                return null;
            }

            return user;
        }


        public async Task<bool> LoginAsync(User? user, bool rememberMe) {
            
            if (user is null) {
                return false;
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            
            foreach (var role in user.Roles) {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var properties = new AuthenticationProperties {
                IsPersistent = rememberMe
            };

            var identity = new ClaimsIdentity(claims, "auth_cookie");
            var principal = new ClaimsPrincipal(identity);
            
            await _authenticationStateProvider.SignInAsync(principal, properties);
            return true;
        }
    }
}