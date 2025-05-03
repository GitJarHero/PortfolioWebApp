using Microsoft.AspNetCore.Components;
using PortfolioWebApp.Services;

namespace PortfolioWebApp.Components.Pages.Account;


public partial class Register
{
    [SupplyParameterFromForm(FormName = "RegisterData")]
    private RegisterModel RegisterData { get; set; } = new RegisterModel();
    
    [Inject]
    public NavigationManager NavigationManager { get; set; }
    
    [Inject]
    private IUserRegistrationService UserRegistrationService { get; set; }
    
    [CascadingParameter]
    public HttpContext? HttpContext { get; set; }
    
    private bool ShowRegisterError { get; set; }
    private string RegisterErrorMessage { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        var identity = HttpContext.User.Identity;
        if (identity != null && identity.IsAuthenticated)
        {
            NavigationManager.NavigateTo("/Home");
        }
    }
    
    private async Task HandleRegister()
    {
        
        if (!await UserRegistrationService.RegisterAsync(RegisterData.Username, RegisterData.Password))
        {
            ShowRegisterError = true;
            RegisterErrorMessage = "Username is already taken.";
            return;
        }
        
        NavigationManager.NavigateTo("/Login", forceLoad: true);
    }
    
}