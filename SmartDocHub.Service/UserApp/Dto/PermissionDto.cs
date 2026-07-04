namespace SmartDocHub.Service.UserApp.Dto;

public class PermissionDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public PermissionType Type { get; set; }
}

public class PermissionCreateDto
{
    public string Name { get; set; }
    public PermissionType Type { get; set; }
}

public class PermissionUpdateDto
{
    public string Name { get; set; }
    public PermissionType Type { get; set; }
}

public class PermissionQueryDto
{
    public string Name { get; set; }
    public PermissionType? Type { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
