using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Client.Network;
using Shared;
using Shared.Proto;
using Serilog;

namespace Client.GUI.Windows;

public partial class MainWindow : Window, IObserver
{
    private static readonly ILogger Logger = Log.ForContext<MainWindow>();
    private readonly ServiceProxy _proxy;
    private readonly INavigationListener _nav;
    private readonly ProtoUser _currentUser;
    private ProtoTrip? _selectedTrip;
    private List<ProtoSeat> _selectedSeats = new();
    private bool _refreshing;

    private static readonly SolidColorBrush BrushFree     = new(Color.FromRgb(224, 224, 224));
    private static readonly SolidColorBrush BrushSelected = new(Color.FromRgb(185,  185,  185));
    private static readonly SolidColorBrush BrushReserved = new(Color.FromRgb(145, 145, 145));

    public MainWindow(ServiceProxy proxy, INavigationListener nav, ProtoUser user)
    {
        _proxy = proxy;
        _nav = nav;
        _currentUser = user;
        InitializeComponent();
        _proxy.Observer = this;
        Title = $"Reservations | Office: {user.OfficeAddress} | Worker: {user.FullName}";
        RefreshTrips();
        RefreshReservations();
    }

    public void OnPushReceived(PushPayload push)
    {
        Logger.Debug("Push received for trip {TripId}", push.UpdatedTripId);
        Dispatcher.Invoke(() =>
        {
            RefreshReservations(push.Reservations);
            if (_selectedTrip != null && _selectedTrip.Id == push.UpdatedTripId)
                RefreshSeats();
        });
    }

    private void TripTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshing) return;
        if (TripTable.SelectedItem is ProtoTrip trip)
        {
            _selectedTrip = trip;
            DrawSeats(trip.Id, new HashSet<long>());
        }
    }

    private void DrawSeats(long tripId, HashSet<long> keepSelectedIds)
    {
        SeatGrid.Children.Clear();
        SeatGrid.RowDefinitions.Clear();
        _selectedSeats.Clear();

        var seats = _proxy.GetSeats(tripId).Seats.OrderBy(s => s.Number).ToList();

        int cols = 3;
        int rows = (int)Math.Ceiling(seats.Count / (double)cols);
        for (int r = 0; r < rows; r++)
            SeatGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });

        int freeCount = 0;

        for (int i = 0; i < seats.Count; i++)
        {
            var seat = seats[i];
            var btn = new Button
            {
                Content = seat.Number.ToString(),
                Margin  = new Thickness(4)
            };

            if (seat.Reserved)
            {
                btn.IsEnabled  = false;
                btn.Background = BrushReserved;
                btn.Foreground = Brushes.White;
                btn.ToolTip    = $"Reservation #{seat.ReservationId}";
            }
            else
            {
                freeCount++;
                if (keepSelectedIds.Contains(seat.Id))
                {
                    _selectedSeats.Add(seat);
                    btn.Background = BrushSelected;
                    btn.Foreground = Brushes.White;
                }
                else
                {
                    btn.Background = BrushFree;
                    btn.Foreground = Brushes.Black;
                }
                var captured = seat;
                btn.Click += (_, _) => ToggleSeat(btn, captured);
            }

            Grid.SetRow(btn, i / cols);
            Grid.SetColumn(btn, i % cols);
            SeatGrid.Children.Add(btn);
        }

        if (_selectedTrip != null)
        {
            _selectedTrip.FreeSeats = freeCount;
            Dispatcher.BeginInvoke(() =>
            {
                _refreshing = true;
                TripTable.Items.Refresh();
                _refreshing = false;
            });
        }
    }

    private void ToggleSeat(Button btn, ProtoSeat seat)
    {
        if (_selectedSeats.Contains(seat))
        {
            _selectedSeats.Remove(seat);
            btn.Background = BrushFree;
            btn.Foreground = Brushes.Black;
        }
        else
        {
            _selectedSeats.Add(seat);
            btn.Background = BrushSelected;
            btn.Foreground = Brushes.White;
        }
    }

    private void RefreshSeats()
    {
        if (_selectedTrip == null) return;
        var keep = _selectedSeats.Select(s => s.Id).ToHashSet();
        DrawSeats(_selectedTrip.Id, keep);
    }

    private void ReserveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTrip == null)           { ShowError("Select a trip first.");      return; }
        if (!_selectedSeats.Any())           { ShowError("Select at least one seat."); return; }
        string name = ClientNameField.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) { ShowError("Enter client name.");        return; }
        try
        {
            Logger.Debug("Making reservation for {ClientName} on trip {TripId} with {SeatCount} seats", name, _selectedTrip.Id, _selectedSeats.Count);
            _proxy.MakeReservation(name, _selectedSeats.Select(s => s.Id).ToList(), _currentUser.Id);
            Logger.Information("Reservation made for {ClientName} by user {UserId}", name, _currentUser.Id);
            ClientNameField.Clear();
            DrawSeats(_selectedTrip.Id, new HashSet<long>()); // clear selection, push redraws again
        }
        catch (Exception ex)
        {
            Logger.Warning("Reservation failed: {Message}", ex.Message);
            var keep = _selectedSeats.Select(s => s.Id).ToHashSet();
            DrawSeats(_selectedTrip.Id, keep);
            ShowError(ex.Message);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        if (ResList.SelectedItem is not ProtoReservation selected)
        { ShowError("Select a reservation to cancel."); return; }
        try   
        { 
            Logger.Debug("Cancelling reservation {ReservationId}", selected.Id);
            _proxy.CancelReservation(selected.Id); 
            Logger.Information("Reservation {ReservationId} cancelled", selected.Id);
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            TripTable.ItemsSource = _proxy.SearchTrips(
                DestFilter.Text, StartTimeField.Text, EndTimeField.Text).Trips;
        }
        catch { ShowError("Dates must be: yyyy-MM-dd HH:mm"); }
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        DestFilter.Clear();
        StartTimeField.Clear();
        EndTimeField.Clear();
        RefreshTrips();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        Logger.Information("User {UserId} logging out", _currentUser.Id);
        _proxy.Observer = null;
        _proxy.Logout(_currentUser.Id);
        _nav.OnLogout();
    }


    private void RefreshTrips() 
    {
        Logger.Debug("Refreshing trip list");
        TripTable.ItemsSource = _proxy.SearchTrips("", "", "").Trips;
    }

    private void RefreshReservations() 
    {
        Logger.Debug("Refreshing reservation list");
        ResList.ItemsSource = _proxy.GetAllReservations().Reservations;
    }

    private void RefreshReservations(ReservationList list) =>
        ResList.ItemsSource = list.Reservations;

    private void ShowError(string msg) =>
        MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
}