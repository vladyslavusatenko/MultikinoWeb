using System.ComponentModel.DataAnnotations;

namespace MultikinoWeb.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int ScreeningId { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.Now;
        public int NumberOfTickets { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Confirmed";
        public string PaymentMethod { get; set; } = "Card";

        public virtual User User { get; set; } = null!;
        public virtual Screening Screening { get; set; } = null!;
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}