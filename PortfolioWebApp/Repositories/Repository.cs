using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;

namespace PortfolioWebApp.Repositories;

/// <summary>
/// Concrete implementation of <see cref="IRepository{T}"/> using Entity Framework Core.
/// Provides data access logic for entities, including navigation property handling and change tracking.
/// </summary>
/// <typeparam name="T">The entity type, must inherit from <see cref="EntityBase"/>.</typeparam>
public class Repository<T> : IRepository<T> where T : EntityBase
{
    private readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T}"/> class with the specified database context.
    /// </summary>
    /// <param name="context">The application's DbContext.</param>
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Loads an entity by its ID, including all navigation properties.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public async Task<T?> GetByIdAsync(int id)
    {
        var query = IncludeNavigationProperties(_dbSet);
        var entityType = _context.Model.FindEntityType(typeof(T));
        var keyProperty = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();

        if (keyProperty == null)
            throw new InvalidOperationException("Primary key could not be determined.");

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// Retrieves all entities of the specified type, including navigation properties.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await IncludeNavigationProperties(_dbSet).ToListAsync();
    }

    /// <summary>
    /// Adds or updates a single entity depending on its tracking state and primary key.
    /// </summary>
    /// <param name="entity">The entity to persist.</param>
    /// <returns>The saved entity.</returns>
    public async Task<T> SaveAsync(T entity)
    {
        var result = await SaveAllAsync([entity]);
        return result.First();
    }

    /// <summary>
    /// Adds or updates a collection of entities based on tracking state and key presence.
    /// </summary>
    /// <param name="entities">The entities to persist.</param>
    /// <returns>The saved entities.</returns>
    public async Task<IEnumerable<T>> SaveAllAsync(IEnumerable<T> entities)
    {
        var saveAllAsync = entities.ToList();

        foreach (var entity in saveAllAsync)
        {
            var entry = _context.Entry(entity);

            if (entry.State != EntityState.Detached)
            {
                if (IsKeyDefault(entity))
                {
                    throw new InvalidOperationException(
                        "You cannot save an already tracked entity with an uninitialized primary key.");
                }

                entry.State = EntityState.Modified;
            }
            else
            {
                if (IsKeyDefault(entity))
                {
                    await _dbSet.AddAsync(entity);
                }
                else
                {
                    _dbSet.Attach(entity);
                    entry.State = EntityState.Modified;
                }
            }
        }

        await SaveChangesAsync();
        return saveAllAsync;
    }

    /// <summary>
    /// Removes the given entity and saves the changes to the database.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public async Task Remove(T entity)
    {
        _dbSet.Remove(entity);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Commits all pending changes in the current DbContext.
    /// </summary>
    protected async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Dynamically includes all navigation properties for the given entity type.
    /// </summary>
    /// <param name="dbSet">The DbSet to query.</param>
    /// <returns>A queryable including all navigation properties.</returns>
    protected IQueryable<T> IncludeNavigationProperties(DbSet<T> dbSet)
    {
        var query = dbSet.AsQueryable();
        var navigations = _context.Model.FindEntityType(typeof(T))?.GetNavigations();

        if (navigations != null)
        {
            foreach (var navigation in navigations)
            {
                query = query.Include(navigation.Name);
            }
        }

        return query;
    }

    /// <summary>
    /// Checks whether the entity's primary key is uninitialized (default value).
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <returns><c>true</c> if the key is default (0); otherwise, <c>false</c>.</returns>
    private static bool IsKeyDefault(T entity)
    {
        return entity.Id == 0;
    }
}
