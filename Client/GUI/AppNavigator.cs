using System.Windows;
using Client.GUI.Windows;
using Client.Network;
using Shared.Proto;
using Serilog;

namespace Client.GUI;

public class AppNavigator : INavigationListener

{
    private static readonly ILogger Logger = Log.ForContext<AppNavigator>();
    private readonly ServiceProxy _proxy;
    private Window? _current;
    public AppNavigator(ServiceProxy proxy)
    {
        _proxy = proxy;
    }
    public void ShowLogin()
    {
        Logger.Debug("Showing login window");
        SwitchTo(new LoginWindow(_proxy, this));
    }

    public void OnLoginSuccess(ProtoUser user)
    {
        Logger.Information("User {UserId} logged in successfully", user.Id);
        SwitchTo(new MainWindow(_proxy, this, user));
    }

    public void OnLogout()
    {
        Logger.Information("User logged out");
        ShowLogin();
    }

    private void SwitchTo(Window next)
    {
        var old = _current;
        _current = next;
        next.Show();
        old?.Close();
    }

}