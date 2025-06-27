using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Models;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly MultikinoDbContext _context;

        public ProfileModel(IUserService userService, MultikinoDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [BindProperty]
        public EditProfileViewModel ProfileData { get; set; } = new();

        public User? User { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalTickets { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            User = await _userService.GetUserByIdAsync(userId.Value);
            if (User == null)
            {
                return NotFound();
            }

            ProfileData.FirstName = User.FirstName;
            ProfileData.LastName = User.LastName;
            ProfileData.Email = User.Email;

            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId && b.Status == "Confirmed")
                .ToListAsync();

            TotalBookings = bookings.Count;
            TotalSpent = bookings.Sum(b => b.TotalAmount);
            TotalTickets = bookings.Sum(b => b.NumberOfTickets);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            User = await _userService.GetUserByIdAsync(userId.Value);
            if (User == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            User.FirstName = ProfileData.FirstName;
            User.LastName = ProfileData.LastName;

            var result = await _userService.UpdateUserAsync(User);
            if (result)
            {
                HttpContext.Session.SetString("UserName", $"{User.FirstName} {User.LastName}");
                TempData["SuccessMessage"] = "Profil został zaktualizowany pomyślnie!";
                return RedirectToPage();
            }

            ModelState.AddModelError("", "Wystąpił błąd podczas aktualizacji profilu.");
            return Page();
        }
    }
}