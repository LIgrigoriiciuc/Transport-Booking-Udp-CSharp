namespace Server.Domain;

public class Reservation : Entity<long>
{
    public string ClientName { get; set; }
    public DateTime ReservationTime { get; set; }
    public Reservation(string clientName) : base()
    {
        ClientName = clientName;
        ReservationTime = DateTime.Now;
    }
    public Reservation(long id, string clientName, DateTime reservationTime) : base(id)
    {
        ClientName = clientName;
        ReservationTime = reservationTime;
    }
}