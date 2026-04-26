using System.Collections.Concurrent;
using Server.Service;
using Shared;
using Shared.Network;
using Shared.Proto;
using Shared.Util;

namespace Server.Network;

public class NetworkServiceImpl : INetworkService
{
    private readonly FacadeService _facade;
    private readonly ConcurrentDictionary<long, IObserver> _observers = new();
    private readonly object _reservationLock = new();
    
    public NetworkServiceImpl(FacadeService facade)
    {
        _facade = facade;
    }
    public void RegisterObserver(long userId, IObserver observer)
    {
        _observers[userId] = observer;
    }
    public void Logout(long userId)
    {
        _observers.TryRemove(userId, out _);
    }
    
    public ProtoUser Login(string username, string password)
    {
        var user = _facade.Login(username, password)
                   ?? throw new Exception("Authentication failed.");
        
        if (!_observers.TryAdd(user.Id, null!))
            throw new Exception("User already logged in.");
        var office = _facade.GetOfficeById(user.OfficeId);
        return ProtoUtils.ToProto(user, office);
        
    }
    public TripList SearchTrips(string destination, string from, string to)
    {
        var fromDt = string.IsNullOrWhiteSpace(from) ? (DateTime?)null : DateTimeUtils.Parse(from);
        var toDt   = string.IsNullOrWhiteSpace(to)   ? (DateTime?)null : DateTimeUtils.Parse(to);
        var trips = _facade.SearchTrips(destination, fromDt, toDt);
        var free = trips.Select(t => _facade.CountFreeSeats(t.Id)).ToList();
        return ProtoUtils.ToTripList(trips, free);
    }
    public SeatList GetSeats(long tripId)
    {
        var seats = _facade.GetSeatsForTrip(tripId);
        return ProtoUtils.ToSeatList(seats);
    }
    public ReservationList GetAllReservations()
    {
        var reservations = _facade.GetAllReservations();
        var seatsPerRes= reservations.Select(r => _facade.GetSeatNumbersByReservation(r.Id)).ToList();
        var users= reservations.Select(r => _facade.GetUserById(r.UserId)).ToList();
        var tripIds = reservations.Select(r => _facade.GetTripIdByReservation(r.Id)).ToList();
        return ProtoUtils.ToReservationList(reservations, seatsPerRes, users, tripIds);
    }
    internal long MakeReservationCore(string clientName, List<long> seatIds, long userId)
    {
        lock (_reservationLock)
        {
            var seats = seatIds.Select(_facade.GetSeatById).ToList();
            foreach (var s in seats)
                if (s.IsReserved)
                    throw new Exception($"Seat {s.Number} is already reserved.");
            _facade.MakeReservationForSeats(clientName, seats, userId);
            return seats[0].TripId;
        }
    }
    public void MakeReservation(string clientName, List<long> seatIds, long userId)
    {
        var tripId = MakeReservationCore(clientName, seatIds, userId);
        NotifyPush(tripId);
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
    internal void NotifyPush(long tripId)
    {
        var reservations = GetAllReservations();
        //snapshot
        //avoid NullPointerE mid login nulls
        foreach (var observer in _observers.Values.Where(o => o != null))
        {
            var obs = observer;
            //.NET ThreadPool under the hood
            Task.Run(() => obs.OnPushReceived(
                new PushPayload
                {
                    UpdatedTripId = tripId, 
                    Reservations = reservations
                }
            ));
        }
    }
}
