using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace PortfolioWebApp.Services;

public class TrackingCircuitHandler : CircuitHandler {
    
    public class OnlineUserInfo {
        public string CircuitId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
    
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    private readonly ILogger<TrackingCircuitHandler> _logger;

    public TrackingCircuitHandler(IHttpContextAccessor httpContextAccessor, ILogger<TrackingCircuitHandler> logger) {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken) {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated == true) {
            var userName = user.Identity.Name;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userId)) {
                var userInfo = new OnlineUserInfo {
                    CircuitId = circuit.Id,
                    UserId = userId,
                    Roles = roles
                };
                
                _logger.LogInformation("User {userName} (ID: {userId}) connected with Circuit {circuit.Id}", userName, userId, circuit.Id);
            }
        }

        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }


    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken) {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated == true) {
            var userName = user.Identity.Name;

            if (!string.IsNullOrEmpty(userName)) {
                _logger.LogInformation("User {userName} disconnected from Circuit {circuit.Id}", userName, circuit.Id);
            }
        }

        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }
    
}
