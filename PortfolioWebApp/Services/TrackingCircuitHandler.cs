using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace PortfolioWebApp.Services;

// not needed but kept for debugging or maybe later an "current online users" feature
public class TrackingCircuitHandler : CircuitHandler {
    
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

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userId)) {
                
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
