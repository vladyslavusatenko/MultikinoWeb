using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Filters;
using MultikinoWeb.Models;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Admin.Movies
{
    [AdminAuthorization]
    public class AdminMoviesEditModel : PageModel
    {
        private readonly IMovieService _movieService;

        public AdminMoviesEditModel(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [BindProperty]
        public EditMovieViewModel MovieData { get; set; } = new();

        public Movie? Movie { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Movie = await _movieService.GetMovieByIdAsync(id);

            if (Movie == null)
            {
                return NotFound();
            }

            MovieData = new EditMovieViewModel
            {
                MovieId = Movie.MovieId,
                Title = Movie.Title,
                Description = Movie.Description,
                Duration = Movie.Duration,
                Genre = Movie.Genre,
                Director = Movie.Director,
                ReleaseDate = Movie.ReleaseDate,
                Rating = Movie.Rating,
                IsActive = Movie.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, IFormFile? posterFile)
        {
            Movie = await _movieService.GetMovieByIdAsync(id);

            if (Movie == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (posterFile != null)
            {
                if (posterFile.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    ModelState.AddModelError("", "Plik postera jest za duży. Maksymalny rozmiar to 5MB.");
                    return Page();
                }

                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(posterFile.ContentType))
                {
                    ModelState.AddModelError("", "Nieprawidłowy format pliku. Obsługiwane formaty: JPG, PNG, GIF.");
                    return Page();
                }
            }

            Movie.Title = MovieData.Title;
            Movie.Description = MovieData.Description;
            Movie.Duration = MovieData.Duration;
            Movie.Genre = MovieData.Genre;
            Movie.Director = MovieData.Director;
            Movie.ReleaseDate = MovieData.ReleaseDate;
            Movie.Rating = MovieData.Rating;
            Movie.IsActive = MovieData.IsActive;

            if (posterFile != null)
            {
                using var memoryStream = new MemoryStream();
                await posterFile.CopyToAsync(memoryStream);
                Movie.Poster = memoryStream.ToArray();
            }

            var result = await _movieService.UpdateMovieAsync(Movie);

            if (result)
            {
                TempData["SuccessMessage"] = $"Film '{Movie.Title}' został zaktualizowany pomyślnie!";
                return RedirectToPage("/Admin/Movies/Index");
            }

            ModelState.AddModelError("", "Wystąpił błąd podczas aktualizacji filmu.");
            return Page();
        }
    }
}