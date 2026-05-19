namespace Purchases.API.Domain.Entities;

public class PurchaseOrderItem
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public Guid ProductCen { get; set; }
    public int Quantity { get; set; }

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}
