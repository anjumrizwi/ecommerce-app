using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Services.Carts;
using Ecommerce.Application.Services.Orders;
using Ecommerce.Application.Services.Products;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // In-memory cache for frequently accessed data (e.g. product listings)
            services.AddMemoryCache();

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICartService, CartService>();

        return services;
    }
}
