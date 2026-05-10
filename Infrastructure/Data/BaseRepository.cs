using Domain;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
{
    protected readonly JujuTestContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public BaseRepository(JujuTestContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<List<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();

    public virtual async Task<TEntity> GetAsync(object id) => await _dbSet.FindAsync(id);

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public virtual async Task<List<TEntity>> CreateAllAsync(List<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        return entities;
    }

    public virtual async Task<(TEntity entity, bool changed)> UpdateAsync(TEntity editedEntity, TEntity originalEntity)
    {
        _context.Entry(originalEntity).CurrentValues.SetValues(editedEntity);

        var changed = _context.Entry(originalEntity).State == EntityState.Modified;
        await _context.SaveChangesAsync();

        return (originalEntity, changed);
    }

    public virtual async Task<TEntity> DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public virtual async Task<int> DeleteAllAsync(List<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();

        return entities.Count;
    }

    public virtual async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    protected async Task<(TEntity entity, bool changed)> CreateOrThrowAsync(TEntity entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return (entity, true);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            var name = GetEntityName(entity);
            throw new DuplicateNameException(name);
        }
    }

    protected async Task<List<TEntity>> CreateAllOrThrowAsync(List<TEntity> entities)
    {
        try
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            var name = GetEntityName(entities.First());
            throw new DuplicateNameException(name);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // SQL Server unique constraint violations: error 2601 or 2627
        var message = ex.InnerException?.Message ?? string.Empty;
        return message.Contains("2601") || message.Contains("2627");
    }

    private static string GetEntityName(TEntity entity)
    {
        // Try to get Name property via reflection
        var nameProperty = entity.GetType().GetProperty("Name");
        return nameProperty?.GetValue(entity)?.ToString() ?? "unknown";
    }
}
