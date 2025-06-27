using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Services;

using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

// Konfiguracja bazy danych
builder.Services.AddDbContext<MultikinoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Rejestracja serwisów
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
// Dodaj po linii z IAdminService
builder.Services.AddScoped<IScreeningCleanupService, ScreeningCleanupService>();
// Dodaj także HostedService dla automatycznego wywołania
builder.Services.AddHostedService<ScreeningCleanupBackgroundService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

// Dodanie Razor Pages
builder.Services.AddRazorPages();

// Konfiguracja sesji
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

var supportedCultures = new[] { new CultureInfo("en-US") }; // 👈 Change to desired culture

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US"); // 👈 Force culture
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Clear(); // 👈 Disables auto-detection (system/browser)
});


var app = builder.Build();

// Inicjalizacja bazy danych
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MultikinoDbContext>();
    await context.Database.EnsureCreatedAsync();
    await SeedData(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

// Metoda inicjalizacji danych
static async Task SeedData(MultikinoDbContext context)
{
    if (context.Users.Any()) return;

    // Dodaj administratora
    var admin = new MultikinoWeb.Models.User
    {
        FirstName = "Admin",
        LastName = "System",
        Email = "admin@multikino.pl",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
        Role = "Admin",
        CreatedAt = DateTime.Now,
        IsActive = true
    };
    context.Users.Add(admin);

    // Dodaj sale kinowe
    var halls = new[]
    {
        new MultikinoWeb.Models.CinemaHall { HallName = "Sala 1", Capacity = 120, HallType = "Standard", IsActive = true },
        new MultikinoWeb.Models.CinemaHall { HallName = "Sala 2", Capacity = 80, HallType = "VIP", IsActive = true },
        new MultikinoWeb.Models.CinemaHall { HallName = "Sala 3", Capacity = 200, HallType = "IMAX", IsActive = true }
    };
    context.CinemaHalls.AddRange(halls);

    // Dodaj przykładowe filmy
    var movies = new[]
    {
        new MultikinoWeb.Models.Movie
        {
            Title = "Avatar: Droga wody",
            Description = "Akcja filmu \"Avatar: Istota wody\" rozgrywa się ponad dziesięć lat po wydarzeniach z pierwszej części. To opowieść o rodzinie Jake'a i Neytiri oraz ich staraniach, by zapewnić bezpieczeństwo sobie i swoim dzieciom, mimo tragedii, których wspólnie doświadczają i bitew, które muszą stoczyć, aby przeżyć.",
            Duration = 192,
            Genre = "Sci-Fi",
            Director = "James Cameron",
            ReleaseDate = DateTime.Now.AddDays(-30),
            Rating = 8.5m,
            IsActive = true
        },
        new MultikinoWeb.Models.Movie
        {
            Title = "Top Gun: Maverick",
            Description = "Nieustraszony buntownik komandor Pete \"Maverick\" (Tom Cruise), lubiący łamać protokoły i przeciwstawiać się rozkazom, szefuje programowi hipersonicznych samolotów zwiadowczych. Planowane jest jednak jego zamknięcie i zastąpienie pilotów dronami, chyba że podczas testu osiągnie prędkość 10-krotnie większą od dźwięku.",
            Duration = 130,
            Genre = "Akcja",
            Director = "Joseph Kosinski",
            ReleaseDate = DateTime.Now.AddDays(-60),
            Rating = 8.7m,
            IsActive = true
        }
    };
    context.Movies.AddRange(movies);

    await context.SaveChangesAsync();

    // Dodaj seanse
    var screenings = new[]
    {
        new MultikinoWeb.Models.Screening
        {
            MovieId = 1,
            HallId = 1,
            StartTime = DateTime.Now.AddDays(7).AddHours(2),
            EndTime = DateTime.Now.AddDays(7).AddHours(5),
            TicketPrice = 25.00m,
            AvailableSeats = 120
        },
        new MultikinoWeb.Models.Screening
        {
            MovieId = 1,
            HallId = 2,
            StartTime = DateTime.Now.AddDays(5).AddHours(6),
            EndTime = DateTime.Now.AddDays(5).AddHours(9),
            TicketPrice = 35.00m,
            AvailableSeats = 80
        },
        new MultikinoWeb.Models.Screening
        {
            MovieId = 2,
            HallId = 3,
            StartTime = DateTime.Now.AddDays(6).AddHours(3),
            EndTime = DateTime.Now.AddDays(6).AddHours(5),
            TicketPrice = 30.00m,
            AvailableSeats = 200
        }
    };
    context.Screenings.AddRange(screenings);

    await context.SaveChangesAsync();
}