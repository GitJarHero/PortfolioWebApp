using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services;

using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

public class FriendRequestClientService {
    
    private readonly NavigationManager _navigation;
    private readonly IJSRuntime _jsRuntime;
    private HubConnection? _hubConnection;
    private IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public event Action? OnFriendRequestReceived;
    public int PendingFriendRequests { get; private set; }

    public FriendRequestClientService(NavigationManager navigation, 
                                        IJSRuntime jsRuntime, 
                                        IConfiguration config,
                                        IHttpContextAccessor httpContextAccessor) {
        _navigation = navigation;
        _jsRuntime = jsRuntime;
        _config = config;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task StartAsync() {
        var url = _config["SignalR:Url:FriendRequestHub"];
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri(url), options => {
                // add the auth_cookie defined in program.cs so the hub can identify the user.
                options.Cookies.Add( 
                    new Uri(_navigation.BaseUri), 
                    new System.Net.Cookie("auth_cookie", _httpContextAccessor.HttpContext!.Request.Cookies
                        .FirstOrDefault(c => c.Key == "auth_cookie").Value)
                );
            })
            .Build();

        _hubConnection.On("ReceiveFriendRequest", async (FriendShipRequestDto request) => {
            var currentUrl = _navigation.Uri;

            if (currentUrl.Contains("/Chats", StringComparison.OrdinalIgnoreCase)) {
                await _jsRuntime.InvokeVoidAsync("alert", "Neue Nachricht im Chat!");
            }
            else {
                PendingFriendRequests++;
                OnFriendRequestReceived?.Invoke();
            }
        });

        await _hubConnection.StartAsync();
    }


    public async Task SendRequestTo(string targetUser) {
        var me = _httpContextAccessor.HttpContext.User.Identity.Name;
        var requestDto = new FriendShipRequestDto(targetUser, me, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified));
        
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected) {
            await _hubConnection.SendAsync("SendFriendRequest", requestDto);   
        }
    }
    
}
