using Server.Domain;
using Server.Repository;

namespace Server.Service;


public class SeatService : AbstractService<long, Seat>
{
    public SeatService(SeatRepository seatRepository) : base(seatRepository)
    {
    }

    public List<Seat> GetByTripId(long tripId)
    {
        var filter = new Filter();
        filter.AddFilter("trip_id", tripId);
        return Filter(filter);
    }

    public List<Seat> GetByReservationId(long reservationId)
    {
        var filter = new Filter();
        filter.AddFilter("reservation_id", reservationId);
        return Filter(filter);
    }
}