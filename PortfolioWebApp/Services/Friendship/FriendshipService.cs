using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services {
    
    public class FriendshipService {
        
        private readonly AppDbContext _dbContext;

        public FriendshipService(AppDbContext dbContext) {
            _dbContext = dbContext;
        }
        
        public List<UserDto> GetFriendsOfUser(string username) {
            var friendships = _dbContext.Friendships
                .Include(f => f.User1)
                .Include(f => f.User2)
                .Where(f => f.User1.UserName.ToLower() == username.ToLower() 
                            || f.User2.UserName.ToLower() == username.ToLower())
                .ToList();

            return friendships.Select(f => f.User1.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase) 
                ? new UserDto(f.User2.UserName, f.User2.Id) : new UserDto(f.User1.UserName, f.User1.Id)).ToList();
        }
        
        public void Save(Friendship friendship)
        {
            // Attach existing users to context to avoid insert
            _dbContext.Users.Attach(friendship.User1);
            _dbContext.Users.Attach(friendship.User2);

            _dbContext.Friendships.Add(friendship);
            var entriesWritten = _dbContext.SaveChanges();
            if (entriesWritten == 0)
            {
                throw new Exception("No entries were written to the database");
            }
        }

        
        public void Delete(Friendship friendship) {
            var existing = _dbContext.Friendships
                .FirstOrDefault(f =>
                    (f.User1.Id == friendship.User1.Id && f.User2.Id == friendship.User2.Id) ||
                    (f.User1.Id == friendship.User2.Id && f.User2.Id == friendship.User1.Id));

            if (existing == null) return;
            _dbContext.Friendships.Remove(existing);
            _dbContext.SaveChanges();
        }
    }
}
