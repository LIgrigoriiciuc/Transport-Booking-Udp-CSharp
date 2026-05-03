using System.Net;
using System.Net.Sockets;

namespace Server.Network;

public class ConcurrentServer
{
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
        try
        {
            while (true)
            {
                var client = _listener.AcceptTcpClient();
                var worker = new Worker(_service, client);
                Task.Run(worker.Run);
            }
        }
        finally
        {
            _listener.Stop();
        }
    }
    public void Stop() => _listener?.Stop();


}