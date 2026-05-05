using System.Configuration;
using Microsoft.Data.Sqlite;
using Serilog;

namespace Server.Util;

public class DatabaseConnection
{
    private static readonly ILogger Logger = Log.ForContext<DatabaseConnection>();
    private static readonly string DbUrl;
    //this static thread is overengineered for this app - because we have a shared lock by reserve, unreserve (the only methods who imply transactions) BUT for further refactoring it's necessary
    //in java I had a local thread on a Connection object and there it was necessary since methods implying a transaction should use a specific connection, and the ones that don't imply a transaction aren't locked so they can run in parallel with the locked ones
    //otherwise, they can't conflict with methods that don't imply a transaction, even if they run in parallel because they receive a new connection and the value in the local thread remains null
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