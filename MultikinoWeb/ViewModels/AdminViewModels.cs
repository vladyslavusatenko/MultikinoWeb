using System.ComponentModel.DataAnnotations;
using MultikinoWeb.Models;

namespace MultikinoWeb.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalMovies { get; set; }
        public int TotalHalls { get; set; }
        public int TotalUsers { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; }
        public int TodayBookings { get; set; }
        public int PendingBookings { get; set; }
        public int TodayScreenings { get; set; }
        public int UpcomingScreenings { get; set; }

        public IEnumerable<Booking> RecentBookings { get; set; } = new List<Booking>();
        public IEnumerable<PopularMovieViewModel> PopularMovies { get; set; } = new List<PopularMovieViewModel>();

        public decimal RevenueGrowth => LastMonthRevenue > 0 ? ((ThisMonthRevenue - LastMonthRevenue) / LastMonthRevenue) * 100 : 0;
    }

    public class PopularMovieViewModel
    {
        public Movie Movie { get; set; } = null!;
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CreateHallViewModel
    {
        [Required(ErrorMessage = "Nazwa sali jest wymagana")]
        [StringLength(100, ErrorMessage = "Nazwa sali nie może być dłuższa niż 100 znaków")]
        public string HallName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pojemność jest wymagana")]
        [Range(1, 1000, ErrorMessage = "Pojemność musi być między 1 a 1000 miejsc")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Typ sali jest wymagany")]
        public string HallType { get; set; } = "Standard";

        public List<string> HallTypes { get; set; } = new List<string> { "Standard", "VIP", "IMAX", "4DX", "Dolby Atmos" };
    }

    public class CreateScreeningViewModel
    {
        [Required(ErrorMessage = "Film jest wymagany")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Sala jest wymagana")]
        public int HallId { get; set; }

        [Required(ErrorMessage = "Data i godzina rozpoczęcia jest wymagana")]
        public DateTime StartTime { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Cena biletu jest wymagana")]
        [Range(0.01, 1000, ErrorMessage = "Cena biletu musi być większa niż 0")]
        public decimal TicketPrice { get; set; } = 25.00m;

        // For dropdown lists
        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();
        public IEnumerable<CinemaHall> Halls { get; set; } = new List<CinemaHall>();
    }

    public class RevenueReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalTickets { get; set; }
        public decimal AverageTicketPrice { get; set; }

        public IEnumerable<MovieRevenueViewModel> RevenueByMovie { get; set; } = new List<MovieRevenueViewModel>();
        public IEnumerable<HallRevenueViewModel> RevenueByHall { get; set; } = new List<HallRevenueViewModel>();
        public IEnumerable<DailyRevenueViewModel> DailyRevenue { get; set; } = new List<DailyRevenueViewModel>();
    }

    public class MovieRevenueViewModel
    {
        public Movie Movie { get; set; } = null!;
        public decimal Revenue { get; set; }
        public int Bookings { get; set; }
        public int Tickets { get; set; }
    }

    public class HallRevenueViewModel
    {
        public CinemaHall Hall { get; set; } = null!;
        public decimal Revenue { get; set; }
        public int Bookings { get; set; }
        public int Tickets { get; set; }
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Bookings { get; set; }
    }

    public class MoviePopularityViewModel
    {
        public Movie Movie { get; set; } = null!;
        public int TotalBookings { get; set; }
        public int TotalTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class HallUtilizationViewModel
    {
        public CinemaHall Hall { get; set; } = null!;
        public int TotalScreenings { get; set; }
        public int TotalCapacity { get; set; }
        public int BookedSeats { get; set; }
        public decimal UtilizationPercentage { get; set; }
    }

    public class SystemSettingsViewModel
    {
        [Required(ErrorMessage = "Nazwa kina jest wymagana")]
        [StringLength(200, ErrorMessage = "Nazwa kina nie może być dłuższa niż 200 znaków")]
        public string CinemaName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Domyślna cena biletu jest wymagana")]
        [Range(0.01, 1000, ErrorMessage = "Cena musi być większa niż 0")]
        public decimal DefaultTicketPrice { get; set; }

        [Required(ErrorMessage = "Limit czasu rezerwacji jest wymagany")]
        [Range(5, 120, ErrorMessage = "Limit czasu musi być między 5 a 120 minutami")]
        public int BookingTimeLimit { get; set; }

        [Required(ErrorMessage = "Limit czasu anulowania jest wymagany")]
        [Range(1, 24, ErrorMessage = "Limit czasu musi być między 1 a 24 godzinami")]
        public int CancellationTimeLimit { get; set; }

        [Required(ErrorMessage = "Maksymalna liczba biletów na rezerwację jest wymagana")]
        [Range(1, 20, ErrorMessage = "Maksymalna liczba biletów musi być między 1 a 20")]
        public int MaxTicketsPerBooking { get; set; }

        public bool EnableNotifications { get; set; }
        public bool MaintenanceMode { get; set; }
    }
}