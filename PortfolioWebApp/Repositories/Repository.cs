using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using System.Reflection;
using PortfolioWebApp.Models.Entities;

namespace PortfolioWebApp.Repositories;

public class Repository<T> : IRepository<T> where T : EntityBase {

    private readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context) {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) {
        var query = IncludeNavigationProperties(_dbSet);
        var entityType = _context.Model.FindEntityType(typeof(T));
        var keyProperty = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();
        if (keyProperty == null)
            throw new InvalidOperationException("Primary key could not be determined.");

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync() {
        return await IncludeNavigationProperties(_dbSet).ToListAsync();
    }

    public async Task<T> SaveAsync(T entity) {
        var entry = _context.Entry(entity);

        if (entry.State != EntityState.Detached) {
            if (IsKeyDefault(entity)) {
                throw new InvalidOperationException(
                    "You cannot save an already tracked entity with an uninitialized primary key.");
            }
            entry.State = EntityState.Modified;
        }
        else {
            if (IsKeyDefault(entity)) {
                await _dbSet.AddAsync(entity);
            }
            else {
                _dbSet.Attach(entity);
                entry.State = EntityState.Modified;
            }
        }

        await SaveChangesAsync();
        return entity;
    }

    public async Task Remove(T entity) {
        _dbSet.Remove(entity);
        await SaveChangesAsync();
    }

    protected async Task SaveChangesAsync() {
        await _context.SaveChangesAsync();
    }
    
    protected IQueryable<T> IncludeNavigationProperties(DbSet<T> dbSet) {
        var query = dbSet.AsQueryable();
        var navigations = _context.Model.FindEntityType(typeof(T))?.GetNavigations();

        if (navigations != null) {
            foreach (var navigation in navigations) {
                query = query.Include(navigation.Name);
            }
        }

        return query;
    }
    
    private static bool IsKeyDefault(T entity) {
        return entity.Id == 0;
    }
}
