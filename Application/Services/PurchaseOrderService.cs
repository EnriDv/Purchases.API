using Purchases.API.Application.DTOs;
using Purchases.API.Application.Interfaces;
using Purchases.API.Domain.Entities;
using Purchases.API.Domain.Enums;
using Shared.Core.Cen;
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
        var companyId = await PurchasesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
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
        var company = await PurchasesCenResolver.ResolveCompanyAsync(_uow, companyCen);
        await PurchasesCenResolver.ValidateWarehouseAsync(_uow, company.Id, request.WarehouseCen);
        await PurchasesCenResolver.ValidateProductsAsync(_uow, company.Id, request.Items.Select(i => i.ProductCen));

        var supplier = await PurchasesCenResolver.ResolveSupplierAsync(_uow, company.Id, request.SupplierCen);
        var warehouseCen = CenParser.ParseRequired(request.WarehouseCen, "bodega");

        var order = new PurchaseOrder
        {
            Cen = Guid.NewGuid(),
            CompanyId = company.Id,
            Code = GenerateOrderCode(),
            SupplierId = supplier.Id,
            WarehouseCen = warehouseCen,
            Status = PurchaseStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new PurchaseOrderItem
            {
                ProductCen = CenParser.ParseRequired(i.ProductCen, "producto"),
                Quantity = i.Quantity
            }).ToList()
        };

        await _uow.PurchaseOrders.AddAsync(order);
        await _uow.SaveAsync();

        return new PurchaseOrderSummaryDto
        {
            OrderCen = CenParser.Format(order.Cen),
            Status = order.Status
        };
    }

    public async Task<PurchaseOrderDetailDto> GetOrderAsync(string companyCen, string orderCen)
    {
        var companyId = await PurchasesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var order = await _uow.PurchaseOrders.GetByCenAsync(companyId, orderCen)
            ?? throw new NotFoundException($"Orden de compra no encontrada: {orderCen}");

        return MapToDetailDto(order);
    }

    public async Task<PurchaseOrderConfirmationDto> ConfirmOrderAsync(string companyCen, string orderCen)
    {
        var company = await PurchasesCenResolver.ResolveCompanyAsync(_uow, companyCen);
        var order = await _uow.PurchaseOrders.GetByCenAsync(company.Id, orderCen)
            ?? throw new NotFoundException($"Orden de compra no encontrada: {orderCen}");

        if (order.Status == PurchaseStatus.Confirmed)
            throw new ConflictException($"La orden {orderCen} ya está confirmada.");

        if (!order.Items.Any())
            throw new DomainException("La orden no tiene items para confirmar.");

        await _inventory.IncreaseStockAsync(
            CenParser.Format(company.Cen),
            CenParser.Format(order.WarehouseCen),
            order.Items.Select(i => (CenParser.Format(i.ProductCen), i.Quantity)),
            $"PO-{order.Code}");

        order.Status = PurchaseStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.PurchaseOrders.Update(order);
        await _uow.SaveAsync();

        return new PurchaseOrderConfirmationDto
        {
            OrderCen = CenParser.Format(order.Cen),
            Status = order.Status,
            ConfirmedAt = order.ConfirmedAt.Value
        };
    }

    private static string GenerateOrderCode() =>
        $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

    private static PurchaseOrderListDto MapToListDto(PurchaseOrder o) => new()
    {
        OrderCen = CenParser.Format(o.Cen),
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        ConfirmedAt = o.ConfirmedAt,
        SupplierCen = o.Supplier != null ? CenParser.Format(o.Supplier.Cen) : string.Empty,
        ItemCount = o.Items?.Count ?? 0
    };

    private static PurchaseOrderDetailDto MapToDetailDto(PurchaseOrder o) => new()
    {
        OrderCen = CenParser.Format(o.Cen),
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        ConfirmedAt = o.ConfirmedAt,
        SupplierCen = o.Supplier != null ? CenParser.Format(o.Supplier.Cen) : string.Empty,
        WarehouseCen = CenParser.Format(o.WarehouseCen),
        Items = o.Items.Select(i => new PurchaseOrderDetailItemDto
        {
            ProductCen = CenParser.Format(i.ProductCen),
            Quantity = i.Quantity
        }).ToList()
    };
}
