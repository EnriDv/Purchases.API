using System.ComponentModel.DataAnnotations;

namespace Purchases.API.Application.DTOs;

public class CreatePurchaseOrderDto
{
    [Required] public string SupplierCen { get; set; } = string.Empty;
    [Required] public string WarehouseCen { get; set; } = string.Empty;
    [Required, MinLength(1)] public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemDto
{
    [Required] public string ProductCen { get; set; } = string.Empty;
    [Required, Range(1, int.MaxValue)] public int Quantity { get; set; }
}
