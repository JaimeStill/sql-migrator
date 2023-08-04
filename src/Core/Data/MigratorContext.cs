using Microsoft.EntityFrameworkCore;

namespace Core.Data;
public class MigratorContext : DbContext
{
    public MigratorContext(DbContextOptions<MigratorContext> options) : base(options) { }    
    public DbSet<MigrationLog> MigrationLogs => Set<MigrationLog>();
}