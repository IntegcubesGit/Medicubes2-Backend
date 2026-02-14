using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CommonService : ICommonService
    {
        private readonly IJWTService jWTService;
        private readonly AppDbContext _db;

        public CommonService(IJWTService jWTService, AppDbContext db)
        {
            this.jWTService = jWTService;
            this._db = db;
        }
        public async Task<object> GetConfigSettings()
        {
            var jwtcliams = jWTService.DecodeToken();
            var tenantId = jwtcliams.TenantId;
            var settings = await _db.ConfigSettings.Where(s => s.tenantid == tenantId).ToListAsync();
            return settings;
        }

        public async Task<object> GetSaleDeliveryList()
        {
            var tempUser = jWTService.DecodeToken();

            var res = await (from sale in _db.SaleDeliveries.Where(sale => sale.IsDeleted == 0 && sale.TenantId == tempUser.TenantId)
                             join cust in _db.Customers on sale.CustId equals cust.customerid into custJoin
                             from cust in custJoin.DefaultIfEmpty()
                             select new
                             {
                                 sale.DeliveryId,
                                 sale.DeliveryNumber,
                                 sale.Notes,
                                 sale.DeliveryDate,
                                 sale.CustId,
                                 CustomerName = cust != null ? cust.name : ""
                             }).ToListAsync();

            return res;
        }
        public async Task<object> GetAllLocations()
        {
            var tempUser = jWTService.DecodeToken();
            return await _db.OrgLocations.Where(a => a.TenantId == tempUser.TenantId).ToListAsync();
        }

        public async Task<object> GetSaleDeliveryInInvoiceList()
        {
            var tempUser = jWTService.DecodeToken();

            var deliveries = await (from sale in _db.SaleDeliveries.Where(a => a.IsDeleted == 0 && a.TenantId == tempUser.TenantId)
                                    join cust in _db.Customers.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId) on sale.CustId equals cust.customerid
                                    select new
                                    {
                                        sale.DeliveryId,
                                        sale.DeliveryNumber,
                                        sale.CustId,
                                        sale.DeliveryDate,
                                        sale.ExpectedDeliveryDate,
                                        sale.DeliveryStatusId,
                                        sale.ShippingAddress,
                                        sale.ShippingMethod,
                                        sale.TrackingNumber,
                                        sale.ContactPerson,
                                        sale.ContactPhone,
                                        sale.Notes,
                                        sale.CreatedBy,
                                        sale.CreatedOn,
                                        CustomerName = cust.name,
                                        deliveryName = sale.DeliveryNumber + "-" + cust.name

                                    }).ToListAsync();

            if (deliveries.Count == 0)
                return deliveries;

            var deliveryIdsInInvoice = await _db.Invoices.Where(x => x.IsDeleted == 0 && x.TenantId == tempUser.TenantId).Select(x => x.DeliveryId).ToListAsync();

            var filteredList = deliveries.Where(d => !deliveryIdsInInvoice.Contains(d.DeliveryId)).ToList();

            return filteredList;

        }

        public async Task<object> GetTaxRate(int stateId, int saleTypeId)
        {
            var taxRates = await _db.ITaxRates
                .Where(tr => tr.StateId == stateId && tr.SaleTypeId == saleTypeId && tr.IsDeleted == 0)
                .Select(tr => new
                {
                    tr.RateId,
                    tr.RateTitle,
                    tr.RateValue,
                    tr.StateId,
                    tr.SaleTypeId,
                    tr.TaxRateId
                })
                .ToListAsync();

            return taxRates;
        }

        public async Task<object> GetSROSchedule(int stateId, int rateId)
        {
            // rateId is the primary key (RateId) from itaxrate table
            var sroSchedules = await _db.ISroSchedules
                .Where(sro => sro.StateId == stateId && sro.RateId == rateId && sro.IsDeleted == 0)
                .Select(sro => new
                {
                    sro.SroId,
                    sro.SerNo,
                    sro.Title,
                    sro.RateId,
                    sro.StateId
                })
                .ToListAsync();

            return sroSchedules;
        }

        public async Task<object> GetSROItemCodes(int sroId)
        {
            var sroItemCodes = await _db.ISroItemCodes
                .Where(item => item.SroId == sroId && item.IsDeleted == 0)
                .Select(item => new
                {
                    ItemId = item.SchItemId, // SchItemId is the primary key from isroitemcode table
                    item.SroItemTitle,
                    item.SroId
                })
                .ToListAsync();

            return sroItemCodes;
        }

        public async Task<object> GetAllInvoiceTypes()
        {
            var invoiceTypes = await _db.IInvoiceTypes
                .Select(it => new
                {
                    it.InvoiceTypeId,
                    it.InvoiceTypeDescription
                })
                .ToListAsync();

            return invoiceTypes;
        }
    }
}
