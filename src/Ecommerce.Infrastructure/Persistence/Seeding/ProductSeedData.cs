using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Persistence.Seeding;

public static class ProductSeedData
{
    public static async Task SeedAsync(AppDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Product seed skipped because products already exist.");
            return;
        }

        var categoryDefinitions = new (string Name, string Description)[]
        {
            ("Electronics", "Devices, peripherals, and personal gadgets."),
            ("Home", "Home essentials and appliances."),
            ("Fitness", "Wearables and equipment for active lifestyles."),
            ("Travel", "Bags and accessories for commuting and travel.")
        };

        var existingCategories = await dbContext.Categories
            .ToDictionaryAsync(category => category.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var categoryDefinition in categoryDefinitions)
        {
            if (existingCategories.ContainsKey(categoryDefinition.Name))
            {
                continue;
            }

            var category = Category.Create(categoryDefinition.Name, categoryDefinition.Description);
            dbContext.Categories.Add(category);
            existingCategories[categoryDefinition.Name] = category;
        }

        var products = new List<Product>
        {
            Product.Create("Wireless Noise-Cancelling Headphones", "Over-ear Bluetooth headphones with active noise cancellation and 30-hour battery life.", 199.99m, 120),
            Product.Create("Mechanical Gaming Keyboard", "Compact RGB mechanical keyboard with hot-swappable switches and programmable macros.", 129.00m, 85),
            Product.Create("4K UHD Monitor 27-inch", "IPS panel 4K monitor with HDR support, adjustable stand, and USB-C connectivity.", 329.50m, 42),
            Product.Create("Ergonomic Office Chair", "Breathable mesh office chair with lumbar support, 4D armrests, and tilt lock.", 289.00m, 35),
            Product.Create("Smart Fitness Watch", "Water-resistant smartwatch with heart-rate tracking, GPS, and sleep analytics.", 149.95m, 90),
            Product.Create("Portable SSD 1TB", "High-speed external SSD with USB 3.2 Gen 2 for fast backups and transfers.", 99.00m, 200),
            Product.Create("Action Camera 5K", "Rugged action camera with 5K recording, image stabilization, and voice control.", 249.99m, 55),
            Product.Create("Air Purifier for Home", "True HEPA air purifier suitable for medium rooms with auto mode and quiet sleep mode.", 179.00m, 60),
            Product.Create("Coffee Grinder Burr Mill", "Adjustable burr grinder with 18 grind settings for espresso to French press.", 79.50m, 70),
            Product.Create("Electric Toothbrush Pro", "Rechargeable toothbrush with pressure sensor, timer, and multiple brushing modes.", 59.99m, 140),
            Product.Create("Travel Backpack 35L", "Water-resistant carry-on backpack with laptop compartment and anti-theft pocket.", 68.75m, 110),
            Product.Create("Bluetooth Speaker Mini", "Portable speaker with deep bass, 12-hour playtime, and IPX7 waterproof rating.", 44.99m, 180)
        };

        products[0].SetCategory(existingCategories["Electronics"].Id);
        products[1].SetCategory(existingCategories["Electronics"].Id);
        products[2].SetCategory(existingCategories["Electronics"].Id);
        products[3].SetCategory(existingCategories["Home"].Id);
        products[4].SetCategory(existingCategories["Fitness"].Id);
        products[5].SetCategory(existingCategories["Electronics"].Id);
        products[6].SetCategory(existingCategories["Electronics"].Id);
        products[7].SetCategory(existingCategories["Home"].Id);
        products[8].SetCategory(existingCategories["Home"].Id);
        products[9].SetCategory(existingCategories["Home"].Id);
        products[10].SetCategory(existingCategories["Travel"].Id);
        products[11].SetCategory(existingCategories["Electronics"].Id);

        await dbContext.Products.AddRangeAsync(products, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {ProductCount} products across {CategoryCount} categories.", products.Count, existingCategories.Count);
    }
}
