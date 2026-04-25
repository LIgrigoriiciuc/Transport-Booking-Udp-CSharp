namespace Server.Util;

public class DatabaseConnection
{
    private static SqliteConnection? _instance;
    private static readonly string DbUrl;

    static DatabaseConnection()
    {
        var settings = ConfigurationManager.ConnectionStrings["DefaultConnection"];
        DbUrl = settings?.ConnectionString ?? "Data Source=transport.db";
        }

    public static SqliteConnection Instance
    {
        get
        {
            if (_instance == null)
            {
                try
                {
                    _instance = new SqliteConnection(DbUrl);
                    _instance.Open();
                    InitSchema(_instance);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return _instance;
        }
    }

    private static void InitSchema(SqliteConnection conn)
    {
        using var command = conn.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS offices (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                address TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL UNIQUE,
                password TEXT NOT NULL,
                fullName TEXT,
                officeId INTEGER,
                FOREIGN KEY (officeId) REFERENCES offices(id)
            );
            CREATE TABLE IF NOT EXISTS trips (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                destination TEXT NOT NULL,
                time TEXT NOT NULL,
                busNumber TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS reservations (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                clientName TEXT NOT NULL,
                reservationTime TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS seats (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                number INTEGER NOT NULL,
                isReserved INTEGER NOT NULL DEFAULT 0,
                trip_id INTEGER NOT NULL,
                reservation_id INTEGER,
                FOREIGN KEY (trip_id) REFERENCES trips(id),
                FOREIGN KEY (reservation_id) REFERENCES reservations(id)
            );";
        command.ExecuteNonQuery();
    }

    public static void Close()
    {
        if (_instance != null)
        {
            _instance.Close();
            _instance = null;
        }
    }
}