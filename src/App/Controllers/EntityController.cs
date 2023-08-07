using App.Schema;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;
public abstract class EntityController<T> : ApiController
where T : Entity
{
    protected readonly IEntityService<T> entitySvc;

    public EntityController(IEntityService<T> svc)
    {
        entitySvc = svc;
    }

    [HttpGet("[action]")]
    public virtual async Task<IActionResult> Get() =>
        ApiReturn(await entitySvc.Get());

    [HttpGet("[action]/{id:int}")]
    public virtual async Task<IActionResult> GetById([FromRoute]int id) =>
        ApiReturn(await entitySvc.GetById(id));
        
    [HttpPost("[action]")]
    public virtual async Task<IActionResult> Validate([FromBody]T entity) =>
        ApiReturn(await entity.Validate());

    [HttpPost("[action]")]
    public virtual async Task<IActionResult> Save([FromBody]T entity) =>
        ApiReturn(await entitySvc.Save(entity));

    [HttpDelete("[action]")]
    public virtual async Task<IActionResult> Remove([FromBody]T entity) =>
        ApiReturn(await entitySvc.Remove(entity));
}