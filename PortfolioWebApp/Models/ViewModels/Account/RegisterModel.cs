using System.ComponentModel.DataAnnotations;

public class RegisterModel
{
    [Required(ErrorMessage = "Benutzername ist erforderlich")]
    public string Username { get; set; }
    

    [Required(ErrorMessage = "Passwort ist erforderlich")]
    [MinLength(6, ErrorMessage = "Passwort muss mindestens 6 Zeichen lang sein")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Passwortbestätigung ist erforderlich")]
    [Compare(nameof(Password), ErrorMessage = "Passwörter stimmen nicht überein")]
    public string ConfirmPassword { get; set; }
    
}