using Server.Domain;
using Server.Repository;

namespace Server.Service;


public class TripService : AbstractService<long, Trip>
{
    public TripService(TripRepository repository) : base(repository)
    {
    }

    public List<Trip> Search(string destination, DateTime? from, DateTime? to)
    {
        var f = new Filter();

        if (!string.IsNullOrWhiteSpace(destination))
        {
            f.AddLikeFilter("destination", destination);
        }

        if (from.HasValue && to.HasValue)
        {
            f.AddRangeFilter("time", 
                from.Value.ToString("yyyy-MM-ddTHH:mm:ss"), 
                to.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
        }

        return f.IsEmpty ? Repository.GetAll() : Repository.Filter(f);
    }
}