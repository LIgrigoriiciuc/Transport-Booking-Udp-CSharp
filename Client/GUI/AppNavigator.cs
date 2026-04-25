using System.Windows;
using Client.GUI.Windows;
using Client.Network;
using Shared.Proto;

namespace Client.GUI;

public class AppNavigator : INavigationListener

{
    private readonly ServiceProxy _proxy;
    private Window? _current;
    public AppNavigator(ServiceProxy proxy)
    {
        _proxy = proxy;
    }
    public void ShowLogin()
    {
        SwitchTo(new LoginWindow(_proxy, this));
    }

    public void OnLoginSuccess(ProtoUser user)
    {
        SwitchTo(new MainWindow(_proxy, this, user));
    }

    public void OnLogout()
    {
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