using System.ComponentModel.DataAnnotations;

namespace MultikinoWeb.ViewModels
{
    public class CreateMovieViewModel
    {
        [Required(ErrorMessage = "Tytuł jest wymagany")]
        [StringLength(200, ErrorMessage = "Tytuł nie może być dłuższy niż 200 znaków")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Opis jest wymagany")]
        [StringLength(1000, ErrorMessage = "Opis nie może być dłuższy niż 1000 znaków")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Czas trwania jest wymagany")]
        [Range(1, 500, ErrorMessage = "Czas trwania musi być między 1 a 500 minutami")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Gatunek jest wymagany")]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reżyser jest wymagany")]
        [StringLength(200, ErrorMessage = "Nazwa reżysera nie może być dłuższa niż 200 znaków")]
        public string Director { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data premiery jest wymagana")]
        public DateTime ReleaseDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Ocena jest wymagana")]
        [Range(1.0, 10.0, ErrorMessage = "Ocena musi być między 1.0 a 10.0")]
        public decimal Rating { get; set; }
    }
}