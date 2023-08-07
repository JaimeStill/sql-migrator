using App.Schema;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[Route("api/[controller]")]
public class EmployeeController : EntityController<Employee>
{
    readonly EmployeeService svc;
    public EmployeeController(EmployeeService svc) : base(svc)
    {
        this.svc = svc;
    }

    [HttpGet("[action]/{departmentId:int}")]
    public async Task<IActionResult> GetByDepartment([FromRoute]int departmentId) =>
        ApiReturn(await svc.GetByDepartment(departmentId));
}