using App.Schema;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[Route("api/[controller]")]
public class DepartmentController : EntityController<Department>
{
    public DepartmentController(DepartmentService svc) : base(svc)
    { }
}