using System.ComponentModel.DataAnnotations;

namespace PortfolioWebApp.Models.ViewModels.Account;

public class LoginModel
{
    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; }
    
    public bool RememberMe { get; set; }
}