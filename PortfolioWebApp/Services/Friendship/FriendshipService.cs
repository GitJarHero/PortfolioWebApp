using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services {
    
    public class FriendshipService {
        
        private readonly AppDbContext _dbContext;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        
        public FriendshipService(AppDbContext dbContext, AuthenticationStateProvider authenticationStateProvider) {
            _dbContext = dbContext;
            _authenticationStateProvider = authenticationStateProvider;
        }
        
        public async Task<List<UserDto>> GetFriendsAsync() {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            string username = authState.User.Identity.Name;

            var friendships = await _dbContext.Friendships
                .Include(f => f.User1)
                .Include(f => f.User2)
                .Where(f => f.User1.UserName.ToLower() == username.ToLower()
                            || f.User2.UserName.ToLower() == username.ToLower())
                .ToListAsync();

            return friendships.Select(f => f.User1.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase)
                    ? new UserDto(f.User2.UserName, f.User2.Id, f.User2.ProfileColor)
                    : new UserDto(f.User1.UserName, f.User1.Id, f.User1.ProfileColor))
                .ToList();
        }

        
        public void Save(Friendship friendship) {
            _dbContext.Add(friendship);
            _dbContext.SaveChanges();
        }
        
        public void Delete(Friendship friendship) {
            _dbContext.Friendships.Remove(friendship);
            _dbContext.SaveChanges();
        }
        
    }
}
