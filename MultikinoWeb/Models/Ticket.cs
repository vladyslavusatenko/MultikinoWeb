using System.ComponentModel.DataAnnotations;

namespace MultikinoWeb.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }
        public int BookingId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsUsed { get; set; } = false;

        public virtual Booking Booking { get; set; } = null!;
    }
}