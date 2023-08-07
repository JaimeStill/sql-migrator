using App.Data;
using App.Schema;

namespace App.Services;
public class DepartmentService : EntityService<Department>
{
    public DepartmentService(AppDbContext db) : base(db)
    { }
}