using Server.Domain;
using Server.Repository;

namespace Server.Service;

public interface IService<TId, TE> where TE : Entity<TId>
{
    void Add(TE entity);
    
    bool Remove(TId id);
    
    TE? FindById(TId id);
    
    List<TE> GetAll();
    
    bool Update(TE entity);
    
    List<TE> Filter(Filter filter);
}