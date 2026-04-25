using Server.Network;
using Server.Repository;
using Server.Service;

namespace Server;

public class StartServer
{
    static void Main(string[] args)
    {
        int port = LoadPort();

        // repositories
        var seatRepo    = new SeatRepository();
        var tripRepo    = new TripRepository();
        var resRepo     = new ReservationRepository();
        var userRepo    = new UserRepository();
        var officeRepo  = new OfficeRepository();

        // services
        var officeService = new OfficeService(officeRepo);
        var authService   = new AuthService(userRepo, officeService);
        var tripService   = new TripService(tripRepo);
        var seatService   = new SeatService(seatRepo);
        var resService    = new ReservationService(resRepo, seatService);

        var facade  = new FacadeService(authService, tripService, seatService, resService, officeService);
        var service = new ReservationServiceImpl(facade);
        var server  = new ConcurrentServer(port, service);

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            server.Stop();
            Console.WriteLine("Server stopped.");
        };

        server.Start();
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
        catch { /* use default */ }

        return int.TryParse(props["server.port"], out int p) ? p : DefaultPort;
    }
}