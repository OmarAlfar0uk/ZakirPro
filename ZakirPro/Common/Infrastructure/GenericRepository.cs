using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Data;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Common.Infrastructure;

public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id)
        => await _dbSet.AsNoTracking()
                       .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);

    public async Task<TEntity?> GetByIdIgnoreFiltersAsync(Guid id)
        => await _dbSet.AsNoTracking()
                       .IgnoreQueryFilters()
                       .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);

    public IQueryable<TEntity> Query()
        => _dbSet.AsNoTracking();

    public IQueryable<TEntity> QueryIgnoreFilters()
        => _dbSet.AsNoTracking().IgnoreQueryFilters();

    public async Task AddAsync(TEntity entity)
        => await _dbSet.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        => await _dbSet.AddRangeAsync(entities);

    public void Update(TEntity entity)
    {
        // Stamp UpdatedAt if the entity has that property.
        var prop = typeof(TEntity).GetProperty("UpdatedAt");
        prop?.SetValue(entity, DateTime.UtcNow);
        _dbSet.Update(entity);
    }

    public void SoftDelete(TEntity entity)
    {
        var isDeletedProp = typeof(TEntity).GetProperty("IsDeleted");
        var updatedAtProp = typeof(TEntity).GetProperty("UpdatedAt");

        if (isDeletedProp is null)
            throw new InvalidOperationException($"{typeof(TEntity).Name} does not support soft delete (no IsDeleted property).");

        isDeletedProp.SetValue(entity, true);
        updatedAtProp?.SetValue(entity, DateTime.UtcNow);
        _dbSet.Update(entity);
    }

    public void HardDelete(TEntity entity)
        => _dbSet.Remove(entity);

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
}
