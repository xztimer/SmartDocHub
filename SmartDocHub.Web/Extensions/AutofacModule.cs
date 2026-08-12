using Autofac;

using Microsoft.AspNetCore.Authorization;

using SmartDocHub.Service.Common;
using SmartDocHub.Web.AuditLog;
using SmartDocHub.Web.Auth;

namespace SmartDocHub.Web.Extensions;

public class AutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var serviceAssembly = typeof(IBaseService).Assembly;
        builder.RegisterAssemblyTypes(serviceAssembly)
               .Where(t => t.Name.EndsWith("Service"))
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope();
        builder.RegisterType<AuditLogQueue>().SingleInstance();
        builder.RegisterType<AuditLogConsumerService>().As<IHostedService>().SingleInstance();
        builder.RegisterType<AuditLogFilter>().InstancePerLifetimeScope();

        builder.RegisterType<DynamicPolicyProvider>()
        .As<IAuthorizationPolicyProvider>()
        .SingleInstance();
        builder.RegisterType<RbacAuthorizationHandler>()
            .As<IAuthorizationHandler>()
            .InstancePerLifetimeScope();
    }
}
