using Microsoft.Extensions.Configuration;
using Serilog;
using Server.Network;
using Server.Repository;
using Server.Service;
using Server.Util;
using Serilog;

namespace Server;

public class StartServer
{
    private static readonly ILogger Logger = Log.ForContext<StartServer>();
    private const int DefaultPort = 65535;
    static void Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();
        DatabaseInitializer.Initialize();
        int port = LoadPort();
        Logger.Information("Server starting on port {Port}", port);
        var seatRepo = new SeatRepository();
        var tripRepo = new TripRepository();
        var resRepo = new ReservationRepository();
        var userRepo = new UserRepository();
        var officeRepo = new OfficeRepository();
        Logger.Debug("Repositories initialized");
        var officeService = new OfficeService(officeRepo);
        var authService = new AuthService(userRepo);
        var tripService = new TripService(tripRepo);
        var seatService = new SeatService(seatRepo);
        var resService = new ReservationService(resRepo, seatService);
        var txManager = new TransactionManager();
        var facade  = new FacadeService(authService, tripService, seatService, resService, officeService, txManager);
        var service = new NetworkServiceImpl(facade);
        var server  = new ConcurrentServer(port, service);
        Logger.Debug("Services initialized");

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            server.Stop();
            Console.WriteLine("Server stopped.");
        };

        server.Start();
        Logger.Information("Server started");
    }
    private static int LoadPort()
    {
        var props = new System.Collections.Specialized.NameValueCollection();
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "server.properties");
            if (File.Exists(path))
                foreach (var line in File.ReadAllLines(path))
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                        props[parts[0].Trim()] = parts[1].Trim();
                }
        }
        catch
        {
            //default
        }

        return int.TryParse(props["server.port"], out int p) ? p : DefaultPort;
    }
}