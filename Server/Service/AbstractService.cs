using Server.Domain;
using Server.Repository;

namespace Server.Service;


public abstract class AbstractService<TId, TE> : IService<TId, TE> where TE : Entity<TId>
{
    protected readonly GenericRepository<TId, TE> Repository;

    protected AbstractService(GenericRepository<TId, TE> repository)
    {
        Repository = repository;
    }

    public virtual void Add(TE entity)
    {
        Repository.Add(entity);
    }

    public virtual bool Remove(TId id)
    {
        return Repository.Remove(id);
    }

    public virtual TE? FindById(TId id)
    {
        return Repository.FindById(id);
    }

    public virtual List<TE> GetAll()
    {
        return Repository.GetAll();
    }

    public virtual bool Update(TE entity)
    {
        return Repository.Update(entity);
    }

    public virtual List<TE> Filter(Filter filter)
    {
        return Repository.Filter(filter);
    }
}