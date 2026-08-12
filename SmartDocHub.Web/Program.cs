using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service;
using SmartDocHub.Web.AuditLog;
using SmartDocHub.Web.Auth;
using SmartDocHub.Web.Converter;
using SmartDocHub.Web.Exceptions;
using SmartDocHub.Web.Extensions;

using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new AutofacModule());
});

builder.Logging.AddLog4Net();
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<GlobalExceptionFilter>();
    opt.Filters.Add<AuditLogFilter>();
}).AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
});

builder.AddDocDbContext();

builder.Services.AddMemoryCache();

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<SmartDocHubDbContext>()
    .AddDefaultTokenProviders();

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
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, true);
});
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<DocHubProfile>();
});
builder.AddJwt();
builder.Services.AddCors(c =>
    c.AddPolicy("timer",
    a => a.WithOrigins("http://localhost:5173")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials())
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("timer");
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers(); app.Use(next => context =>
{
    context.Request.EnableBuffering();
    return next(context);
});

app.Run();
