using Server.Domain;
using Server.Repository;
using Serilog;

namespace Server.Service;


public class SeatService : AbstractService<long, Seat>
{
    private static readonly ILogger Logger = Log.ForContext<SeatService>();
    public SeatService(SeatRepository seatRepository) : base(seatRepository)
    {
    }

    public List<Seat> GetByTripId(long tripId)
    {
        Logger.Debug("Getting seats for trip {TripId}", tripId);
        var filter = new Filter();
        filter.AddFilter("trip_id", tripId);
        return Filter(filter);
    }

    public List<Seat> GetByReservationId(long reservationId)
    {
        Logger.Debug("Getting seats for reservation {ReservationId}", reservationId);
        var filter = new Filter();
        filter.AddFilter("reservation_id", reservationId);
        return Filter(filter);
    }
    public List<Seat> GetFreeByTripId(long tripId)
    {
        Logger.Debug("Getting free seats for trip {TripId}", tripId);
        var filter = new Filter();
        filter.AddFilter("trip_id", tripId);
        filter.AddFilter("isReserved", 0);
        return Filter(filter);
    }
    public List<int> GetSeatNumbersByReservation(long reservationId)
        => GetByReservationId(reservationId)
            .Select(s => s.Number)
            .ToList();
    
    public long? GetTripIdByReservationId(long reservationId)
        => GetByReservationId(reservationId)
            .Select(s => (long?)s.TripId)
            .FirstOrDefault();
}
