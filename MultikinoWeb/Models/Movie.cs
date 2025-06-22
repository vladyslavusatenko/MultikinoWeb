using System.ComponentModel.DataAnnotations;

namespace MultikinoWeb.Models
{
    public class Movie
    {
        [Key]
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public decimal Rating { get; set; }
        public byte[]? Poster { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }
}