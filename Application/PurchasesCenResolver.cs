using Purchases.API.Application.Interfaces;
using Purchases.API.Domain.Entities;
using Shared.Core.Cen;
using Shared.Core.Exceptions;

namespace Purchases.API.Application;

public static class PurchasesCenResolver
{
    public static async Task<Company> ResolveCompanyAsync(IUnitOfWork uow, string companyCen)
    {
        var cen = CenParser.ParseRequired(companyCen, "empresa");
        var company = (await uow.Companies.GetAllAsync(c => c.Cen == cen)).FirstOrDefault();
        if (company == null || !company.Active)
            throw new NotFoundException($"Empresa no encontrada: {companyCen}");
        return company;
    }

    public static async Task<int> ResolveCompanyIdAsync(IUnitOfWork uow, string companyCen) =>
        (await ResolveCompanyAsync(uow, companyCen)).Id;

    public static async Task<Supplier> ResolveSupplierAsync(IUnitOfWork uow, int companyId, string supplierCen)
    {
        var cen = CenParser.ParseRequired(supplierCen, "proveedor");
        var supplier = (await uow.Suppliers.GetAllAsync(s => s.CompanyId == companyId && s.Cen == cen && s.Active))
            .FirstOrDefault();
        return supplier ?? throw new NotFoundException($"Proveedor no encontrado: {supplierCen}");
    }

    public static async Task ValidateWarehouseAsync(IUnitOfWork uow, int companyId, string warehouseCen)
    {
        var cen = CenParser.ParseRequired(warehouseCen, "bodega");
        var warehouse = (await uow.Warehouses.GetAllAsync(w => w.CompanyId == companyId && w.Cen == cen))
            .FirstOrDefault();
        if (warehouse == null || !warehouse.Active)
            throw new NotFoundException($"Bodega no encontrada: {warehouseCen}");
    }

    public static async Task ValidateProductsAsync(IUnitOfWork uow, int companyId, IEnumerable<string> productCens)
    {
        foreach (var productCen in productCens.Distinct())
        {
            var cen = CenParser.ParseRequired(productCen, "producto");
            var product = (await uow.Products.GetAllAsync(p => p.CompanyId == companyId && p.Cen == cen))
                .FirstOrDefault();
            if (product == null || !product.Active)
                throw new NotFoundException($"Producto no encontrado: {productCen}");
        }
    }
}
