namespace Server.Domain;
public class Seat : Entity<long>
{
    public int Number { get; set; }
    public bool IsReserved { get; private set; }
    public long TripId { get; set; }
    public long? ReservationId { get; private set; }

    public Seat(long id, int number, bool isReserved, long tripId, long? reservationId) 
        : base(id)
    {
        Number = number;
        IsReserved = isReserved;
        TripId = tripId;
        ReservationId = reservationId;
    }
    
}