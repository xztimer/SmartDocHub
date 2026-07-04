namespace SmartDocHub.Domain.UserPermission;

public class Department
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string DeptName { get; set; }
    public string DeptCode { get; set; }
    public string Leader { get; set; }
    public string PhoneNumber { get; set; }
    public int Sort { get; set; }
    public DepartmentStatus Status { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime? UpdateTime { get; set; }

}

public enum DepartmentStatus
{
    Forbidden = 0,
    Normal = 1
}
