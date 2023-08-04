using Microsoft.EntityFrameworkCore;

namespace Core.Data;
public class ContextBuilder<T> where T : DbContext
{
    private ContextBuilder() { }

    public static T Build(string connectionString) =>
        GetDbContext(
            connectionString
        );

    static T GetDbContext(string connectionString) =>
        GetDbContext(connectionString, () =>
            new DbContextOptionsBuilder<T>()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        );

    static T GetDbContext(string connectionString, Func<DbContextOptionsBuilder<T>> config)
    {
        DbContextOptionsBuilder<T> builder = config()
            .UseSqlServer(connectionString);

        return Activator.CreateInstance(typeof(T), builder.Options) as T
            ?? throw new Exception("Failed to initialize an instance of DbContext");
    }
}