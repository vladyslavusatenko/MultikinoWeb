using MultikinoWeb.Models;

namespace MultikinoWeb.Services
{
    public interface IMovieService
    {
        Task<IEnumerable<Movie>> GetAllMoviesAsync();
        Task<IEnumerable<Movie>> GetActiveMoviesAsync();
        Task<Movie?> GetMovieByIdAsync(int id);
        Task<bool> AddMovieAsync(Movie movie);
        Task<bool> UpdateMovieAsync(Movie movie);
        Task<bool> DeleteMovieAsync(int id);
        Task<IEnumerable<Movie>> SearchMoviesAsync(string searchTerm);
    }
}