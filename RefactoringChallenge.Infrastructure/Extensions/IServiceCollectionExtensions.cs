using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Infrastructure.Repositories;
using System.Data;

namespace RefactoringChallenge.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for registering infrastructure services.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IDbConnection>(sp =>
            {
                return new SqlConnection(connectionString);
            });

            return addPersistence(services);
        }

        private static IServiceCollection addPersistence(IServiceCollection services)
        {
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderLogRepository, OrderLogRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }
    }
}
