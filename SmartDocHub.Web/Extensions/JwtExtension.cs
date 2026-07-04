using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using System.Text;

namespace SmartDocHub.Web.Extensions;

public static class JwtExtension
{
    public static void AddJwt(this WebApplicationBuilder builder)
    {
        var jwtBearer = builder.Configuration.GetSection("Authentication").GetSection("JwtBearer");
        builder.Services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,//是否验证Issuer
                ValidIssuer = jwtBearer.GetValue<string>("Issuer"),

                ValidateAudience = true,//是否验证Audience
                ValidAudience = jwtBearer.GetValue<string>("Audience"),

                ValidateIssuerSigningKey = true,//是否验证SecurityKey
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtBearer.GetValue<string>("SecurityKey"))),

                ValidateLifetime = true,//是否验证失效时间
                ClockSkew = TimeSpan.FromSeconds(5)
            };
        });
    }
}
