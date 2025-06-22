using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Models;
using MultikinoWeb.Services;

namespace MultikinoWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMovieService _movieService;

        public IndexModel(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public IEnumerable<Movie> CurrentMovies { get; set; } = new List<Movie>();

        public async Task OnGetAsync()
        {
            CurrentMovies = await _movieService.GetActiveMoviesAsync();
        }
    }
}