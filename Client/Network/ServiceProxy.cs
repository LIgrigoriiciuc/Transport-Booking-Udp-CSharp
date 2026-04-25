using System.Collections.Concurrent;
using System.Net.Sockets;
using Google.Protobuf;
using Shared;
using Shared.Network;
using Shared.Proto;

namespace Client.Network;

public class ServiceProxy : INetworkService
{
    private readonly string _host;
    private readonly int    _port;
    private TcpClient?     _client;
    private NetworkStream? _stream;
    private Thread? _readerThread;
    private volatile bool  _running;
    private readonly BlockingCollection<Response> _responses = new();

    public IObserver? Observer { get; set; }
    public ServiceProxy(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public void Connect()
    {
        _client = new TcpClient(_host, _port);
        _stream = _client.GetStream();
        _running = true;
        _readerThread = new Thread(ReadLoop)
        {
            IsBackground = true,
            Name = "protoreader"
        };
        _readerThread.Start();
    }

    public void Disconnect()
    {
        _running = false;
        _stream?.Close();
        _client?.Close();
    }

    private void ReadLoop()
    {
        while (_running)
        {
            try
            {
                var response = Response.Parser.ParseDelimitedFrom(_stream!);
                if (response.Type == Response.Types.Type.Push)
                    Observer?.OnPushReceived(response.Push); // push — straight to observer
                else
                    _responses.Add(response); // normal — goes to Exchange()
            }
            catch
            {
                if (_running) break;
            }
        }
    }

    private Response Exchange(Request request)
    {
        Send(request);
        if (!_responses.TryTake(out var response, TimeSpan.FromSeconds(10)))
            throw new Exception("Server did not respond in time.");
        if (response.Type == Response.Types.Type.Error)
            throw new Exception(response.Error);
        return response;

    }

    private void Send(Request request)
    {
        lock (_stream!)
        {
            request.WriteDelimitedTo(_stream);
            _stream.Flush();
        }
    }

    public ProtoUser Login(string username, string password)
    {
        var response = Exchange(new Request
        {
            Type = Request.Types.Type.Login,
            Login = new LoginPayload
            {
                Username = username,
                Password = password
            }
        });
        return response.User;
    }

    public void Logout(long userId)
    {
        Exchange(new Request
        {
            Type = Request.Types.Type.Logout,
            Logout = new LogoutPayload
            {
                UserId = userId
            }
        });
        Observer = null;
    }
    public TripList SearchTrips(string destination, string from, string to)
    {
        var response = Exchange(new Request
        {
            Type        = Request.Types.Type.SearchTrips,
            SearchTrips = new SearchTripsPayload
                { Destination = destination, From = from, To = to }
        });
        return response.Trips;
    }

    public SeatList GetSeats(long tripId)
    {
        var response = Exchange(new Request
        {
            Type     = Request.Types.Type.GetSeats,
            GetSeats = new GetSeatsPayload { TripId = tripId }
        });
        return response.Seats;
    }

    public ReservationList GetAllReservations()
    {
        var response = Exchange(new Request
        {
            Type = Request.Types.Type.GetReservations
        });
        return response.Reservations;
    }
    public void MakeReservation(string clientName, List<long> seatIds, long userId)
    {
        var payload = new MakeReservationPayload
            { ClientName = clientName, UserId = userId };
        payload.SeatIds.AddRange(seatIds);
        Exchange(new Request { Type = Request.Types.Type.MakeReservation, MakeReservation = payload });
    }

    public void CancelReservation(long reservationId)
    {
        Exchange(new Request
        {
            Type              = Request.Types.Type.CancelReservation,
            CancelReservation = new CancelReservationPayload { ReservationId = reservationId }
        });
    }




}