using Domain;
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
}
