using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Infrastructure;

public class SmartDocHubDbContext : DbContext
{
    public SmartDocHubDbContext(DbContextOptions options) : base(options)
    {
    }

    protected SmartDocHubDbContext()
    {
    }

    DbSet<User> Users { get; set; }
    DbSet<Role> Roles { get; set; }
    DbSet<UserRoleMapping> UserRoles { get; set; }
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
    }
}
