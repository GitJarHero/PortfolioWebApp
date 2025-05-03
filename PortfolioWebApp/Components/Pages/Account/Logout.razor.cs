using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;

namespace PortfolioWebApp.Components.Pages.Account
{
    public partial class Logout : ComponentBase
    {
        [CascadingParameter]
        public HttpContext? HttpContext { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync();
                NavigationManager.NavigateTo("/Account/Login", forceLoad: true);    
            }
        }
        
    }
}