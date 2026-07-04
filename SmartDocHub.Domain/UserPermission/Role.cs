namespace SmartDocHub.Domain.UserPermission;

public class Role
{
    public long Id { get; set; }
    public string RoleName { get; set; }
    public string Code { get; set; }

    public DataScope DataScope { get; set; }
    public string Remark { get; set; }

    public RoleStatus Status { get; set; }
}


public enum RoleStatus
{
    Forbidden = 0,
    Normal = 1
}

public enum DataScope
{
    全部数据 = 1,
    本部门 = 2,
    本部门及子部门 = 3,
    自定义部门 = 4,
    本人 = 5
}