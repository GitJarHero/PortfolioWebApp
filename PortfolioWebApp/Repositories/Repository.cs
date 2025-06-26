using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace PortfolioWebApp.Repositories;

/// <summary>
/// A generic repository implementation using Entity Framework Core that
/// dynamically detects the primary key of the entity type at runtime.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TId">The type of the primary key.</typeparam>
public class Repository<T, TId> : IRepository<T, TId> where T : class
{
    private readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;
    private readonly IProperty _primaryKeyProperty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T, ID}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public Repository(DbContext context) {
        _context = context;
        _dbSet = _context.Set<T>();

        var primaryKey = _context.Model
            .FindEntityType(typeof(T))?
            .FindPrimaryKey();

        if (primaryKey == null || primaryKey.Properties.Count != 1)
            throw new InvalidOperationException(
                $"Entity {typeof(T).Name} needs to have exactly 1 primary key defined.");

        _primaryKeyProperty = primaryKey.Properties[0];
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(TId id)
    {
        var predicate = BuildKeyPredicate(id);
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetAllByIdAsync(IEnumerable<TId> ids)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, _primaryKeyProperty.Name);

        var containsMethod = typeof(Enumerable)
            .GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TId));

        var idsConst = Expression.Constant(ids, typeof(IEnumerable<TId>));
        var containsCall = Expression.Call(containsMethod, idsConst, property);

        var lambda = Expression.Lambda<Func<T, bool>>(containsCall, parameter);
        return await IncludeNavigationProperties(_dbSet).Where(lambda).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<T> SaveAsync(T entity)
    {
        if (IsKeyDefault(entity))
        {
            await _dbSet.AddAsync(entity);
        }
        else
        {
            _dbSet.Update(entity);
        }

        await _context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> SaveAllAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            if (IsKeyDefault(entity))
            {
                await _dbSet.AddAsync(entity);
            }
            else
            {
                _dbSet.Update(entity);
            }
        }

        await _context.SaveChangesAsync();
        return entities;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Builds a lambda expression for comparing an entity's primary key to a given ID.
    /// Equivalent to: e => e.PrimaryKey == id.
    /// </summary>
    private Expression<Func<T, bool>> BuildKeyPredicate(TId id)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, _primaryKeyProperty.Name);
        var constant = Expression.Constant(id, typeof(TId));
        var equal = Expression.Equal(property, constant);

        return Expression.Lambda<Func<T, bool>>(equal, parameter);
    }

    /// <summary>
    /// Determines whether the entity's primary key has the default value.
    /// </summary>
    private bool IsKeyDefault(T entity)
    {
        var value = _primaryKeyProperty.PropertyInfo?.GetValue(entity);

        if (value == null)
            return true;

        var defaultValue = typeof(TId).IsValueType ? Activator.CreateInstance(typeof(TId)) : null;
        return value.Equals(defaultValue);
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

}
