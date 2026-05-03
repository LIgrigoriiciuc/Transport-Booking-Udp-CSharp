using System.Windows;
using System.Windows.Input;
using Client.Network;
using Serilog;

namespace Client.GUI.Windows;

public partial class LoginWindow: Window

{
    private static readonly ILogger Logger = Log.ForContext<LoginWindow>();
    private readonly ServiceProxy _proxy;
    private readonly INavigationListener _nav;

    public LoginWindow(ServiceProxy proxy, INavigationListener nav)
    {
        _proxy = proxy;
        _nav   = nav;
        InitializeComponent();
        UsernameField.KeyDown += (_, e) => { if (e.Key == Key.Enter) HandleLogin(); };
        PasswordField.KeyDown += (_, e) => { if (e.Key == Key.Enter) HandleLogin(); };
    }
    private void LoginButton_Click(object sender, RoutedEventArgs e) => HandleLogin();

    private void HandleLogin()
    {
        try
        {
            ErrorLabel.Content = "";
            Logger.Debug("Attempting login for user {Username}", UsernameField.Text.Trim());
            var user = _proxy.Login(UsernameField.Text.Trim(), PasswordField.Password);
            Logger.Information("Login successful for user {UserId}", user.Id);
            _nav.OnLoginSuccess(user);
        }
        catch (Exception ex)
        {
            Logger.Warning("Login failed for user {Username}: {Message}", UsernameField.Text.Trim(), ex.Message);
            ErrorLabel.Content  = ex.Message;
            PasswordField.Clear();
        }
    }



}