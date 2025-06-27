using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // DODAJ tę linię
using MultikinoWeb.Data; // DODAJ tę linię
using MultikinoWeb.Filters;
using MultikinoWeb.Models;
using MultikinoWeb.Services;

[AdminAuthorization]
public class AdminMoviesIndexModel : PageModel
{
    private readonly IMovieService _movieService;
    private readonly MultikinoDbContext _context; // DODAJ tę linię

    public AdminMoviesIndexModel(IMovieService movieService, MultikinoDbContext context) // DODAJ context
    {
        _movieService = movieService;
        _context = context; // DODAJ tę linię
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
        var movie = await _movieService.GetMovieByIdAsync(id);
        if (movie == null)
        {
            TempData["ErrorMessage"] = "Film nie został znaleziony.";
            return RedirectToPage();
        }

        var hasScreenings = await _context.Screenings
            .AnyAsync(s => s.MovieId == id);

        if (hasScreenings)
        {
            TempData["ErrorMessage"] = $"Nie można usunąć filmu '{movie.Title}' - ma przypisane seanse. Usuń najpierw wszystkie seanse tego filmu.";
            return RedirectToPage();
        }

        var hasBookings = await _context.Bookings
            .AnyAsync(b => b.Screening.MovieId == id);

        if (hasBookings)
        {
            TempData["ErrorMessage"] = $"Nie można usunąć filmu '{movie.Title}' - istnieją rezerwacje na seanse tego filmu.";
            return RedirectToPage();
        }

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Film '{movie.Title}' został całkowicie usunięty z systemu.";
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