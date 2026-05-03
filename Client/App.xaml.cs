using System.IO;
using System.Windows;
using Client.GUI;
using Client.Network;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Client;


public partial class App : Application
{
    private static readonly ILogger Logger = Log.ForContext<App>();
    private ServiceProxy _proxy;
    private AppNavigator _navigator;

    protected override void OnStartup(StartupEventArgs e)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();
        Logger.Information("Application starting");
        base.OnStartup(e);
        string host = LoadSetting("server.host", "localhost");
        int    port = int.TryParse(LoadSetting("server.port", "65535"), out int p) ? p : 65535;

        _proxy = new ServiceProxy(host, port);
        try
        {
            _proxy.Connect();
            Logger.Information("Connected to server at {Host}:{Port}", host, port);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to connect to server");
            MessageBox.Show($"Cannot connect to server: {ex.Message}");
            Shutdown();
            return;
        }

        _navigator = new AppNavigator(_proxy);
        _navigator.ShowLogin();
    }

    private static string LoadSetting(string key, string fallback)
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "client.properties");
            var lines = File.ReadAllLines(path);
            var line = lines.FirstOrDefault(l => l.StartsWith(key + "="));
            return line?.Split('=', 2)[1].Trim() ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }
    protected override void OnExit(ExitEventArgs e)
    {
        Logger.Information("Application exiting");
        Log.CloseAndFlush();
        _proxy?.Disconnect();
        base.OnExit(e);
    }


}