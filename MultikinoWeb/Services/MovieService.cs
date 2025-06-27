using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Models;

namespace MultikinoWeb.Services
{
    public class MovieService : IMovieService
    {
        private readonly MultikinoDbContext _context;

        public MovieService(MultikinoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
        {
            return await _context.Movies
                .Include(m => m.Screenings) // DODAJ tę linię
                .OrderBy(m => m.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> GetActiveMoviesAsync()
        {
            return await _context.Movies
                .Where(m => m.IsActive)
                .OrderBy(m => m.Title)
                .ToListAsync();
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.Screenings)
                .ThenInclude(s => s.Hall)
                .FirstOrDefaultAsync(m => m.MovieId == id);
        }

        public async Task<bool> AddMovieAsync(Movie movie)
        {
            _context.Movies.Add(movie);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateMovieAsync(Movie movie)
        {
            _context.Movies.Update(movie);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                movie.IsActive = false;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<IEnumerable<Movie>> SearchMoviesAsync(string searchTerm)
        {
            return await _context.Movies
                .Where(m => m.IsActive &&
                       (m.Title.Contains(searchTerm) ||
                        m.Genre.Contains(searchTerm) ||
                        m.Director.Contains(searchTerm)))
                .OrderBy(m => m.Title)
                .ToListAsync();
        }
    }
}