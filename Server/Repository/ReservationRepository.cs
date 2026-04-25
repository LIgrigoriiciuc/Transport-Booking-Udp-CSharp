using System.Transactions;
using Server.Domain;

namespace Server.Repository;

public class ReservationRepository : GenericRepository<long, Reservation>
{

    public ReservationRepository(SqliteConnection conn, TransactionManager tx) : base(conn, tx)
    {
    }

    public override string GetTableName() => "reservations";

    protected override string BuildInsertSql()
    {
        return "INSERT INTO reservations (clientName, reservationTime) VALUES (@clientName, @reservationTime)";
    }

    protected override void SetInsertParameters(SqliteCommand command, Reservation reservation)
    {
        command.Parameters.AddWithValue("@clientName", reservation.ClientName);
        command.Parameters.AddWithValue("@reservationTime", reservation.ReservationTime.ToString("o"));
        }

    protected override string BuildUpdateSql()
    {
        return "UPDATE reservations SET clientName = @clientName, reservationTime = @reservationTime WHERE id = @id";
    }

    protected override void SetUpdateParameters(SqliteCommand command, Reservation reservation)
    {
        command.Parameters.AddWithValue("@clientName", reservation.ClientName);
        command.Parameters.AddWithValue("@reservationTime", reservation.ReservationTime.ToString("o"));
        command.Parameters.AddWithValue("@id", reservation.Id);
        }

    protected override Reservation MapResultSetToEntity(SqliteDataReader reader)
    {
        long id = reader.GetInt64(reader.GetOrdinal("id"));
        string clientName = reader.GetString(reader.GetOrdinal("clientName"));
        DateTime reservationTime = DateTime.Parse(reader.GetString(reader.GetOrdinal("reservationTime")));

        var reservation = new Reservation(id, clientName, reservationTime);
        return reservation;
    }
}