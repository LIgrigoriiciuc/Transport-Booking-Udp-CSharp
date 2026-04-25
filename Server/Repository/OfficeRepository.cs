using System.Transactions;
using Server.Domain;

namespace Server.Repository;


public class OfficeRepository : GenericRepository<long, Office>
{

    public OfficeRepository(SqliteConnection conn, TransactionManager tx) : base(conn, tx)
    {
    }

    public override string GetTableName() => "offices";

    protected override string BuildInsertSql()
    {
        return "INSERT INTO offices (address) VALUES (@address)";
    }

    protected override void SetInsertParameters(SqliteCommand command, Office office)
    {
        command.Parameters.AddWithValue("@address", office.Address);
        }

    protected override string BuildUpdateSql()
    {
        return "UPDATE offices SET address = @address WHERE id = @id";
    }

    protected override void SetUpdateParameters(SqliteCommand command, Office office)
    {
        command.Parameters.AddWithValue("@address", office.Address);
        command.Parameters.AddWithValue("@id", office.Id);
        }

    protected override Office MapResultSetToEntity(SqliteDataReader reader)
    {
        long id = reader.GetFieldValue<long>(reader.GetOrdinal("id"));
        string address = reader.GetFieldValue<string>(reader.GetOrdinal("address"));

        var office = new Office(id, address);
        return office;
    }
}