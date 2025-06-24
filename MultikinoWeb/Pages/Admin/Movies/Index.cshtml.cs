using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Filters;
using MultikinoWeb.Models;
using MultikinoWeb.Services;

namespace MultikinoWeb.Pages.Admin.Movies
{
    [AdminAuthorization]
    public class AdminMoviesIndexModel : PageModel
    {
        private readonly IMovieService _movieService;

        public AdminMoviesIndexModel(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string GenreFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var allMovies = await _movieService.GetAllMoviesAsync();
            Movies = FilterMovies(allMovies);
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie != null)
            {
                movie.IsActive = !movie.IsActive;
                await _movieService.UpdateMovieAsync(movie);

                TempData["SuccessMessage"] = $"Status filmu '{movie.Title}' został zmieniony.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _movieService.DeleteMovieAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Film został usunięty pomyślnie.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie można usunąć filmu. Sprawdź czy nie ma przypisanych seansów.";
            }

            return RedirectToPage();
        }

        private IEnumerable<Movie> FilterMovies(IEnumerable<Movie> movies)
        {
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                movies = movies.Where(m =>
                    m.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    m.Director.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(GenreFilter))
            {
                movies = movies.Where(m => m.Genre == GenreFilter);
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                bool isActive = bool.Parse(StatusFilter);
                movies = movies.Where(m => m.IsActive == isActive);
            }

            return movies.OrderBy(m => m.Title);
        }
    }
}