using Purchases.API.Application.DTOs;
using Purchases.API.Application.Interfaces;
using Purchases.API.Domain.Entities;
using Purchases.API.Domain.Enums;
using Shared.Core.Exceptions;

namespace Purchases.API.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IInventoryIntegrationService _inventory;

    public PurchaseOrderService(IUnitOfWork uow, IInventoryIntegrationService inventory)
    {
        _uow = uow;
        _inventory = inventory;
    }

    public async Task<PagedResultDto<PurchaseOrderListDto>> GetOrdersAsync(
        string companyCen,
        PurchaseStatus? status,
        int page,
        int pageSize,
        bool sortDescending)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (orders, totalCount) = await _uow.PurchaseOrders.GetPagedAsync(
            companyId, status, page, pageSize, sortDescending);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResultDto<PurchaseOrderListDto>
        {
            Items = orders.Select(MapToListDto).ToList(),
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page
        };
    }

    public async Task<PurchaseOrderSummaryDto> CreateOrderAsync(string companyCen, CreatePurchaseOrderDto request)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        await ValidateWarehouseAsync(companyId, request.WarehouseCen);
        await ValidateProductsAsync(companyId, request.Items.Select(i => i.ProductCen));

        var supplier = await ResolveSupplierAsync(companyId, request.SupplierCen);

        var order = new PurchaseOrder
        {
            CompanyId = companyId,
            Code = GenerateOrderCode(),
            SupplierId = supplier.Id,
            WarehouseCode = request.WarehouseCen,
            Status = PurchaseStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new PurchaseOrderItem
            {
                ProductCode = i.ProductCen,
                Quantity = i.Quantity
            }).ToList()
        };

        await _uow.PurchaseOrders.AddAsync(order);
        await _uow.SaveAsync();

        return new PurchaseOrderSummaryDto
        {
            OrderCen = order.Code,
            Status = order.Status
        };
    }

    public async Task<PurchaseOrderDetailDto> GetOrderAsync(string companyCen, string orderCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var order = await _uow.PurchaseOrders.GetByCenAsync(companyId, orderCen)
            ?? throw new NotFoundException($"Orden de compra no encontrada: {orderCen}");

        return MapToDetailDto(order);
    }

    public async Task<PurchaseOrderConfirmationDto> ConfirmOrderAsync(string companyCen, string orderCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var order = await _uow.PurchaseOrders.GetByCenAsync(companyId, orderCen)
            ?? throw new NotFoundException($"Orden de compra no encontrada: {orderCen}");

        if (order.Status == PurchaseStatus.Confirmed)
            throw new ConflictException($"La orden {orderCen} ya está confirmada.");

        if (!order.Items.Any())
            throw new DomainException("La orden no tiene items para confirmar.");

        await _inventory.IncreaseStockAsync(
            companyCen,
            order.WarehouseCode,
            order.Items.Select(i => (i.ProductCode, i.Quantity)),
            $"PO-{order.Code}");

        order.Status = PurchaseStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.PurchaseOrders.Update(order);
        await _uow.SaveAsync();

        return new PurchaseOrderConfirmationDto
        {
            OrderCen = order.Code,
            Status = order.Status,
            ConfirmedAt = order.ConfirmedAt.Value
        };
    }

    private async Task<int> ResolveCompanyIdAsync(string companyCen)
    {
        if (!int.TryParse(companyCen, out var id))
            throw new ValidationException($"CEN de empresa inválido: {companyCen}");

        var company = await _uow.Companies.GetByIdAsync(id);
        if (company == null || !company.Active)
            throw new NotFoundException($"Empresa no encontrada: {companyCen}");

        return id;
    }

    private async Task<Supplier> ResolveSupplierAsync(int companyId, string supplierCen)
    {
        var suppliers = await _uow.Suppliers.GetAllAsync(
            s => s.CompanyId == companyId && s.Code == supplierCen && s.Active);
        return suppliers.FirstOrDefault()
            ?? throw new NotFoundException($"Proveedor no encontrado: {supplierCen}");
    }

    private async Task ValidateWarehouseAsync(int companyId, string warehouseCen)
    {
        var warehouses = await _uow.Warehouses.GetAllAsync(
            w => w.CompanyId == companyId && w.Code == warehouseCen && w.Active);
        if (!warehouses.Any())
            throw new NotFoundException($"Bodega no encontrada: {warehouseCen}");
    }

    private async Task ValidateProductsAsync(int companyId, IEnumerable<string> productCens)
    {
        foreach (var productCen in productCens.Distinct())
        {
            var products = await _uow.Products.GetAllAsync(
                p => p.CompanyId == companyId && p.Code == productCen && p.Active);
            if (!products.Any())
                throw new NotFoundException($"Producto no encontrado: {productCen}");
        }
    }

    private static string GenerateOrderCode() =>
        $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

    private static PurchaseOrderListDto MapToListDto(PurchaseOrder o) => new()
    {
        OrderCen = o.Code,
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        ConfirmedAt = o.ConfirmedAt,
        SupplierCen = o.Supplier?.Code ?? string.Empty,
        ItemCount = o.Items?.Count ?? 0
    };

    private static PurchaseOrderDetailDto MapToDetailDto(PurchaseOrder o) => new()
    {
        OrderCen = o.Code,
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        ConfirmedAt = o.ConfirmedAt,
        SupplierCen = o.Supplier?.Code ?? string.Empty,
        WarehouseCen = o.WarehouseCode,
        Items = o.Items.Select(i => new PurchaseOrderDetailItemDto
        {
            ProductCen = i.ProductCode,
            Quantity = i.Quantity
        }).ToList()
    };
}
