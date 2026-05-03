using System.IO;
using Microsoft.Data.Sqlite;

namespace Server.Util;

public static class DatabaseInitializer
{
    public static void Initialize()
    {
        using var holder = DatabaseConnection.GetConnection();
        var conn = holder.Connection;

        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='offices'";
        var count = (long)checkCmd.ExecuteScalar()!;

        if (count > 0)
        {
            Console.WriteLine("Database already initialized.");
            return;
        }

        var sqlPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "DatabaseInit.sql");
        var sql = File.ReadAllText(sqlPath);

        var initCmd = conn.CreateCommand();
        initCmd.CommandText = sql;
        initCmd.ExecuteNonQuery();

        Console.WriteLine("Database initialized successfully.");
    }
}
