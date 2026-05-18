namespace Purchases.API.Domain.Entities;

public class PurchaseOrderItem
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}
