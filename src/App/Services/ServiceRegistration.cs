namespace App.Services;
public static class ServiceRegistration
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<ContactInfoService>();
        services.AddScoped<DepartmentService>();
        services.AddScoped<EmployeeService>();
    }
}