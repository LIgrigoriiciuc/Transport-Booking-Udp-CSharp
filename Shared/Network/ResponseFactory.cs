using Shared.Proto;

namespace Shared.Network;

public static class ResponseFactory
{
    public static Response OkResponse() =>
        new Response { Type = Response.Types.Type.Ok };

    public static Response ErrorResponse(string message) =>
        new Response { Type = Response.Types.Type.Error, Error = message };

    public static Response LoginOkResponse(ProtoUser user) =>
        new Response { Type = Response.Types.Type.LoginOk, User = user };

    public static Response TripsResponse(TripList trips) =>
        new Response { Type = Response.Types.Type.Trips, Trips = trips };

    public static Response SeatsResponse(SeatList seats) =>
        new Response { Type = Response.Types.Type.Seats, Seats = seats };

    public static Response ReservationsResponse(ReservationList reservations) =>
        new Response { Type = Response.Types.Type.Reservations, Reservations = reservations };

    public static Response PushResponse(long updatedTripId, ReservationList reservations) =>
        new Response
        {
            Type = Response.Types.Type.Push,
            Push = new PushPayload
            {
                UpdatedTripId = updatedTripId,
                Reservations  = reservations
            }
        };
}