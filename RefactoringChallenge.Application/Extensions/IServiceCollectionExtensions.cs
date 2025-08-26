using Microsoft.Extensions.DependencyInjection;
using RefactoringChallenge.Application.Implementations.Service;
using RefactoringChallenge.Application.Implementations.Services;
using RefactoringChallenge.Application.Interfaces.Services;
using RefactoringChallenge.Domain.Services;
using System.Reflection;

namespace RefactoringChallenge.Application.Extensions
{
    /// <summary>
    /// Adds application services to the service collection.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddScoped<IOrderLogService, OrderLogService>();
            services.AddSingleton<IDiscountStrategy, DefaultDiscountStrategy>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IInventoryService, InventoryService>();
            return services;
        }
    }
}
