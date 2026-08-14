using System.Linq.Expressions;

namespace ZakirPro.Common.Abstractions;

public interface IGenericRepository<TEntity> where TEntity : class
{
    /// <summary>Returns entity by ID. Respects the global soft-delete query filter by default.</summary>
    Task<TEntity?> GetByIdAsync(Guid id);

    /// <summary>Returns entity by ID, bypassing the soft-delete filter (includes deleted records).</summary>
    Task<TEntity?> GetByIdIgnoreFiltersAsync(Guid id);

    /// <summary>Returns a tracking-disabled, filter-applied queryable for custom projections.</summary>
    IQueryable<TEntity> Query();

    /// <summary>Returns a tracking-disabled queryable that bypasses global query filters.</summary>
    IQueryable<TEntity> QueryIgnoreFilters();

    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    void Update(TEntity entity);

    /// <summary>Soft delete: sets IsDeleted = true, UpdatedAt = UtcNow.</summary>
    void SoftDelete(TEntity entity);

    /// <summary>Hard delete: permanently removes the row.</summary>
    void HardDelete(TEntity entity);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
}
