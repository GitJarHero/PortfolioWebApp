namespace PortfolioWebApp.Hubs.Connection;

public interface IUserConnectionStorage {
    
    void AddConnection(string userId, string connectionId);
    
    bool RemoveConnection(string? userId, string connectionId);
    
    HashSet<string> GetConnections(string userId);
    
    string? GetUser(string connectionId);
    
}