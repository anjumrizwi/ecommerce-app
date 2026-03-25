using System.Linq.Expressions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Repositories;

public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync([id], cancellationToken);

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task<PaginatedList<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await DbSet.AsNoTracking().CountAsync(cancellationToken);
        var items = await DbSet.AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public virtual void Update(T entity)
        => DbSet.Update(entity);

    public virtual void Delete(T entity)
        => DbSet.Remove(entity);
}
