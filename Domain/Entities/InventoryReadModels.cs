namespace Purchases.API.Domain.Entities;

/// <summary>Entidades de solo lectura del esquema inventory.</summary>
public class Company
{
    public Guid Cen { get; set; }
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
}

public class Warehouse
{
    public Guid Cen { get; set; }
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
}

public class Product
{
    public Guid Cen { get; set; }
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
}
