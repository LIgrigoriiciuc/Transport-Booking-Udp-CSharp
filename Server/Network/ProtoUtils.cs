using Server.Domain;
using Shared.Proto;

namespace Server.Network;

public static class ProtoUtils
{
    public static ProtoUser ToProto(User user, Office? office) => new ProtoUser
    {
        Id = user.Id,
        FullName  = user.FullName,
        OfficeAddress = office?.Address ?? ""
    };
    public static ProtoTrip ToProto(Trip trip, int freeSeats) => new ProtoTrip
    {
        Id          = trip.Id,
        Destination = trip.Destination,
        Time        = trip.Time.ToString("yyyy-MM-dd HH:mm"),
        BusNumber   = trip.BusNumber,
        FreeSeats   = freeSeats
    };
    public static ProtoSeat ToProto(Seat seat) => new ProtoSeat
    {
        Id            = seat.Id,
        Number        = seat.Number,
        Reserved      = seat.IsReserved,
        TripId        = seat.TripId,
        ReservationId = seat.ReservationId ?? 0
    };
    public static ProtoReservation ToProto(Reservation r, List<int> seatNumbers, 
        User user, long tripId) => new ProtoReservation
    {
        Id              = r.Id,
        ClientName      = r.ClientName,
        ReservationTime = r.ReservationTime.ToString("yyyy-MM-dd HH:mm"),
        TripId          = tripId,
        UserUsername    = user.Username,
        SeatNumbers     = { seatNumbers }
    };
    public static TripList ToTripList(List<Trip> trips, List<int> freeSeats)
    {
        var list = new TripList();
        for (int i = 0; i < trips.Count; i++)
            list.Trips.Add(ToProto(trips[i], freeSeats[i]));
        return list;
    }

    public static SeatList ToSeatList(List<Seat> seats)
    {
        var list = new SeatList();
        seats.ForEach(s => list.Seats.Add(ToProto(s)));
        return list;
    }

    public static ReservationList ToReservationList(List<Reservation> reservations,
        List<List<int>> seatsPerRes, List<User> users, List<long> tripIds)
    {
        var list = new ReservationList();
        for (int i = 0; i < reservations.Count; i++)
            list.Reservations.Add(ToProto(reservations[i], seatsPerRes[i], users[i], tripIds[i]));
        return list;
    }
    
}