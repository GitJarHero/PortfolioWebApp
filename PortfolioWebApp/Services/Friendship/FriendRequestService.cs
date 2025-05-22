using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services;

public class FriendRequestService {

    private readonly AppDbContext _appDbContext;
    private readonly ILogger<FriendRequestService> _logger;
    
    public FriendRequestService(ILogger<FriendRequestService> logger, AppDbContext appDbContext) {
        _appDbContext = appDbContext;
        _logger = logger;
    }

    public List<FriendShipRequestDto> GetAllFriendRequests(string username) {
        
        var requests = _appDbContext.FriendRequests
            .Where(fr => fr.From.UserName.ToLower() == username.ToLower() 
                         || fr.To.UserName.ToLower() == username.ToLower())
            .Include(req => req.From)
            .Include(req => req.To)
            .ToList();

        return requests.Select(req => new FriendShipRequestDto(
            req.From.UserName,
            req.To.UserName,
            req.Created
        )).ToList();
    }

    public List<FriendShipRequestDto> FilterIncomingRequests(List<FriendShipRequestDto> allRequests, string userName)
    {
        return allRequests
            .Where(req => req.to.ToLower() == userName.ToLower())
            .ToList();
    }

    public List<FriendShipRequestDto> FilterOutgoingRequests(List<FriendShipRequestDto> allRequests, string userName)
    {
        return allRequests
            .Where(req => req.from.ToLower() == userName.ToLower())
            .ToList();
    }
    

    public void Save(FriendShipRequestDto dto) {
        var from = _appDbContext.Users
            .FirstOrDefault(usr => usr.UserName.ToLower() == dto.from.ToLower());
        var to = _appDbContext.Users
            .FirstOrDefault(usr => usr.UserName.ToLower() == dto.to.ToLower());

        if (from == null) {
            throw new Exception("User not found: " + dto.from);
        }
        if (to == null) {
            throw new Exception("User not found: " + dto.to);
        }
        
        var fr = new FriendRequest {
            Created = dto.ceated,
            From = from,
            To = to
        };

        _appDbContext.Add(fr);
        _appDbContext.SaveChanges();
    }
    
    public void Delete(FriendShipRequestDto friendRequest) {
        var requestToDelete = _appDbContext.FriendRequests
            .Include(req => req.From)
            .Include(req => req.To)
            .FirstOrDefault(req => 
                req.From.UserName.ToLower() == friendRequest.from.ToLower() && 
                req.To.UserName.ToLower() == friendRequest.to.ToLower() &&
                req.Created == friendRequest.ceated);
        
        if (requestToDelete == null) {
            _logger.LogWarning("Cannot delete: Friendrequest from {} to {} at {} not found",
                friendRequest.from, friendRequest.to, friendRequest.ceated);
            return;
        }
        
        _appDbContext.FriendRequests.Remove(requestToDelete);
        _appDbContext.SaveChanges();
    }

}