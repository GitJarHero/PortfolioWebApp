using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;
using PortfolioWebApp.Services;

namespace PortfolioWebApp.Services
{
    public interface IUserLoginService
    {
        Task<User?> ValidateCredentialsAsync(string username, string password);
        Task<bool> LoginAsync(User? user);
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

        public async Task<User?> ValidateCredentialsAsync(string username, string password)
        {
            var user = await _dbContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());

            if (user == null || user.State != UserState.Active) {
                return null;
            }

            if (!_passwordHashingService.Verify(password, user.Password)) {
                return null;
            }

            return user;
        }

        public async Task<bool> LoginAsync(User? user)
        {
            // Konvertiere User zu ClaimsPrincipal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("State", user.State.ToString()),
                new Claim("LastOnline", user.LastOnline?.ToString("o") ?? string.Empty),
                new Claim("Created", user.Created.ToString("o"))
            };

            // Füge Rollen als Claims hinzu
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "PortfolioAuth");
            var principal = new ClaimsPrincipal(identity);
            
            await _authenticationStateProvider.SignInAsync(principal);
            return true;
        }
    }
}