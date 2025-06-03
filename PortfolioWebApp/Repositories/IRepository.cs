namespace PortfolioWebApp.Repositories;

/// <summary>
/// Defines a generic repository contract for basic data operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    Task<T?> GetByIdAsync(int id);
    
    /// <summary>
    /// Retrieves all entities of type T.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Adds or updates a single entity.
    /// </summary>
    Task<T> SaveAsync(T entity);
    
    /// <summary>
    /// Adds or updates multiple entities.
    /// </summary>
    Task<IEnumerable<T>> SaveAllAsync(IEnumerable<T> entities);
    
    /// <summary>
    /// Deletes the given entity.
    /// </summary>
    Task Remove(T entity);
}