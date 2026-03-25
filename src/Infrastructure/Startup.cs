using ABCShared.Library.Constants;
using ABCShared.Library.Wrappers;
using Application;
using Application.Features.Identity.Roles;
using Application.Features.Identity.Tokens;
using Application.Features.Identity.Users;
using Application.Features.Schools;
using Application.Features.Tenancy;
using Application.Mapping;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Infrastructure.Contexts;
using Infrastructure.Identity;
using Infrastructure.Identity.Auth;
using Infrastructure.Identity.Models;
using Infrastructure.Identity.Tokens;
using Infrastructure.OpenApi;
using Infrastructure.Schools;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddDbContext<TenantDbContext>(options => options
                .UseSqlServer(config.GetConnectionString("DefaultConnection")))
            .AddMultiTenant<ABCSchoolTenantInfo>()
                .WithHeaderStrategy(TenancyConstants.TenantIdName)
                .WithClaimStrategy(TenancyConstants.TenantIdName)
                .WithEFCoreStore<TenantDbContext, ABCSchoolTenantInfo>()
                .Services
            .AddDbContext<ApplicationDbContext>(options => options
                .UseSqlServer(config.GetConnectionString("DefaultConnection")))
            .AddTransient<ITenantDbSeeder, TenantDbSeeder>()
            .AddTransient<ApplicationDbSeeder>()
            .AddScoped<ITenantService, TenantService>()
            .AddScoped<ISchoolService, SchoolService>()
            .AddIdentityService()
            .AddPermissions()
            .AddJwtAuthentication(services.AddJwtSettings(config)!)
            .AddOpenApiDocumentation(config)
            .AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(MappingProfiles).Assembly);
            });
    }

    public static async Task AddDatabaseInitialiserAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ITenantDbSeeder>().InitialiseDatabaseAsync(cancellationToken);
    }

    internal static IServiceCollection AddIdentityService(this IServiceCollection services)
    {
        return services
            .AddIdentity<ApplicationUser, ApplicationRole>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .Services
            .AddScoped<ITokenService, TokenService>()
            .AddScoped<IRoleService, RoleService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<ICurrentUserService, CurrentUserService>()
            .AddScoped<CurrentUserMiddleware>();
    }

    internal static IServiceCollection AddPermissions(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }

    public static JwtSettings? AddJwtSettings(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettingsConfig = config.GetSection(nameof(JwtSettings));
        services.Configure<JwtSettings>(jwtSettingsConfig);
        return jwtSettingsConfig.Get<JwtSettings>();
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
    {
        var secret = Encoding.ASCII.GetBytes(jwtSettings.Secret);
        services.AddAuthentication(auth =>
        {
            auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        })
        .AddJwtBearer(bearer =>
        {
            bearer.RequireHttpsMetadata = false;
            bearer.SaveToken = true;
            bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
            bearer.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/type";
                            var result = JsonSerializer.Serialize(ResponseWrapper.Fail("Token has expired"));
                            return context.Response.WriteAsync(result);

                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "application/type";
                            var result = JsonSerializer.Serialize(ResponseWrapper.Fail("An unhandled error has occured"));
                            return context.Response.WriteAsync(result);

                        }
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/type";
                        var result = JsonSerializer.Serialize(ResponseWrapper.Fail("You are not authorised"));
                        return context.Response.WriteAsync(result);
                    }
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/type";
                    var result = JsonSerializer.Serialize(ResponseWrapper.Fail("You are not authorised to access this resource"));
                    return context.Response.WriteAsync(result);
                }
            };
        });

        services.AddAuthorization(options =>
        {
            foreach (var property in typeof(SchoolPermissions).GetNestedTypes()
             .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
            {
                var propertyValue = property.GetValue(null);
                if (propertyValue is not null)
                {
                    options.AddPolicy(propertyValue.ToString()!, policy =>
                    {
                        policy.RequireClaim(ClaimConstants.Permission, propertyValue.ToString()!);
                    });
                }
            }
        });

        return services;
    }

    internal static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration config)
    {
        var swaggerSettings = config.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();
        services
            .AddEndpointsApiExplorer();

        _ = services.AddOpenApiDocument((document, serviceProvider) =>
        {
            document.PostProcess = doc =>
            {
                doc.Info.Title = swaggerSettings?.Title;
                doc.Info.Description = swaggerSettings?.Description;
                doc.Info.Contact = new OpenApiContact
                {
                    Name = swaggerSettings?.ContactName,
                    Email = swaggerSettings?.ContactEmail,
                    Url = swaggerSettings?.ContactUrl
                };
                doc.Info.License = new OpenApiLicense
                {
                    Name = swaggerSettings?.LicenseName,
                    Url = swaggerSettings?.LicenseUrl
                };
            };
            document.AddSecurity(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter your Bearer token to attach it as a header on your requests",
                In = OpenApiSecurityApiKeyLocation.Header,
                Type = OpenApiSecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            });
            document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor());
            document.OperationProcessors.Add(new SwaggerGlobalAuthProcessor());
            document.OperationProcessors.Add(new SwaggerHeaderAttributeProcessor());
        });

        return services;
    }

    public static IApplicationBuilder UseInfrastructureServices(this IApplicationBuilder app)
    {
        return app
            .UseAuthentication()
            .UseMiddleware<CurrentUserMiddleware>()
            .UseMultiTenant()
            .UseAuthorization()
            .UseOpenApiDocumentation();
    }

    internal static IApplicationBuilder UseOpenApiDocumentation(this IApplicationBuilder app)
    {
        app.UseOpenApi();
        app.UseSwaggerUi(options =>
        {
            options.DefaultModelExpandDepth = -1;
            options.DocExpansion = "none";
            options.TagsSorter = "alpha";
        });

        return app;
    }
}
