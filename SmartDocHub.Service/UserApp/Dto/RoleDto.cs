namespace SmartDocHub.Service.UserApp.Dto;

public class RoleDto
{
    public long Id { get; set; }
    public string RoleName { get; set; }
    public string Code { get; set; }
    public DataScope DataScope { get; set; }
    public string Remark { get; set; }
    public RoleStatus Status { get; set; }
    public List<long> PermissionIds { get; set; } = new();
}

public class RoleCreateDto
{
    public string RoleName { get; set; }
    public string Code { get; set; }
    public DataScope DataScope { get; set; }
    public string Remark { get; set; }
}

public class RoleUpdateDto
{
    public string RoleName { get; set; }
    public string Code { get; set; }
    public DataScope DataScope { get; set; }
    public string Remark { get; set; }
}

public class AssignRolePermissionDto
{
    public long RoleId { get; set; }
    public List<long> PermissionIds { get; set; } = new();
}

public class RoleQueryDto
{
    public string RoleName { get; set; }
    public RoleStatus? Status { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
