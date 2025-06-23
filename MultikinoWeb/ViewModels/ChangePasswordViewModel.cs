using System.ComponentModel.DataAnnotations;

namespace MultikinoWeb.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Aktualne hasło jest wymagane")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nowe hasło jest wymagane")]
        [StringLength(100, ErrorMessage = "Hasło musi mieć minimum {2} znaków", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Hasła się nie zgadzają")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}