using App.Models;
using App.Schema;

namespace App.Services;
public interface IEntityService<T>
where T : Entity
{
    Task<List<T>> Get();
    Task<T?> GetById(int id);
    Task<ApiResult<T>> Save(T entity);
    Task<ApiResult<int>> Remove(T entity);
}