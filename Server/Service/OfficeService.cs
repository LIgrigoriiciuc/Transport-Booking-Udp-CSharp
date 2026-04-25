using Server.Domain;
using Server.Repository;

namespace Server.Service;

public class OfficeService : AbstractService<long, Office>
{
    public OfficeService(OfficeRepository repository) : base(repository)
    {
    }
}