using HotelBookingApp.Data;
using HotelBookingApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelBookingApp.Pages
{
    public class AvailableRoomsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AvailableRoomsModel(ApplicationDbContext context)
        {
            _context = context;
        }
        // Datointervall:
        [BindProperty] public DateTime FromDate { get; set; }
        [BindProperty] public DateTime ToDate { get; set; }
        // Filtre:
        [BindProperty] public string RoomType { get; set; }
        [BindProperty] public int? MinBeds { get; set; } //kan være null

        // Dropdown-verdier: 
        public List<string> AllRoomTypes { get; set; } = new();
        public List<int> AllBedCounts { get; set; } = new();

        // Resultat:
        public List<Room> AvailableRooms { get; set; } = new();

        //For booking: 
        [BindProperty]
        public int? BookingRoomId { get; set; }

        // Kalles når siden først lastes
        public async Task OnGetAsync()
        {
            FromDate = DateTime.Today;
            ToDate = DateTime.Today.AddDays(1);

            // Fyll dropdown
            AllRoomTypes = await _context.Rooms
                .Select(r => r.RoomType).Distinct().ToListAsync();
            AllBedCounts = await _context.Rooms
                .Select(r => r.NumberOfBeds).Distinct().OrderBy(x => x).ToListAsync();
        }

        // Kalles når brukeren søker etter ledig rom:
        public async Task<IActionResult> OnPostAsync()
        {
            if (FromDate >= ToDate)
            {
                ModelState.AddModelError("", "Til-dato må være etter fra-dato.");
                await OnGetAsync();    // fyll dropdown på nytt
                return Page();
            }

            // bygg opp spørringen
            var query = _context.Rooms
                .Include(r => r.Reservations)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(RoomType))
                query = query.Where(r => r.RoomType == RoomType);

            if (MinBeds.HasValue)
                query = query.Where(r => r.NumberOfBeds >= MinBeds.Value);

            // sjekk kollisjoner med eksisterende reservasjoner
            AvailableRooms = await query
                .Where(r => r.Reservations
                    .Where(res => res.Status == ReservationStatus.Active)
                    .All(res => ToDate <= res.FromDate || FromDate >= res.ToDate))
                .ToListAsync();

            // fyll dropdown‐lister på nytt (slik at de vises riktig etter post)
            AllRoomTypes = await _context.Rooms
                .Select(r => r.RoomType).Distinct().ToListAsync();
            AllBedCounts = await _context.Rooms
                .Select(r => r.NumberOfBeds).Distinct().OrderBy(x => x).ToListAsync();

            return Page();
        }

        //Kalles når brukeren bestiller rom:
        public async Task<IActionResult> OnPostBookAsync()
        {
            // valider datoene på nytt
            if (FromDate >= ToDate)
            {
                ModelState.AddModelError("", "Til‐dato må være etter fra‐dato.");
                await OnGetAsync();  // hente dropdowns på nytt
                return Page();
            }

            if (BookingRoomId == null)
            {
                ModelState.AddModelError("", "Du må velge et rom å booke.");
                await OnGetAsync();
                return Page();
            }

            // sjekk at rommet fortsatt er ledig
            var room = await _context.Rooms
                .Include(r => r.Reservations)
                .FirstOrDefaultAsync(r => r.Id == BookingRoomId.Value);
            if (room == null)
                return NotFound();

            var collision = room.Reservations.Any(r =>
                !(ToDate <= r.FromDate || FromDate >= r.ToDate));
            if (collision)
            {
                ModelState.AddModelError("", "Beklager, rommet ble akkurat reservert av noen andre.");
                await OnGetAsync();
                return Page();
            }

            // opprett reservasjon
            var newRes = new Reservation
            {
                RoomId = room.Id,
                FromDate = FromDate,
                ToDate = ToDate,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Status = ReservationStatus.Active
            };
            _context.Reservations.Add(newRes);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Rom #{room.Id} ({room.RoomType}) ble booket!";
            return RedirectToPage(); // refresher siden
        }
    }
}
