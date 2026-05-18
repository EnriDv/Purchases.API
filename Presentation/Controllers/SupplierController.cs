using Microsoft.AspNetCore.Mvc;
using Purchases.API.Application.Interfaces;

namespace Purchases.API.Presentation.Controllers;

/// <summary>
/// Supplier — Contrato v1
/// Base: /api/purchases/companies/{companyCen}/suppliers
/// </summary>
[ApiController]
[Route("api/purchases/companies/{companyCen}/suppliers")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _suppliers;

    public SupplierController(ISupplierService suppliers) => _suppliers = suppliers;

    [HttpGet]
    public async Task<IActionResult> GetSuppliers(string companyCen) =>
        Ok(await _suppliers.GetSuppliersAsync(companyCen));
}
