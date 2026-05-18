using Microsoft.EntityFrameworkCore;
using Purchases.API.Application.Interfaces;
using Purchases.API.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Purchases.API.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly PurchasesDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(PurchasesDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
    {
        IQueryable<T> query = _dbSet;
        if (filter != null) query = query.Where(filter);
        if (includeProperties != null)
        {
            foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                query = query.Include(includeProp);
        }
        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
}
