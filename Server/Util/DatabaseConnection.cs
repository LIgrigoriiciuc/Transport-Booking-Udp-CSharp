using System.Configuration;
using Microsoft.Data.Sqlite;
using Serilog;

namespace Server.Util;

public class DatabaseConnection
{
    private static readonly ILogger Logger = Log.ForContext<DatabaseConnection>();
    private static readonly string DbUrl;
 
    [ThreadStatic]
    private static SqliteConnection? _transactionConnection;
    [ThreadStatic]
    private static SqliteTransaction? _activeTransaction;
    static DatabaseConnection()
    {
        var settings = ConfigurationManager.ConnectionStrings["DefaultConnection"];
        var baseUrl = settings?.ConnectionString ?? "Data Source=transport.db";
        //pooling=true closes itself
        DbUrl = baseUrl.TrimEnd(';') + ";Pooling=True;";
    }

    public static void BindConnection(SqliteConnection conn, SqliteTransaction tx)
    {
        Logger.Debug("Binding transaction");
        _transactionConnection = conn;
        _activeTransaction = tx;
    }
 
    public static void UnbindConnection()
    {
        Logger.Debug("Unbinding transaction");
        _transactionConnection = null;
        _activeTransaction = null;
    }

    public static ConnectionHolder GetConnection()
    {
        Logger.Debug("Getting database connection");
        if (_transactionConnection != null)
            return new ConnectionHolder(_transactionConnection, false);
 
        var conn = new SqliteConnection(DbUrl);
        conn.Open();
        return new ConnectionHolder(conn, true);
    }
    public static SqliteTransaction? GetActiveTransaction() => _activeTransaction;
}