using HotelBookingApp.Data;
using HotelBookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelBookingApp.Pages
{
    [Authorize]
    public class MyReservationsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public MyReservationsModel(ApplicationDbContext db) => _db = db;

        public List<Reservation> Reservations { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Hent innlogget bruker‐ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Hent alle reserverasjoner for denne brukeren
            Reservations = await _db.Reservations
                                   .Include(r => r.Room)
                                   .Where(r => r.UserId == userId)
                                   .OrderByDescending(r => r.FromDate)
                                   .ToListAsync();

            // Oppdater status til Completed for alle som har passert ToDate
            bool anyChanged = false;
            foreach (var r in Reservations)
            {
                if (r.Status == ReservationStatus.Active && r.ToDate < DateTime.Today)
                {
                    r.Status = ReservationStatus.Completed;
                    anyChanged = true;
                }
            }

            // Lagre endringene om det var noen oppdaterte
            if (anyChanged)
            {
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var res = await _db.Reservations.FindAsync(id);

            if (res == null || res.UserId != userId)
                return NotFound();

            // Sett status til avbestilt
            res.Status = ReservationStatus.Cancelled;
            await _db.SaveChangesAsync();

            // Reload så du ser endringen umiddelbart
            return RedirectToPage();
        }
    }
}
