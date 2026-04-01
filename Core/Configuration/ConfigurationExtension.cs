using System;
using Core.Interfaces;
using Core.Services;
using Data.Models;
using Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Configuration;

public static class ConfigurationExtension
{
    public static void RegisterServices(this IServiceCollection services)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        // Register Repositories
        services.AddScoped<IRepository<Car>, Repository<Car>>();
        services.AddScoped<IRepository<User>, Repository<User>>();
        services.AddScoped<IRepository<Request>, Repository<Request>>();

        // Register Services
        services.AddScoped<ICarService, CarService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRequestService, RequestService>();
    }
}
