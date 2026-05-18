using Microsoft.EntityFrameworkCore;
using Purchases.API.Application.Interfaces;
using Purchases.API.Domain.Entities;
using Purchases.API.Domain.Enums;
using Purchases.API.Infrastructure.Persistence;

namespace Purchases.API.Infrastructure.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly PurchasesDbContext _ctx;

    public PurchaseOrderRepository(PurchasesDbContext ctx) => _ctx = ctx;

    public async Task AddAsync(PurchaseOrder order) => await _ctx.PurchaseOrders.AddAsync(order);

    public void Update(PurchaseOrder order) => _ctx.PurchaseOrders.Update(order);

    public async Task<PurchaseOrder?> GetByCenAsync(int companyId, string orderCen)
    {
        return await _ctx.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.CompanyId == companyId && o.Code == orderCen);
    }

    public async Task<(List<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(
        int companyId,
        PurchaseStatus? status,
        int page,
        int pageSize,
        bool sortDescending)
    {
        var query = _ctx.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Items)
            .Where(o => o.CompanyId == companyId);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var totalCount = await query.CountAsync();

        query = sortDescending
            ? query.OrderByDescending(o => o.CreatedAt)
            : query.OrderBy(o => o.CreatedAt);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly PurchasesDbContext _context;

    public IGenericRepository<Supplier> Suppliers { get; }
    public IGenericRepository<Company> Companies { get; }
    public IGenericRepository<Warehouse> Warehouses { get; }
    public IGenericRepository<Product> Products { get; }
    public IPurchaseOrderRepository PurchaseOrders { get; }

    public UnitOfWork(PurchasesDbContext context)
    {
        _context = context;
        Suppliers = new GenericRepository<Supplier>(context);
        Companies = new GenericRepository<Company>(context);
        Warehouses = new GenericRepository<Warehouse>(context);
        Products = new GenericRepository<Product>(context);
        PurchaseOrders = new PurchaseOrderRepository(context);
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
