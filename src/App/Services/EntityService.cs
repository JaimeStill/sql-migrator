using App.Data;
using App.Models;
using App.Schema;
using Microsoft.EntityFrameworkCore;

namespace App.Services;
public abstract class EntityService<T> : IEntityService<T>
where T : Entity
{
    protected AppDbContext db;
    protected IQueryable<T> Query => db.Set<T>();

    protected virtual Func<T, Task<T>>? OnAdd { get; set; }
    protected virtual Func<T, Task<T>>? OnUpdate { get; set; }
    protected virtual Func<T, Task<T>>? OnSave { get; set; }
    protected virtual Func<T, Task<T>>? OnRemove { get; set; }

    protected virtual Func<T, Task>? AfterAdd { get; set; }
    protected virtual Func<T, Task>? AfterUpdate { get; set; }
    protected virtual Func<T, Task>? AfterSave { get; set; }
    protected virtual Func<T, Task>? AfterRemove { get; set; }

    public EntityService(AppDbContext db)
    {
        this.db = db;
    }

    #region Internal

    protected async Task<ApiResult<T>> Add(T entity)
    {
        try
        {
            if (OnAdd is not null)
                entity = await OnAdd(entity);

            await db.Set<T>().AddAsync(entity);
            await db.SaveChangesAsync();

            if (AfterAdd is not null)
                await AfterAdd(entity);

            return new(entity, $"{typeof(T)} successfully added");
        }
        catch (Exception ex)
        {
            return new("Add", ex);
        }
    }

    protected async Task<ApiResult<T>> Update(T entity)
    {
        try
        {
            if (OnUpdate is not null)
                entity = await OnUpdate(entity);

            db.Set<T>().Update(entity);
            await db.SaveChangesAsync();

            if (AfterUpdate is not null)
                await AfterUpdate(entity);

            return new(entity, $"{typeof(T)} successfully updated");
        }
        catch (Exception ex)
        {
            return new("Update", ex);
        }
    }

    #endregion

    #region Public

    public virtual async Task<List<T>> Get() =>
        await Query.ToListAsync();

    public virtual async Task<T?> GetById(int id) =>
        await Query.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<ApiResult<T>> Save(T entity)
    {
        ValidationResult validity = await entity.Validate();

        if (validity.IsValid)
        {
            if (OnSave is not null)
                entity = await OnSave(entity);
                
            ApiResult<T> result = entity.Id > 0
                ? await Update(entity)
                : await Add(entity);

            if (AfterSave is not null)
                await AfterSave(entity);

            return result;
        }
        else
            return new(validity);
    }

    public async Task<ApiResult<int>> Remove(T entity)
    {
        try
        {
            if (OnRemove is not null)
                entity = await OnRemove(entity);

            db.Set<T>().Remove(entity);

            int result = await db.SaveChangesAsync();

            if (result > 0)
            {
                if (AfterRemove is not null)
                    await AfterRemove(entity);

                return new(entity.Id, $"{typeof(T)} successfully removed");
            }
            else
                return new("Remove", new Exception("The operation was not successful"));
        }
        catch (Exception ex)
        {
            return new("Remove", ex);
        }
    }

    #endregion
}