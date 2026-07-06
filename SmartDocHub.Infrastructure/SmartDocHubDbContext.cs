using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Infrastructure;

public class SmartDocHubDbContext : IdentityDbContext<User, Role, long>
{
    public SmartDocHubDbContext(DbContextOptions<SmartDocHubDbContext> options) : base(options)
    {
    }

    protected SmartDocHubDbContext()
    {
    }

    DbSet<Permission> Permissions { get; set; }
    DbSet<RolePermissionMapping> RolePermissions { get; set; }
    DbSet<Department> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasMany<RolePermissionMapping>().WithOne().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasMany<RolePermissionMapping>().WithOne().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermissionMapping>()
            .HasKey(rpm => new { rpm.RoleId, rpm.PermissionId });

        modelBuilder.Entity<UserRoleMapping>()
            .HasKey(t => new { t.UserId, t.RoleId });
    }
}
