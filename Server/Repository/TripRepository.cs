using System.Transactions;
using Server.Domain;

namespace Server.Repository;


public class TripRepository : GenericRepository<long, Trip>
{
    public TripRepository(SqliteConnection conn, TransactionManager tx) : base(conn, tx)
    {
    }

    public override string GetTableName() => "trips";

    protected override string BuildInsertSql()
    {
        return "INSERT INTO trips (destination, time, busNumber) VALUES (@destination, @time, @busNumber)";
    }

    protected override void SetInsertParameters(SqliteCommand command, Trip trip)
    {
        command.Parameters.AddWithValue("@destination", trip.Destination);
        command.Parameters.AddWithValue("@time", trip.Time.ToString("o"));
        command.Parameters.AddWithValue("@busNumber", trip.BusNumber);
    }

    protected override string BuildUpdateSql()
    {
        return "UPDATE trips SET destination = @destination, time = @time, busNumber = @busNumber WHERE id = @id";
    }

    protected override void SetUpdateParameters(SqliteCommand command, Trip trip)
    {
        command.Parameters.AddWithValue("@destination", trip.Destination);
        command.Parameters.AddWithValue("@time", trip.Time.ToString("o"));
        command.Parameters.AddWithValue("@busNumber", trip.BusNumber);
        command.Parameters.AddWithValue("@id", trip.Id);
    }

    protected override Trip MapResultSetToEntity(SqliteDataReader reader)
    {
        long id = reader.GetInt64(reader.GetOrdinal("id"));
        string destination = reader.GetString(reader.GetOrdinal("destination"));
        DateTime time = DateTime.Parse(reader.GetString(reader.GetOrdinal("time")));
        string busNumber = reader.GetString(reader.GetOrdinal("busNumber"));

        var trip = new Trip(id, destination, time, busNumber);
        
        return trip;
    }
}