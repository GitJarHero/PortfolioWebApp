namespace PortfolioWebApp.Repositories;

/// <summary>
/// Defines a generic repository contract for basic data operations,
/// supporting arbitrary entity types and primary key types.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="ID">The type of the primary key.</typeparam>
public interface IRepository<T, ID> where T : class
{
    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    Task<T?> GetByIdAsync(ID id);

    /// <summary>
    /// Retrieves entities by a collection of primary keys.
    /// </summary>
    /// <param name="ids">A collection of primary key values.</param>
    Task<IEnumerable<T>> GetAllByIdAsync(IEnumerable<ID> ids);

    /// <summary>
    /// Retrieves all entities of the specified type.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Adds or updates a single entity.
    /// </summary>
    /// <param name="entity">The entity to persist.</param>
    Task<T> SaveAsync(T entity);

    /// <summary>
    /// Adds or updates multiple entities.
    /// </summary>
    /// <param name="entities">The collection of entities to persist.</param>
    Task<IEnumerable<T>> SaveAllAsync(IEnumerable<T> entities);

    /// <summary>
    /// Removes the given entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    Task RemoveAsync(T entity);
}