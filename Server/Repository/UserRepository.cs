using System.Transactions;
using Server.Domain;

namespace Server.Repository;


public class UserRepository : GenericRepository<long, User>
{

    public UserRepository(SqliteConnection conn, TransactionManager tx) : base(conn, tx)
    {
        }

    public override string GetTableName() => "users";

    protected override string BuildInsertSql()
    {
        return "INSERT INTO users (username, password, fullName, officeId) VALUES (@username, @password, @fullName, @officeId)";
    }

    protected override void SetInsertParameters(SqliteCommand command, User user)
    {
        command.Parameters.AddWithValue("@username", user.Username);
        command.Parameters.AddWithValue("@password", user.Password);
        command.Parameters.AddWithValue("@fullName", user.FullName);
        command.Parameters.AddWithValue("@officeId", user.OfficeId);
        
        }

    protected override string BuildUpdateSql()
    {
        return "UPDATE users SET username = @username, password = @password, fullName = @fullName, officeId = @officeId WHERE id = @id";
    }

    protected override void SetUpdateParameters(SqliteCommand command, User user)
    {
        command.Parameters.AddWithValue("@username", user.Username);
        command.Parameters.AddWithValue("@password", user.Password);
        command.Parameters.AddWithValue("@fullName", user.FullName);
        command.Parameters.AddWithValue("@officeId", user.OfficeId);
        command.Parameters.AddWithValue("@id", user.Id);

        }

    protected override User MapResultSetToEntity(SqliteDataReader reader)
    {
        var user = new User(
            reader.GetInt64(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("fullName")),
            reader.GetInt64(reader.GetOrdinal("officeId"))
        );

        return user;
    }
}