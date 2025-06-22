using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Models;
using MultikinoWeb.Services;

namespace MultikinoWeb.Pages.Movies
{
    public class MoviesIndexModel : PageModel
    {
        private readonly IMovieService _movieService;

        public MoviesIndexModel(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Movies = await _movieService.SearchMoviesAsync(SearchTerm);
            }
            else
            {
                Movies = await _movieService.GetActiveMoviesAsync();
            }
        }
    }
}