namespace GymFlow.Dal.Repositories.Implementations;

/// <summary>
/// Generic repository implementation with soft delete support and automatic audit timestamp management.
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
/// <remarks>
/// This class provides a concrete implementation of <see cref="IRepository{T}"/> using
/// Entity Framework Core. It uses <see cref="IDbContextFactory{TContext}"/> to create
/// short-lived DbContext instances for each operation, ensuring proper resource management.
/// </remarks>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T}"/> class.
    /// </summary>
    /// <param name="dbContextFactory">Factory for creating DbContext instances.</param>
    public Repository(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Creates a new DbContext instance for database operations.
    /// </summary>
    /// <returns>A new AppDbContext instance.</returns>
    protected async Task<AppDbContext> CreateContextAsync()
    {
        return await _dbContextFactory.CreateDbContextAsync();
    }

    // ========== Query Methods ==========

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        await using var context = await CreateContextAsync();
        return await context.Set<T>().FindAsync(id);
    }

    /// <inheritdoc />
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        await using var context = await CreateContextAsync();
        return await context.Set<T>().FirstOrDefaultAsync(predicate);
    }

    /// <inheritdoc />
    public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        await using var context = await CreateContextAsync();
        return await context.Set<T>().SingleOrDefaultAsync(predicate);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        await using var context = await CreateContextAsync();
        return await context.Set<T>().ToListAsync();
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        await using var context = await CreateContextAsync();
        return await context.Set<T>().Where(predicate).ToListAsync();
    }

    public virtual async Task<T?> FindAsync(int id)
    {
        await using var context = await CreateContextAsync();
        var entity =  await context.Set<T>().FindAsync(id);
        if (entity is null) return null;
        return entity.IsDeleted == false ? entity : null;
    }
    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        await using var context = await CreateContextAsync();
        return await context.Set<T>().AnyAsync(predicate);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AllAsync(Expression<Func<T, bool>> predicate)
    {
        await using var context = await CreateContextAsync();
        return await context.Set<T>().AllAsync(predicate);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        await using var context = await CreateContextAsync();
        return predicate is null
            ? await context.Set<T>().CountAsync()
            : await context.Set<T>().CountAsync(predicate);
    }

    // ========== Command Methods ==========

    /// <inheritdoc />
    public virtual async Task<T> AddAsync(T entity)
    {
        await using var context = await CreateContextAsync();
        await context.Set<T>().AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<T> UpdateAsync(T entity)
    {
        await using var context = await CreateContextAsync();
        entity.UpdatedAt = DateTime.UtcNow;
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(T entity)
    {
        await using var context = await CreateContextAsync();
        
        if (!context.Set<T>().Contains(entity))
            return false;

        var e = await context.Set<T>().FindAsync(entity.Id);
        
        if (e is null) return false;
        e.IsDeleted = true;
        e.DeletedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        // context.Set<T>().Remove(entity);
        return true;
    }

    /// <inheritdoc />
    public virtual async Task<bool> DeleteByIdAsync(int id)
    {
        await using var context = await CreateContextAsync();
        var entity = await context.Set<T>().FindAsync(id);
        if (entity is null) return false;
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        // context.Set<T>().Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    /// <inheritdoc />
    /// all delete methods should soft delete 
    [Obsolete("Use DeleteByIdAsync() instead")]
    public virtual async Task<bool> SoftDeleteAsync(int id)
    {
        await using var context = await CreateContextAsync();
        var entity = await context.Set<T>().FindAsync(id);
        if (entity is null) return false;
        
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        
        return await context.SaveChangesAsync() > 0;
    }

    public virtual async Task<bool> DeleteAllAsync()
    {
        await using var context = await CreateContextAsync();
        
        if (!await context.Set<T>().AnyAsync())
            return false;

        await context.Set<T>().ForEachAsync(x =>
        {
            x.IsDeleted = true;
            x.DeletedAt = DateTime.UtcNow;
        });
        await context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        await using var context = await CreateContextAsync();
        await context.Set<T>().AddRangeAsync(entities);
        await context.SaveChangesAsync();
        return entities;
    }

    /// <inheritdoc />
    public virtual async Task<bool> DeleteRangeAsync(IEnumerable<T> entities)
    {
        await using var context = await CreateContextAsync();
        foreach (var x in context.Set<T>())
        {
            x.IsDeleted = true;
            x.DeletedAt = DateTime.UtcNow;
        }
        // context.Set<T>().RemoveRange(entities);
        return await context.SaveChangesAsync() > 0;
    }

    /// <inheritdoc />
    public virtual async Task<int> SaveChangesAsync()
    {
        await using var context = await CreateContextAsync();
        return await context.SaveChangesAsync();
    }
}