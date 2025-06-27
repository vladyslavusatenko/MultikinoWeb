using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Models;
using MultikinoWeb.Services;

namespace MultikinoWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMovieService _movieService;
        private readonly MultikinoDbContext _context;

        public IndexModel(IMovieService movieService, MultikinoDbContext context)
        {
            _movieService = movieService;
            _context = context;
        }

        public IEnumerable<Movie> CurrentMovies { get; set; } = new List<Movie>();
        public IEnumerable<Screening> TodayScreenings { get; set; } = new List<Screening>();
        public IEnumerable<CinemaHall> CinemaHalls { get; set; } = new List<CinemaHall>();

        // Statystyki dla strony głównej
        public int TotalMovies { get; set; }
        public int TotalHalls { get; set; }
        public int TotalUsers { get; set; }
        public decimal AverageRating { get; set; }

        public async Task OnGetAsync()
        {
            // Pobierz aktywne filmy
            CurrentMovies = await _movieService.GetActiveMoviesAsync();

            // Pobierz dzisiejsze seanse
            var today = DateTime.Today;
            TodayScreenings = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Where(s => s.StartTime.Date == today && s.StartTime > DateTime.Now)
                .OrderBy(s => s.StartTime)
                .Take(5)
                .ToListAsync();

            // Pobierz sale kinowe
            CinemaHalls = await _context.CinemaHalls
                .Where(h => h.IsActive)
                .OrderBy(h => h.HallName)
                .ToListAsync();

            // Oblicz statystyki
            await CalculateStatisticsAsync();
        }

        private async Task CalculateStatisticsAsync()
        {
            // Liczba aktywnych filmów
            TotalMovies = await _context.Movies
                .CountAsync(m => m.IsActive);

            // Liczba aktywnych sal
            TotalHalls = await _context.CinemaHalls
                .CountAsync(h => h.IsActive);

            // Liczba aktywnych użytkowników
            TotalUsers = await _context.Users
                .CountAsync(u => u.IsActive);

            // Średnia ocena filmów - ZMIANA: round do całkowitej
            var ratings = await _context.Movies
                .Where(m => m.IsActive)
                .Select(m => m.Rating)
                .ToListAsync();

            AverageRating = ratings.Any() ? Math.Round(ratings.Average()) : 0;
        }
    }
}