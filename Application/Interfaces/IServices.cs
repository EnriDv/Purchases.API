using Purchases.API.Application.DTOs;
using Purchases.API.Domain.Enums;

namespace Purchases.API.Application.Interfaces;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetSuppliersAsync(string companyCen);
}

public interface IPurchaseOrderService
{
    Task<PagedResultDto<PurchaseOrderListDto>> GetOrdersAsync(
        string companyCen,
        PurchaseStatus? status,
        int page,
        int pageSize,
        bool sortDescending);

    Task<PurchaseOrderSummaryDto> CreateOrderAsync(string companyCen, CreatePurchaseOrderDto request);
    Task<PurchaseOrderDetailDto> GetOrderAsync(string companyCen, string orderCen);
    Task<PurchaseOrderConfirmationDto> ConfirmOrderAsync(string companyCen, string orderCen);
}

public interface IInventoryIntegrationService
{
    Task IncreaseStockAsync(string companyCen, string warehouseCen, IEnumerable<(string ProductCen, int Quantity)> items, string reference);
}
