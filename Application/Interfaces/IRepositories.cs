using Purchases.API.Domain.Entities;
using Purchases.API.Domain.Enums;
using System.Linq.Expressions;

namespace Purchases.API.Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
}

public interface IPurchaseOrderRepository
{
    Task AddAsync(PurchaseOrder order);
    Task<PurchaseOrder?> GetByCenAsync(int companyId, string orderCen);
    Task<(List<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(
        int companyId,
        PurchaseStatus? status,
        int page,
        int pageSize,
        bool sortDescending);
    void Update(PurchaseOrder order);
}

public interface IUnitOfWork
{
    IGenericRepository<Supplier> Suppliers { get; }
    IGenericRepository<Company> Companies { get; }
    IGenericRepository<Warehouse> Warehouses { get; }
    IGenericRepository<Product> Products { get; }
    IPurchaseOrderRepository PurchaseOrders { get; }

    Task SaveAsync();
}
