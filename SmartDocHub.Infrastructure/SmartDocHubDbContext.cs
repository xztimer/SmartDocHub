using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Domain.Doc;
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

    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermissionMapping> RolePermissions { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<UserRoleMapping> UserRoleMappings { get; set; }
    public DbSet<SysLog> SysLog { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }

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

    }
}
