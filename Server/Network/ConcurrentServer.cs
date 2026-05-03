using System.Net;
using System.Net.Sockets;
using Serilog;

namespace Server.Network;

public class ConcurrentServer
{
    private static readonly ILogger Logger = Log.ForContext<ConcurrentServer>();
    private readonly int _port;
    private readonly NetworkServiceImpl _service;
    private TcpListener _listener;
    public ConcurrentServer(int port, NetworkServiceImpl service)
    {
        _port = port;
        _service = service;
    }
    public void Start()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        Logger.Information("Server listening on port {Port}", _port);
        try
        {
            while (true)
            {
                var client = _listener.AcceptTcpClient();
                Logger.Debug("Accepted client connection");
                var worker = new Worker(_service, client);
                Task.Run(worker.Run);
            }
        }
        finally
        {
            _listener.Stop();
        }
    }
    public void Stop() 
    {
        Logger.Information("Server stopping");
        _listener?.Stop();
    }


}