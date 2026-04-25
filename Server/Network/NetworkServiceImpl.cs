using Server.Service;
using Shared;
using Shared.Network;
using Shared.Proto;

namespace Server.Network;

public class NetworkServiceImpl : INetworkService
{
    private readonly FacadeService _facade;
    private readonly Dictionary<long, IObserver> _observers = new();
    private readonly object _loginLock       = new();
    private readonly object _reservationLock = new();
    
    private readonly ExecutorService _pushExecutor = new(4);
    public NetworkServiceImpl(FacadeService facade)
    {
        _facade = facade;
    }
    public void RegisterObserver(long userId, IObserver observer)
    {
        lock (_loginLock)
            _observers[userId] = observer;
    }
    public void UnregisterObserver(long userId)
    {
        lock (_loginLock)
            _observers.Remove(userId);
    }
    public ProtoUser Login(string username, string password)
    {
        lock (_loginLock)
        {
            var user = _facade.Login(username, password)
                       ?? throw new Exception("Authentication failed.");
            if (_observers.ContainsKey(user.Id))
                throw new Exception("User already logged in.");
            var office = _facade.GetOfficeById(user.OfficeId);
            return ProtoUtils.ToProto(user, office);
            }
    }
    public void Logout(long userId)
    {
        lock (_loginLock)
            _observers.Remove(userId);
    }
    public TripList SearchTrips(string destination, string from, string to)
    {
        var fromDt = string.IsNullOrWhiteSpace(from) ? (DateTime?)null : DateTime.Parse(from);
        var toDt   = string.IsNullOrWhiteSpace(to)   ? (DateTime?)null : DateTime.Parse(to);
        var trips  = _facade.SearchTrips(destination, fromDt, toDt);
        var free   = trips.Select(t => _facade.CountFreeSeats(t.Id)).ToList();
        return ProtoUtils.ToTripList(trips, free);
    }
    public SeatList GetSeats(long tripId)
    {
        var seats = _facade.GetSeatsForTrip(tripId);
        return ProtoUtils.ToSeatList(seats);
    }
    public ReservationList GetAllReservations()
    {
        var reservations   = _facade.GetAllReservations();
        var seatsPerRes    = reservations.Select(r => _facade.GetSeatNumbersByReservation(r.Id)).ToList();
        var users          = reservations.Select(r => _facade.GetUserById(r.UserId)).ToList();
        var tripIds        = reservations.Select(r => _facade.GetTripIdByReservation(r.Id)).ToList();
        return ProtoUtils.ToReservationList(reservations, seatsPerRes, users, tripIds);
    }
    public void MakeReservation(string clientName, List<long> seatIds, long userId)
    {
        long tripId;
        lock (_reservationLock)
        {
            var seats = seatIds.Select(_facade.GetSeatById).ToList();
            foreach (var s in seats)
                if (s.IsReserved)
                    throw new Exception($"Seat {s.Number} is already reserved.");
            _facade.MakeReservationForSeats(clientName, seats, userId);
            tripId = seats[0].TripId; // all seats same trip
        }
        NotifyPush(tripId); // outside lock
    }
    public void CancelReservation(long reservationId)
    {
        long tripId;
        lock (_reservationLock)
        {
            var res = _facade.GetReservationById(reservationId)
                      ?? throw new Exception("Reservation not found or already cancelled.");
            tripId = _facade.GetTripIdByReservation(reservationId);
            _facade.CancelReservation(reservationId);
        }
        NotifyPush(tripId); // outside lock
    }
    private void NotifyPush(long tripId)
    {
        var reservations = GetAllReservations();
        List<IObserver> snapshot;
        lock (_loginLock)
            snapshot = _observers.Values.ToList();

        foreach (var observer in snapshot)
        {
            var obs = observer;
            Task.Run(() => obs.OnPushReceived(
                new PushPayload { UpdatedTripId = tripId, Reservations = reservations }
            ));
        }
    }
}
