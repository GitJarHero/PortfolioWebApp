using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using PortfolioWebApp.Models.ViewModels.Account;
using PortfolioWebApp.Services;


namespace PortfolioWebApp.Components.Pages.Account {
    
    public partial class Login : ComponentBase {
        
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        
        [Inject]
        private IUserLoginService UserLoginService { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationState { get; set; }
        
        [SupplyParameterFromForm(FormName = "loginForm")]
        private LoginModel LoginModel { get; set; } = new LoginModel();
        
        private string LoginErrorMessage = string.Empty;
        private string? ReturnUrl { get; set; }
        
        protected override async Task OnInitializedAsync() {
            if (AuthenticationState != null) {
                var authState = await AuthenticationState;
                if (authState.User.Identity?.IsAuthenticated == true) {
                    NavigationManager.NavigateTo("/Home");
                }
            }
            
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("returnUrl", out var returnUrl)) {
                ReturnUrl = returnUrl;
            }
        }
        
        private async Task OnValidSubmit() {
            var user = await UserLoginService.ValidateCredentialsAsync(LoginModel.Username, LoginModel.Password);
            if (user == null) {
                LoginErrorMessage = "Invalid Credentials.";
                return;
            }
            
            var success = await UserLoginService.LoginAsync(user, LoginModel.RememberMe);
            if (!success) {
                LoginErrorMessage = "Error: Could not login";
                return;
            }
            NavigationManager.NavigateTo(ReturnUrl ?? "/Home", forceLoad: true);
        }
        
    }
}