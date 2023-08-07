using App.Data;
using App.Schema;
using Microsoft.EntityFrameworkCore;

namespace App.Services;
public class ContactInfoService : EntityService<ContactInfo>
{
    public ContactInfoService(AppDbContext db) : base(db)
    { }

    public async Task<List<ContactInfo>> GetByEmployee(int employeeId) =>
        await Query
            .Where(x => x.EmployeeId == employeeId)
            .OrderBy(x => x.ContactType)
            .ToListAsync();
}