using HotelBookingApp.Data;
using HotelBookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelBookingApp.Pages
{
    [Authorize] //bruker må være logget inn for å booke
    public class BookRoomModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public BookRoomModel(ApplicationDbContext db) => _db = db;

        [BindProperty(SupportsGet = true)]
        public int RoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime To { get; set; }

        public Room? Room { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Room = await _db.Rooms.FindAsync(RoomId);
            if (Room == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Hent rommet på nytt, slik at Model.Room ikke er null
            Room = await _db.Rooms.FindAsync(RoomId);
            if (Room == null)
                return NotFound();


            // Sjekk dato‐rekkefølge
            if (From >= To)
            {
                ModelState.AddModelError("", "Til‐dato må være etter fra‐dato.");
                return Page();
            }

            // Sjekk kollisjon med andre reservasjoner på akkurat dette rommet
            bool collision = await _db.Reservations
                .AnyAsync(r =>
                    r.RoomId == RoomId
                 && r.FromDate < To
                 && From < r.ToDate);

            if (collision)
            {
                ModelState.AddModelError("", "Dette rommet er allerede reservert for de valgte datoene.");
                return Page();
            }

            // Opprett og lagre
            var reservation = new Reservation
            {
                RoomId = RoomId,
                FromDate = From,
                ToDate = To,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Status = ReservationStatus.Active
            };
            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            // Redirect tilbake til søkesiden, kunne kanskje lagd en bekreftelsesside
            return RedirectToPage("AvailableRooms", new { From, To });
        }
    }
}
