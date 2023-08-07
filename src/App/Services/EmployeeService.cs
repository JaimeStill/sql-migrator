using App.Data;
using App.Schema;
using Microsoft.EntityFrameworkCore;

namespace App.Services;
public class EmployeeService : EntityService<Employee>
{
    public EmployeeService(AppDbContext db) : base(db)
    { }

    public async Task<List<Employee>> GetByDepartment(int departmentId) =>
        await Query
            .Where(x => x.DepartmentId == departmentId)
            .OrderBy(x => x.LastName)
            .ToListAsync();
}