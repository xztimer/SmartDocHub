using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service;
using SmartDocHub.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new AutofacModule());
});

builder.Services.AddControllers(opt =>
{
    //opt.Filters.Add<ExceptionFilter>();
});
builder.Services.AddMemoryCache();

builder.AddDocDbContext();

builder.Services.AddIdentity<User, Role>().AddEntityFrameworkStores<SmartDocHubDbContext>().AddDefaultTokenProviders();


builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
        [new OpenApiSecuritySchemeReference("X-API-Key", document)] = []
    });
});
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<DocHubProfile>();
});
builder.AddJwt();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
