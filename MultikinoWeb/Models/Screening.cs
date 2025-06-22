using System.ComponentModel.DataAnnotations;

namespace MultikinoWeb.Models
{
    public class Screening
    {
        [Key]
        public int ScreeningId { get; set; }
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TicketPrice { get; set; }
        public int AvailableSeats { get; set; }

        public virtual Movie Movie { get; set; } = null!;
        public virtual CinemaHall Hall { get; set; } = null!;
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}