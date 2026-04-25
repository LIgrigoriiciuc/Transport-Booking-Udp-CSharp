namespace Server.Domain;

public class Trip : Entity<long>
{
    public string Destination { get; set; }
    public DateTime Time { get; set; }
    public string BusNumber { get; set; }

    public Trip(long id, string destination, DateTime time, string busNumber) : base(id)
    {
        Destination = destination;
        Time = time;
        BusNumber = busNumber;
    }
    
}