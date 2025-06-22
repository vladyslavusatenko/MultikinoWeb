using System.ComponentModel.DataAnnotations;

namespace MultikinoWeb.ViewModels
{
    public class BookingViewModel
    {
        public int ScreeningId { get; set; }

        [Range(1, 10, ErrorMessage = "Można wybrać od 1 do 10 miejsc")]
        public int NumberOfTickets { get; set; } = 1;

        [Required(ErrorMessage = "Metoda płatności jest wymagana")]
        public string PaymentMethod { get; set; } = "Card";

        [Required(ErrorMessage = "Musisz wybrać przynajmniej jedno miejsce")]
        public string SelectedSeats { get; set; } = string.Empty;

        // Dane tylko do wyświetlenia (nie walidowane)
        public decimal TicketPrice { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int AvailableSeats { get; set; }
    }
}