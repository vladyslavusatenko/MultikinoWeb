using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Models;

namespace MultikinoWeb.Pages.Movies
{
    public class DetailsModel : PageModel
    {
        private readonly MultikinoDbContext _context;

        public DetailsModel(MultikinoDbContext context)
        {
            _context = context;
        }

        public Movie? Movie { get; set; }
        public IEnumerable<Screening> AvailableScreenings { get; set; } = new List<Screening>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == id && m.IsActive);

            if (Movie == null)
            {
                return NotFound();
            }

            AvailableScreenings = await _context.Screenings
                .Include(s => s.Hall)
                .Where(s => s.MovieId == id && s.StartTime > DateTime.Now)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return Page();
        }
    }
}