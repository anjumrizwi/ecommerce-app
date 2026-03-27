using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Repositories;

public class CartRepository(AppDbContext context) : Repository<Cart>(context), ICartRepository
{
    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await Context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public async Task AddItemAtomicAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        await Context.Database.ExecuteSqlInterpolatedAsync($@"
SET XACT_ABORT ON;
DECLARE @CartId uniqueidentifier;

SELECT @CartId = [Id]
FROM [Carts] WITH (UPDLOCK, HOLDLOCK)
WHERE [UserId] = {userId};

IF @CartId IS NULL
BEGIN
    SET @CartId = NEWID();

    INSERT INTO [Carts] ([Id], [UserId], [CreatedAt], [UpdatedAt])
    VALUES (@CartId, {userId}, SYSUTCDATETIME(), NULL);
END;

UPDATE [CartItems]
SET [Quantity] = [Quantity] + {quantity},
    [UpdatedAt] = SYSUTCDATETIME()
WHERE [CartId] = @CartId
  AND [ProductId] = {productId};

IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO [CartItems] ([Id], [CartId], [ProductId], [Quantity], [CreatedAt], [UpdatedAt])
    VALUES (NEWID(), @CartId, {productId}, {quantity}, SYSUTCDATETIME(), NULL);
END;
", cancellationToken);
    }
}
