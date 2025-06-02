namespace PortfolioWebApp.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    
    Task<IEnumerable<T>> GetAllAsync();
    
    // save an untracked or update a tracked entity
    Task<T> SaveAsync(T entity);
    
    Task Remove(T entity);
}