using Autofac;
using Autofac.Extensions.DependencyInjection;

using SmartDocHub.Service;
using SmartDocHub.Web.Exception;
using SmartDocHub.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new AutofacModule());
});

builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<ExceptionFilter>();
});


builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<DocHubProfile>();
});
builder.AddJwt();
builder.AddDocDbContext();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
