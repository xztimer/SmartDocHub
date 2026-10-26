using Microsoft.EntityFrameworkCore;

using SmartDocHub.Infrastructure;

namespace SmartDocHub.Web.Extensions;

public static class DbContextExtension
{
    public static void AddDocDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<SmartDocHubDbContext>(opt =>
        {
            opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
        });
    }
}
