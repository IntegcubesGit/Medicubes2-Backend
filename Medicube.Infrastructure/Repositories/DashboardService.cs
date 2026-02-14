using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;
        private readonly IJWTService _jwtService;

        public DashboardService(AppDbContext context, IJWTService jWTService)
        {
            _db = context;
            _jwtService = jWTService;
        }

        public async Task<object> GetCustomerGrowth()
        {
            var tempUser = _jwtService.DecodeToken();
            int currentYear = DateTime.Now.Year;

            var dbData = await _db.Customers.Where(c => c.isdeleted == 0 && c.tenantid == tempUser.TenantId && c.createdon.Year == currentYear).GroupBy(c => c.createdon.Month).Select(g => new
                {
                    Month = g.Key,
                    TotalCustomers = g.Count()
                }).ToListAsync();

            var finalData = Enumerable.Range(1, 12).Select(month => new
            {
                Year = currentYear,
                Month = month,
                MonthName = new DateTime(currentYear, month, 1).ToString("MMMM"),
                TotalCustomers = dbData.FirstOrDefault(x => x.Month == month)?.TotalCustomers ?? 0
            }).ToList();

            return finalData;
        }


        public async Task<object> GetProductStatusDistribution()
        {
            var tempUser = _jwtService.DecodeToken();

            var products = await _db.Products.Where(p => p.isdeleted == 0 && p.tenantid == tempUser.TenantId).ToListAsync();

            int activeCount = products.Count(p => p.isactive == 1);

            int inactiveCount = products.Count(p => p.isactive == 0);

            var stockData = await (from p in _db.Products
                                   where p.isdeleted == 0 && p.tenantid == tempUser.TenantId
                                   join inv in _db.InvInventories on p.itemid equals inv.ItemId into invGroup
                                   from ig in invGroup.DefaultIfEmpty()
                                   group ig by new { p.itemid, p.reorderlevel } into g
                                   select new
                                   {
                                       ItemId = g.Key.itemid,
                                       ReorderLevel = g.Key.reorderlevel,
                                       AvailableQty = g.Sum(x => (decimal?)(x.Quantity - x.ConsumedQuantity) ?? 0)
                                   }).ToListAsync();

            var lowStockCount = stockData.Count(x => x.AvailableQty > 0 && x.AvailableQty < x.ReorderLevel);

            return new
            {
                Active = activeCount,
                Inactive = inactiveCount,
                LowStock = lowStockCount
            };
        }

        public async Task<object> GetTotalSaleAndUser()
        {
            var tempUser = _jwtService.DecodeToken();

            // OPTIMIZATION 1: Get invoice IDs with status = 3 (single query)
            var invoiceIds = await _db.Invoices
                .Where(inv =>
                    inv.StatusId == 3 &&
                    inv.IsDeleted == 0 &&
                    inv.TenantId == tempUser.TenantId)
                .Select(inv => inv.InvoiceId)
                .ToListAsync();

            // OPTIMIZATION 2: If no invoices, return 0 immediately
            if (invoiceIds.Count == 0)
            {
                var totalUserr = await _db.Users.CountAsync();
                var productt = await _db.Products
                    .Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId && a.isactive == 1)
                    .CountAsync();
                return new
                {
                    TotalSale = 0m,
                    TotalTax = 0m,
                    TotalUser = totalUserr,
                    TotalProduct = productt
                };
            }

            // OPTIMIZATION 3: Load invoice details
            var invoiceDetailsData = await _db.InvoiceDetails
                .Where(d => invoiceIds.Contains(d.InvoiceId))
                .Select(d => new
                {
                    d.Qty,
                    d.Price,
                    d.Discount,
                    d.SaleTax
                })
                .ToListAsync();

            // Calculate totals in memory with proper rounding
            decimal totalTax = 0m;
            decimal totalSale = 0m;

            foreach (var detail in invoiceDetailsData)
            {
                decimal subTotal = Math.Round(detail.Qty * detail.Price, 2);
                decimal discountAmount = Math.Round(subTotal * detail.Discount / 100m, 2);
                decimal valueExclST = Math.Round(subTotal - discountAmount, 2);
                decimal salesTax = Math.Round((valueExclST * detail.SaleTax / 100m), 2);
                decimal valueIncTax = Math.Round(valueExclST + salesTax, 2);

                totalTax += salesTax;
                totalSale += valueIncTax;
            }

            // OPTIMIZATION 4: ✅ FIXED - Sequential execution instead of parallel
            // (Parallel causes DbContext concurrency issues)
            var totalUser = await _db.Users.CountAsync();

            var product = await _db.Products
                .Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId && a.isactive == 1)
                .CountAsync();

            return new
            {
                TotalSale = totalSale,
                TotalTax = totalTax, 
                TotalUser = totalUser,
                TotalProduct = product
            };
        }

    }
}
