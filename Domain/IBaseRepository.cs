namespace Domain;

public interface IBaseRepository<TEntity> where TEntity : class, new()
{
    Task<TEntity> GetAsync(object id);
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity> CreateAsync(TEntity entity);
    Task<List<TEntity>> CreateAllAsync(List<TEntity> entities);
    Task<(TEntity entity, bool changed)> UpdateAsync(TEntity editedEntity, TEntity originalEntity);
    Task<TEntity> DeleteAsync(TEntity entity);
    Task<int> DeleteAllAsync(List<TEntity> entities);
    Task SaveChangesAsync();
}
