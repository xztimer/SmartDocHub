using Microsoft.AspNetCore.Mvc;

namespace SmartDocHub.Web.Auth;

public class PermissionAuthorizeAttribute : TypeFilterAttribute
{
    public PermissionAuthorizeAttribute(Type type) : base(type)
    {
    }
}
