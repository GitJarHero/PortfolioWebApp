using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace PortfolioWebApp.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider {
    
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public CustomAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor) {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public override Task<AuthenticationState> GetAuthenticationStateAsync() {
        var user = _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(user));
    }

    public async Task SignInAsync(ClaimsPrincipal principal, AuthenticationProperties properties) {
        await _httpContextAccessor.HttpContext.SignInAsync("auth_cookie",principal, properties);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync() {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext != null) {
            await httpContext.SignOutAsync("auth_cookie");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        else {
            // Fallback, if no HttpContext is available
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
        }
    }
    
}