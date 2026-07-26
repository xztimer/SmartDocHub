using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Domain.UserPermission;

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SmartDocHub.Web.AuditLog;

public class LogHelper
{
    public static SysLog GenerateRequestLog(ActionContext action, UserManager<User> userManager)
    {
        var log = new SysLog();
        var context = action.HttpContext;
        var request = context.Request;
        var userName = context.User.Identity.Name;
        if (!string.IsNullOrEmpty(userName))
        {
            var user = userManager.FindByNameAsync(userName).Result;
            log.UserId = user.Id;
        }
        log.RequestUrl = request.Path;
        log.Method = request.Method;
        log.AuditLogType = AuditLogType.Info;
        log.IP = context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
        var param = "";

        if (request.Method == "Get")
        {
            var json = new JsonObject();
            foreach (var item in request.Query)
            {
                json.Add(item.Key, item.Value.ToString());
            }
            param = JsonSerializer.Serialize(json);
        }
        else
        {
            request.EnableBuffering();

            request.Body.Position = 0;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                param = reader.ReadToEnd();
            }
            request.Body.Position = 0;
        }

        if (param.Length > 0)
        {
            var json = new JsonObject();
            foreach (var item in action.ActionDescriptor.Parameters)
            {
                var value = GetParameterValue(item, action, param);
                json.Add(item.Name, value);
            }
            log.RequestParam = JsonSerializer.Serialize(json);
        }
        return log;
    }
    private static string GetParameterValue(ParameterDescriptor parameterDescriptor, ActionContext action, string parameters)
    {
        var bindingSource = parameterDescriptor.BindingInfo.BindingSource;
        if (bindingSource == BindingSource.Path)
        {
            return action.RouteData.Values.GetValueOrDefault(parameterDescriptor.Name).ToString();
        }
        else
        {
            var parameterObject = JsonSerializer.Deserialize(parameters, parameterDescriptor.ParameterType);
            return JsonSerializer.Serialize(parameterObject);
        }
    }
}
