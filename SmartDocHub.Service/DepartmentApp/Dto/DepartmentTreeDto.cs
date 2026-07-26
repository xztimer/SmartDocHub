using SmartDocHub.Domain.UserPermission;

using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocHub.Service.DepartmentApp.Dto;

public class DepartmentTreeDto
{
    public long Id { get; set; }

    public long ParentId { get; set; }

    public string DeptName { get; set; } = string.Empty;

    public string? Code { get; set; }

    public int Sort { get; set; }

    public DepartmentStatus Status { get; set; }

    public List<DepartmentTreeDto> Children { get; set; } = new();
}
