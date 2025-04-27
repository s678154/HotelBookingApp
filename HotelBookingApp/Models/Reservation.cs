using System.ComponentModel.DataAnnotations;

namespace HotelBookingApp.Models
{
    // For at en bruker kan sjekke status på bookingen sin:
    public enum ReservationStatus
    {
        Active,
        Cancelled,
        Completed
    }

    public class Reservation
    {
        public int Id { get; set; }

        // For å vite hvem som eier reservasjonen
        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        public int RoomId { get; set; }

        public Room Room { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Active;

    }
}
