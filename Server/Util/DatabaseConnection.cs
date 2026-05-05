using System.Configuration;
using Microsoft.Data.Sqlite;
using Serilog;

namespace Server.Util;

public class DatabaseConnection
{
    private static readonly ILogger Logger = Log.ForContext<DatabaseConnection>();
    private static readonly string DbUrl;
    //this ThreadStatic is overengineered for this app since reserve and unreserve share a lock and are the only methods who imply transactions 
    //for further additions in matter of transactions it could be useful
    //in Java I had ThreadLocal<Connection> and there it was necessary since db methods inside transaction need a specific connection
    //the ones that don't imply a transaction run unlocked and in parallel with the locked ones so connection isolation per thread matters
    //otherwise, the methods that don't imply transactions can't conflict even if they run in parallel because they receive a new connection and the value of the connection in the ThreadLocal remains null
    [ThreadStatic]
    private static SqliteTransaction? _activeTransaction;
    static DatabaseConnection()
    {
        var settings = ConfigurationManager.ConnectionStrings["DefaultConnection"];
        var baseUrl = settings?.ConnectionString ?? "Data Source=transport.db";
        //pooling=true closes itself
        DbUrl = baseUrl.TrimEnd(';') + ";Pooling=True;";
    }

    public static void BindConnection(SqliteTransaction tx)
    {
        Logger.Debug("Binding transaction");
        _activeTransaction = tx;
    }
 
    public static void UnbindConnection()
    {
        Logger.Debug("Unbinding transaction");
        _activeTransaction = null;
    }

    public static ConnectionHolder GetConnection()
    {
        Logger.Debug("Getting database connection");
        if (_activeTransaction != null)
            return new ConnectionHolder(_activeTransaction.Connection!, false);
 
        var conn = new SqliteConnection(DbUrl);
        conn.Open();
        return new ConnectionHolder(conn, true);
    }
    public static SqliteTransaction? GetActiveTransaction() => _activeTransaction;
}
