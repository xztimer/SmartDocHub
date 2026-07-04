using Autofac;

using SmartDocHub.Service;

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
    }
}
