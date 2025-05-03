using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace PortfolioWebApp.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider {
    
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public CustomAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public override Task<AuthenticationState> GetAuthenticationStateAsync() {
        var user = _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(user));
    }

    public async Task SignInAsync(ClaimsPrincipal principal) {
        await _httpContextAccessor.HttpContext.SignInAsync(principal);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());   
    }
    
}