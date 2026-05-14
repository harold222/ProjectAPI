using Shared.Contracts;

namespace Shared.Services;

/// <summary>
/// Generic CRUD service — thin wrapper over IBaseRepository.
/// AppServices in each microservice extend this to add domain-specific logic.
/// </summary>
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

    public virtual Task<(TEntity entity, bool changed)> CreateOrThrowAsync(TEntity entity) => _repository.CreateOrThrowAsync(entity);

    public virtual Task<List<TEntity>> CreateAllOrThrowAsync(List<TEntity> entities) => _repository.CreateAllOrThrowAsync(entities);

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
