using System.ComponentModel.DataAnnotations;

namespace MultikinoWeb.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Imię jest wymagane")]
        [StringLength(100, ErrorMessage = "Imię nie może być dłuższe niż 100 znaków")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [StringLength(100, ErrorMessage = "Nazwisko nie może być dłuższe niż 100 znaków")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format email")]
        public string Email { get; set; } = string.Empty;
    }
}