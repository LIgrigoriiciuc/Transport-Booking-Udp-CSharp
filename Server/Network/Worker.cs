using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Google.Protobuf;
using Serilog;
using Shared;
using Shared.Network;
using Shared.Proto;

namespace Server.Network;

public class Worker : IObserver
{ 
    private static readonly ILogger Logger = Log.ForContext<Worker>();
    private readonly NetworkServiceImpl _service;
    private readonly TcpClient _client;
    private readonly NetworkStream  _stream;
    private long? _loggedUserId;
    public Worker(NetworkServiceImpl service, TcpClient client)
    {
        _service = service;
        _client = client;
        _stream = client.GetStream();
    }

    public void Run()
    {
        Logger.Debug("Worker started for client");
        try
        {
            while (true)
            {
                var request = Request.Parser.ParseDelimitedFrom(_stream);
                var response = HandleRequest(request);
                if (response != null)
                    SendResponse(response);
            }
        }
        catch (Exception)
        {
            Logger.Debug("Client connection dropped");
        }
        finally
        {
            Cleanup();
        }
    }

    //called from thread pool
    public void OnPushReceived(PushPayload push)
    {
        Logger.Debug("Sending push notification");
        SendResponse(new Response
        {
            Type = Response.Types.Type.Push,
            Push = push 
        });
    }
    private Response? HandleRequest(Request req)
    {
        Logger.Debug("Handling request of type {Type}", req.Type);
        try
        {
            switch (req.Type)
            {
                case Request.Types.Type.Login:
                {
                    var p = req.Login;
                    var user = _service.Login(p.Username, p.Password);
                    Logger.Information("User {Username} logged in", p.Username);
                    _loggedUserId = user.Id;
                    _service.RegisterObserver(user.Id, this);
                    return ResponseFactory.LoginOkResponse(user);
                }
                case Request.Types.Type.Logout:
                {
                    if (_loggedUserId.HasValue)
                    {
                        _service.Logout(_loggedUserId.Value);
                        Logger.Information("User logged out");
                        _loggedUserId = null;
                    }
                    return ResponseFactory.OkResponse();
                }
                case Request.Types.Type.SearchTrips:
                {
                    var p= req.SearchTrips;
                    var trips = _service.SearchTrips(p.Destination, p.From, p.To);
                    return ResponseFactory.TripsResponse(trips);
                }
                case Request.Types.Type.GetSeats:
                {
                    var seats = _service.GetSeats(req.GetSeats.TripId);
                    return ResponseFactory.SeatsResponse(seats);
                }
                case Request.Types.Type.GetReservations:
                {
                    var res = _service.GetAllReservations();
                    return ResponseFactory.ReservationsResponse(res);
                }
                case Request.Types.Type.MakeReservation:
                {
                    var p = req.MakeReservation;
                    var tripId = _service.MakeReservationCore(
                        p.ClientName, p.SeatIds.ToList(), p.UserId);
                    Logger.Information("Reservation made for {ClientName}", p.ClientName);
                    SendResponse(ResponseFactory.OkResponse());
                    _service.NotifyPush(tripId);
                    return null;
                }
                case Request.Types.Type.CancelReservation:
                {
                    Logger.Debug("Cancelling reservation {ReservationId}", req.CancelReservation.ReservationId);
                    _service.CancelReservation(req.CancelReservation.ReservationId);
                    return ResponseFactory.OkResponse();
                }
                default:
                    return ResponseFactory.ErrorResponse("Unknown request type");
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error handling request {Type}", req.Type);
            return ResponseFactory.ErrorResponse(e.Message);
        }
    }
    private void SendResponse(Response response)
    {
        Logger.Debug("Sending response of type {Type}", response.Type);
        lock (_stream)
        {
            response.WriteDelimitedTo(_stream);
            _stream.Flush();
        }
    }

    private void Cleanup()
    {
        Logger.Debug("Cleaning up worker");
        if (_loggedUserId.HasValue)
            _service.Logout(_loggedUserId.Value); // covers both clean logout miss and TCP drop
        try
        {
            _stream.Close(); 
            _client.Close();
        }
        catch { /* ignore */ }
    }

}