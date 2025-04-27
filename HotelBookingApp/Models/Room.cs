namespace HotelBookingApp.Models
{
    public class Room
    {

        public int Id { get; set; }

        public int NumberOfBeds { get; set; }

        public string RoomType { get; set; }

        public decimal PricePerNight { get; set; }

        public Boolean IsAvailable { get; set; }

        public List<Reservation> Reservations { get; set; }
    }
}
