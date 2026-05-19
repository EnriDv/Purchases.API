using Purchases.API.Domain.Enums;

namespace Purchases.API.Domain.Entities;

public class PurchaseOrder
{
    public Guid Cen { get; set; }
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public Guid WarehouseCen { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Supplier Supplier { get; set; } = null!;
    public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
