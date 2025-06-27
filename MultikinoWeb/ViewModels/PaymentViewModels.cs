using System.ComponentModel.DataAnnotations;

namespace MultikinoWeb.ViewModels
{
    public class PaymentDetailsViewModel
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int BookingId { get; set; }

        public CardPaymentViewModel? CardPayment { get; set; }

        public BlikPaymentViewModel? BlikPayment { get; set; }

        public TransferPaymentViewModel? TransferPayment { get; set; }
    }

    public class CardPaymentViewModel
    {
        [Required(ErrorMessage = "Numer karty jest wymagany")]
        [CreditCard(ErrorMessage = "Nieprawidłowy numer karty")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Imię i nazwisko są wymagane")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data ważności jest wymagana")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Format: MM/YY")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV jest wymagany")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV musi mieć 3-4 cyfry")]
        public string CVV { get; set; } = string.Empty;
    }

    public class BlikPaymentViewModel
    {
        [Required(ErrorMessage = "Kod BLIK jest wymagany")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Kod BLIK musi mieć 6 cyfr")]
        public string BlikCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer telefonu jest wymagany")]
        [Phone(ErrorMessage = "Nieprawidłowy numer telefonu")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class TransferPaymentViewModel
    {
        [Required(ErrorMessage = "Bank jest wymagany")]
        public string SelectedBank { get; set; } = string.Empty;

        public List<string> AvailableBanks { get; set; } = new List<string>
        {
            "PKO Bank Polski",
            "Bank Pekao",
            "mBank",
            "ING Bank Śląski",
            "Santander Bank",
            "Bank Millennium",
            "Alior Bank",
            "Getin Noble Bank"
        };
    }
}