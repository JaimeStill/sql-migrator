using App.Schema;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[Route("api/[controller]")]
public class ContactInfoController : EntityController<ContactInfo>
{
    readonly ContactInfoService svc;
    public ContactInfoController(ContactInfoService svc) : base(svc)
    {
        this.svc = svc;
    }

    [HttpGet("[action]/{employeeId:int}")]
    public async Task<IActionResult> GetByEmplyoee([FromRoute]int employeeId) =>
        ApiReturn(await svc.GetByEmployee(employeeId));
}