namespace Server.Repository;

public interface IRepository<TId, TE> where TE : Entity<TId>
{
    void Add(TE e);
    
    bool Remove(TId id);
    
    TE? FindById(TId id);
    
    List<TE> GetAll();
    
    bool Update(TE e);
    
    List<TE> Filter(Filter f);
}