// Movies/Details.cshtml.cs - POPRAWIONA WERSJA
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Models;
using MultikinoWeb.Services;

namespace MultikinoWeb.Pages.Movies
{
    public class MovieDetailsModel : PageModel
    {
        private readonly IMovieService _movieService;

        public MovieDetailsModel(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public Movie? Movie { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Movie = await _movieService.GetMovieByIdAsync(id);

            if (Movie == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}