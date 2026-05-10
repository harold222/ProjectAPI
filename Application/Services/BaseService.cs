using Domain;

namespace Application.Services;

public class BaseService<TEntity> where TEntity : class, new()
{
    protected readonly IBaseRepository<TEntity> _repository;

    public BaseService(IBaseRepository<TEntity> repository)
    {
        _repository = repository;
    }

    public virtual Task<TEntity> GetAsync(object id) => _repository.GetAsync(id);

    public virtual Task<List<TEntity>> GetAllAsync() => _repository.GetAllAsync();

    public virtual Task<TEntity> CreateAsync(TEntity entity) => _repository.CreateAsync(entity);

    public virtual Task<List<TEntity>> CreateAllAsync(List<TEntity> entities) => _repository.CreateAllAsync(entities);

    public virtual async Task<(TEntity entity, bool changed)> UpdateAsync(object id, TEntity editedEntity)
    {
        var originalEntity = await _repository.GetAsync(id);

        if (originalEntity == null)
            return (null, false);

        return await _repository.UpdateAsync(editedEntity, originalEntity);
    }

    public virtual Task<TEntity> DeleteAsync(TEntity entity) => _repository.DeleteAsync(entity);

    public virtual Task<int> DeleteAllAsync(List<TEntity> entities) => _repository.DeleteAllAsync(entities);

    public virtual Task SaveChangesAsync() => _repository.SaveChangesAsync();
}
