using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDocHub.Domain.UserPermission;

public class RolePermissionMapping
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
}
