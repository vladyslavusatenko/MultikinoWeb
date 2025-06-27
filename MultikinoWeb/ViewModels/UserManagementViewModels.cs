using System.ComponentModel.DataAnnotations;
using MultikinoWeb.Models;

namespace MultikinoWeb.ViewModels
{
    public class UserStatisticsViewModel
    {
        public User User { get; set; } = null!;
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalTickets { get; set; }
        public DateTime? LastBookingDate { get; set; }
        public string FavoriteGenre { get; set; } = string.Empty;
        public List<Booking> RecentBookings { get; set; } = new();
    }

    public class AdminUsersOverviewViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int BannedUsers { get; set; }
        public int AdminUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisMonth { get; set; }
        public List<TopSpenderViewModel> TopSpenders { get; set; } = new();
    }

    public class TopSpenderViewModel
    {
        public User User { get; set; } = null!;
        public decimal TotalSpent { get; set; }
    }

    public class BanUserViewModel
    {
        [Required(ErrorMessage = "Powód jest wymagany")]
        [StringLength(500, ErrorMessage = "Powód nie może być dłuższy niż 500 znaków")]
        public string Reason { get; set; } = string.Empty;

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}