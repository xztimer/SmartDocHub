namespace SmartDocHub.Domain.UserPermission;

public class Department
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string DeptName { get; set; }
    public string? Code { get; set; }
    public int Sort { get; set; }
    public string Description { get; set; } = string.Empty;
    public DepartmentStatus Status { get; set; }
    public bool IsDeleted { get; set; } = false;

    public Department? Parent { get; set; }
    public ICollection<Department> Children { get; set; } = new List<Department>();
}

public enum DepartmentStatus
{
    Forbidden = 0,
    Normal = 1
}
