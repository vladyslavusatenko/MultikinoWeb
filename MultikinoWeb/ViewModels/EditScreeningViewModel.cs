using System.ComponentModel.DataAnnotations;
using MultikinoWeb.Models;

namespace MultikinoWeb.ViewModels
{
    public class EditScreeningViewModel
    {
        [Required(ErrorMessage = "Wybierz film")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Wybierz salę")]
        public int HallId { get; set; }

        [Required(ErrorMessage = "Podaj datę i godzinę rozpoczęcia")]
        [Display(Name = "Data i godzina rozpoczęcia")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Podaj cenę biletu")]
        [Range(0.01, 1000, ErrorMessage = "Cena musi być większa od 0")]
        [Display(Name = "Cena biletu")]
        public decimal TicketPrice { get; set; }

        public List<Movie> Movies { get; set; } = new();
        public List<CinemaHall> Halls { get; set; } = new();
    }
}