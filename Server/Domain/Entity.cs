namespace Server.Domain;

public abstract class Entity<TId>
{
    public TId Id { get; set; }

    protected Entity(TId id)
    {
        Id = id;
    }
    protected Entity()
    {
        Id = default;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null || GetType() != obj.GetType()) return false;

        var entity = (Entity<TId>)obj;
        return EqualityComparer<TId>.Default.Equals(Id, entity.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}