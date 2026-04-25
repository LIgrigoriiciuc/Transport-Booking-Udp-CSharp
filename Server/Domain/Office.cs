namespace Server.Domain;

public class Office : Entity<long>
{
    public string Address { get; set; }
    public Office(long id, string address) : base(id)
    {
        Address = address;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null || GetType() != obj.GetType()) return false;
        if (!base.Equals(obj)) return false;
        var office = (Office)obj;
        return Address == office.Address;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Address);
    }
}