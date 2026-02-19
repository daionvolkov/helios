using Helios.App.Auth;
using Helios.App.Hosting;
using Helios.Application.Abstractions;
using Helios.Application.DependencyInjection;
using Helios.Application.Identity;
using Helios.Application.Tenants;
using Helios.Core.Auth;
using Helios.Identity.Options;
using Helios.Identity.Services;
using Helios.Persistence.Context;
using Helios.Persistence.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace Helios.App.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHeliosApp(this IServiceCollection services, IConfiguration config)
    {
        //Options
        services.Configure<JwtOptions>(config.GetSection("Jwt"));
        services.Configure<SeedOptions>(config.GetSection("Seed"));

        //DbContext
        services.AddDbContext<HeliosDbContext>(opt =>
        {
            var cs = config.GetConnectionString("HeliosDb");
            opt.UseNpgsql(cs);
        });

        //Http / tenant context
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, HttpTenantContext>();

        
        //Identity primitives
        services.AddScoped<PasswordHasher<User>>();

        // Token service + login service
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ILoginService, LoginService>();

        // Application layer (use-cases)
        services.AddHeliosApplication();

        //Authentication/Authorization
        var jwt = config.GetSection("Jwt").Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Jwt config section missing.");

        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");

        services
           .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(o =>
           {
               o.RequireHttpsMetadata = false; // local dev
               o.SaveToken = true;

               o.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidIssuer = jwt.Issuer,

                   ValidateAudience = true,
                   ValidAudience = jwt.Audience,

                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),

                   ValidateLifetime = true,
                   ClockSkew = TimeSpan.FromSeconds(20)
               };
           });

        services.AddAuthorization();

        //Hosted services
        services.AddHostedService<SeedHostedService>();

        return services;
    }


}