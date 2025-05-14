using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services {
    
    public class FriendshipService {
        
        private readonly AppDbContext _dbContext;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        
        public FriendshipService(AppDbContext dbContext, AuthenticationStateProvider authenticationStateProvider) {
            _dbContext = dbContext;
            _authenticationStateProvider = authenticationStateProvider;
        }
        
        public List<UserDto> GetFriends() {
            string username = _authenticationStateProvider.GetAuthenticationStateAsync().Result.User.Identity.Name;
            var friendships = _dbContext.Friendships
                .Include(f => f.User1)
                .Include(f => f.User2)
                .Where(f => f.User1.UserName.ToLower() == username.ToLower() 
                            || f.User2.UserName.ToLower() == username.ToLower())
                .ToList();

            return friendships.Select(f => f.User1.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase) 
                ? new UserDto(f.User2.UserName, f.User2.Id) : new UserDto(f.User1.UserName, f.User1.Id)).ToList();
        }
        
    }
}
