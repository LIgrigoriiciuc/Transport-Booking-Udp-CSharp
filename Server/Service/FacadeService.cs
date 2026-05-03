using Server.Domain;
using Serilog;

namespace Server.Service;


public class FacadeService
{
    private static readonly ILogger Logger = Log.ForContext<FacadeService>();
    private readonly AuthService _userService;
    private readonly TripService _tripService;
    private readonly SeatService _seatService;
    private readonly ReservationService _reservationService;
    private readonly OfficeService _officeService;
    private readonly TransactionManager _txManager;

    public FacadeService(
        AuthService userService,
        TripService tripService,
        SeatService seatService,
        ReservationService reservationService,
        OfficeService officeService,
        TransactionManager txManager)
    {
        _userService = userService;
        _tripService = tripService;
        _seatService = seatService;
        _reservationService = reservationService;
        _officeService = officeService;
        _txManager = txManager;
    }

    public User Login(string user, string pass)
    {
        Logger.Debug("Facade login for {User}", user);
        return _userService.Login(user, pass);
    }
    
    // public void Logout() => _userService.Logout();

    public List<Trip> SearchTrips(string destination, DateTime? from, DateTime? to) 
    {
        Logger.Debug("Searching trips with destination {Destination}", destination);
        return _tripService.Search(destination, from, to);
    }

    public List<Seat> GetSeatsForTrip(long tripId) 
    {
        Logger.Debug("Getting seats for trip {TripId}", tripId);
        return _seatService.GetByTripId(tripId);
    }

    public void MakeReservationForSeats(string clientName, List<Seat> chosenSeats, long userId)
    {
        Logger.Debug("Making reservation for {ClientName}", clientName);
        _txManager.Run(() => _reservationService.ReserveSeats(clientName, chosenSeats, userId));
    }

    public void CancelReservation(long reservationId)
    {
        Logger.Debug("Cancelling reservation {ReservationId}", reservationId);
        _txManager.Run(() => _reservationService.Cancel(reservationId));
    }

    public List<Reservation> GetAllReservations() 
    {
        Logger.Debug("Getting all reservations");
        return _reservationService.GetAll();
    }
    public Office? GetOfficeById(long id)
    {
        Logger.Debug("Getting office by id {Id}", id);
        return _officeService.FindById(id);
    }
 
    public int CountFreeSeats(long tripId)
    {
        Logger.Debug("Counting free seats for trip {TripId}", tripId);
        return _seatService.GetFreeByTripId(tripId).Count;
    }
    public Seat? GetSeatById(long seatId)
    {
        Logger.Debug("Getting seat by id {SeatId}", seatId);
        return _seatService.FindById(seatId);
    }
 
    public Reservation? GetReservationById(long reservationId)
    {
        Logger.Debug("Getting reservation by id {ReservationId}", reservationId);
        return _reservationService.FindById(reservationId);
    }
    public User? GetUserById(long userId)
    {
        Logger.Debug("Getting user by id {UserId}", userId);
        return _userService.FindById(userId);
    }
 
    public List<int> GetSeatNumbersByReservation(long reservationId)
    {
        Logger.Debug("Getting seat numbers for reservation {ReservationId}", reservationId);
        return _seatService.GetSeatNumbersByReservation(reservationId);
    }
 
    public long GetTripIdByReservation(long reservationId)
    {
        Logger.Debug("Getting trip id for reservation {ReservationId}", reservationId);
        return _seatService.GetTripIdByReservationId(reservationId) ?? 0L;
    }
}