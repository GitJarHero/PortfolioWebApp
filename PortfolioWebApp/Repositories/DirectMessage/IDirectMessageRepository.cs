using PortfolioWebApp.Models.Entities;

namespace PortfolioWebApp.Repositories;

public interface IDirectMessageRepository {

    Task<List<DirectMessage>> GetAllByUserAsync(int userId);
    
}