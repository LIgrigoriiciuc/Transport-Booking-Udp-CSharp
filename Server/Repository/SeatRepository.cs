using System.Transactions;
using Microsoft.Data.Sqlite;
using Server.Domain;

namespace Server.Repository;

public class SeatRepository : GenericRepository<long, Seat>
{
    public SeatRepository(SqliteConnection conn, TransactionManager tx) : base(conn, tx)
    {
    }

    public override string GetTableName() => "seats";

    protected override string BuildInsertSql()
    {
        return "INSERT INTO seats (number, isReserved, trip_id, reservation_id) VALUES (@number, @isReserved, @tripId, @reservationId)";
    }

    protected override void SetInsertParameters(SqliteCommand command, Seat seat)
    {
        command.Parameters.AddWithValue("@number", seat.Number);
        command.Parameters.AddWithValue("@isReserved", seat.IsReserved ? 1 : 0);
        command.Parameters.AddWithValue("@tripId", seat.TripId);
        command.Parameters.AddWithValue("@reservationId", (object?)seat.ReservationId ?? DBNull.Value);
    }

    protected override string BuildUpdateSql()
    {
        return "UPDATE seats SET number = @number, isReserved = @isReserved, trip_id = @tripId, reservation_id = @reservationId WHERE id = @id";
    }

    protected override void SetUpdateParameters(SqliteCommand command, Seat seat)
    {
        command.Parameters.AddWithValue("@number", seat.Number);
        command.Parameters.AddWithValue("@isReserved", seat.IsReserved ? 1 : 0);
        command.Parameters.AddWithValue("@tripId", seat.TripId);
        command.Parameters.AddWithValue("@reservationId", (object?)seat.ReservationId ?? DBNull.Value);
        command.Parameters.AddWithValue("@id", seat.Id);
    }

    protected override Seat MapResultSetToEntity(SqliteDataReader reader)
    {
        long id = reader.GetInt64(reader.GetOrdinal("id"));
        int number = reader.GetInt32(reader.GetOrdinal("number"));
        bool isReserved = reader.GetInt32(reader.GetOrdinal("isReserved")) == 1;
        long tripId = reader.GetInt64(reader.GetOrdinal("trip_id"));
        
        int resIdOrdinal = reader.GetOrdinal("reservation_id");
        long? reservationId = reader.IsDBNull(resIdOrdinal) ? null : reader.GetInt64(resIdOrdinal);

        var seat = new Seat(id, number, isReserved, tripId, reservationId);
            
        return seat;
    }
}