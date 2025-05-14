using System.Collections.Concurrent;

namespace PortfolioWebApp.Hubs.Connection;

// All ConnectionStorages use this implementation to store their user's connections 
public class UserConnectionStorage : INotificationConnectionStorage, IDirectChatConnectionStorage{
        
    private readonly ConcurrentDictionary<string, HashSet<string>> _connections = new();
    private readonly object _lock = new();

    public void AddConnection(string userId, string connectionId) {
        lock (_lock) {
            if (!_connections.TryGetValue(userId, out var connections)) {
                connections = new HashSet<string>();
                _connections[userId] = connections;
            }

            connections.Add(connectionId);
        }
    }

    public bool RemoveConnection(string? userId, string connectionId) {
        if (userId == null)
            return false;

        lock (_lock) {
            if (_connections.TryGetValue(userId, out var connections)) {
                bool removed = connections.Remove(connectionId);

                if (connections.Count == 0) {
                    _connections.TryRemove(userId, out _);
                }
                return removed;
            }
        }

        return false;
    }

    public HashSet<string> GetConnections(string userId) {
        lock (_lock) {
            if (_connections.TryGetValue(userId, out var connections)) {
                // Return a copy to avoid external modification
                return new HashSet<string>(connections);
            }

            return new HashSet<string>();
        }
    }

    public string? GetUser(string connectionId) {
        lock (_lock) {
            foreach (var pair in _connections) {
                if (pair.Value.Contains(connectionId)) {
                    return pair.Key;
                }
            }
        }
        return null;
    }
}