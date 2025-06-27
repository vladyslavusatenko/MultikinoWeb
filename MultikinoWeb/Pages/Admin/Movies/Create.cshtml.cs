using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Filters;
using MultikinoWeb.Models;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Admin.Movies
{
    [AdminAuthorization]
    public class AdminMoviesCreateModel : PageModel
    {
        private readonly IMovieService _movieService;

        public AdminMoviesCreateModel(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [BindProperty]
        public CreateMovieViewModel MovieData { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? posterFile)
        {
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

            var movie = new Movie
            {
                Title = MovieData.Title,
                Description = MovieData.Description,
                Duration = MovieData.Duration,
                Genre = MovieData.Genre,
                Director = MovieData.Director,
                ReleaseDate = MovieData.ReleaseDate,
                Rating = MovieData.Rating,
                IsActive = true
            };

            if (posterFile != null)
            {
                using var memoryStream = new MemoryStream();
                await posterFile.CopyToAsync(memoryStream);
                movie.Poster = memoryStream.ToArray();
            }

            var result = await _movieService.AddMovieAsync(movie);

            if (result)
            {
                TempData["SuccessMessage"] = $"Film '{movie.Title}' został dodany pomyślnie!";
                return RedirectToPage("/Admin/Movies/Index");
            }

            ModelState.AddModelError("", "Wystąpił błąd podczas dodawania filmu.");
            return Page();
        }
    }
}