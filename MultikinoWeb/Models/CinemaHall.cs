using System.ComponentModel.DataAnnotations;

namespace MultikinoWeb.Models
{
    public class CinemaHall
    {
        [Key]
        public int HallId { get; set; }
        public string HallName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string HallType { get; set; } = "Standard";
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }
}