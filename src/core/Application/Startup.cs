using System.Reflection;
using Application.Mapping;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class Startup
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        return services
            .AddValidatorsFromAssembly(assembly)
            .AddMediatR(cfg=>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            })
            .AddAutoMapper(cfg=>
            {
                cfg.AddMaps(typeof(MappingProfiles).Assembly);
            });
    }
}

