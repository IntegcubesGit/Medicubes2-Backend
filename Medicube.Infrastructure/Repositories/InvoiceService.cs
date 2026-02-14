using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Azure;
using Domain.DTOs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Repositories
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _db;
        private readonly IJWTService _jwtService;

        public InvoiceService(AppDbContext context, IJWTService jWTService)
        {
            _db = context;
            _jwtService = jWTService;
        }

        public async Task<object> AddInvoice(InvoiceDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();

            using var transaction = await _db.Database.BeginTransactionAsync();
            var autoInvoiceNo = await GenerateInvoiceNumberAsync();
            if (dto.DeliveryId != null)
            {
                var IsCheck = await _db.Invoices.Where(a => a.IsDeleted == 0 && a.TenantId == tempUser.TenantId && a.DeliveryId == dto.DeliveryId).FirstOrDefaultAsync();
                if (IsCheck != null)
                {
                    return new
                    {
                        success = false,
                        message = "Already Invoice exist against Delivery Id " + dto.DeliveryId,
                        statusCode = 200
                    };
                }
            }

            try
            {
                var master = new Invoice
                {
                    DeliveryId = dto.DeliveryId,
                    CustId = dto.CustId,
                    OriginationId = dto.OriginationId,
                    DestinationId = dto.DestinationId,
                    InvoiceNumber = autoInvoiceNo,
                    InvoiceDate = dto.InvoiceDate.ToUniversalTime(),
                    FBRTestScenarioId = dto.FBRTestScenarioId,
                    PaymentTerm = dto.PaymentTerm,
                    TotalDiscount = dto.TotalDiscount,
                    TotalTax = dto.TotalTax,
                    Notes = dto.Notes,
                    IDocTypeId = dto.InvoiceTypeId,
                    SaleTypeId = dto.SaleTypeId,
                    TenantId = tempUser.TenantId,
                    CreatedBy = tempUser.UserId,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = 0,
                    StatusId = (int)EnumInvoiceStatus.InvoiceStatus.Draft

                };

                _db.Invoices.Add(master);
                await _db.SaveChangesAsync();

                if (dto.Details != null && dto.Details.Count > 0)
                {
                    var productLogs = new List<InvoiceProductLog>();

                    foreach (var d in dto.Details)
                    {
                        var detail = new InvoiceDetail
                        {
                            InvoiceId = master.InvoiceId,
                            ItemId = d.ItemId,
                            Qty = d.Qty,
                            Price = d.Price,
                            Discount = d.Discount,
                            SaleTax = d.StRate
                            //SaleTax = d.ValueExclST * d.StRate / 100,
                            //ValueInclST = d.ValueExclST + (d.ValueExclST * d.StRate / 100)
                        };
                        _db.InvoiceDetails.Add(detail);

                        // Prepare invoice product log
                        var productLog = new InvoiceProductLog
                        {
                            ProductId = d.ItemId,
                            InvoiceId = master.InvoiceId,
                            RateId = d.RateId,
                            SroId = d.SroId,
                            ItemId = d.SroItemId, // SchItemId from isroitemcode table (SroItemId in DTO maps to SchItemId in entity)
                            CreatedOn = DateTime.UtcNow
                        };
                        productLogs.Add(productLog);
                    }
                    await _db.SaveChangesAsync();

                    // Save all product logs
                    if (productLogs.Count > 0)
                    {
                        await _db.InvoiceProductLogs.AddRangeAsync(productLogs);
                        await _db.SaveChangesAsync();
                    }
                }

                var UpdateDeliveryStatus = await _db.SaleDeliveries.Where(a => a.IsDeleted == 0 && a.DeliveryId == master.DeliveryId && a.TenantId == tempUser.TenantId).FirstOrDefaultAsync();
                if (UpdateDeliveryStatus != null)
                {
                    UpdateDeliveryStatus.DeliveryStatusId = (int)EnumIDeliveryStatus.IDeliveryStatus.Delivered;
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return new { success = true, message = "Invoice added successfully", invoiceId = master.InvoiceId };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new { success = false, message = ex.Message };
            }
        }

        public async Task<object> DeleteInvoice(int invoiceId)
        {
            var tempUser = _jwtService.DecodeToken();
            var existingItem = await _db.Invoices.FirstOrDefaultAsync(a => a.InvoiceId == invoiceId && a.IsDeleted == 0 && a.TenantId == tempUser.TenantId);

            if (existingItem == null)
            {
                return new
                {
                    message = "Record not found!",
                    statusCode = 200,
                    success = false
                };
            }

            existingItem.IsDeleted = 1;
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Invoice delete successfully",
                data = existingItem
            };
        }

        public async Task<object> GetAllInvoiceList(int status, int custId, string startDate, string ToDate)
        {
            var tempUser = _jwtService.DecodeToken();
            var formats = new[]{
                    "MM-dd-yyyy",
                    "dd-MM-yyyy",
                    "yyyy-MM-dd",
                    "yyyy/MM/dd",
                    "MM/dd/yyyy"
                };
            if (!DateTime.TryParseExact(startDate, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime startDateUtc))
            {
                throw new Exception("Invalid startDate format. Send ISO format: yyyy-MM-dd");
            }
            if (!DateTime.TryParseExact(ToDate, formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime endDateUtc))
            {
                throw new Exception("Invalid endDate format. Send ISO format: yyyy-MM-dd");
            }
            endDateUtc = endDateUtc.AddDays(1).AddSeconds(-1).ToUniversalTime();
            startDateUtc = startDateUtc.ToUniversalTime();

            // STEP 1: Get all invoices with basic info
            var invoicesMaster = await (from inv in _db.Invoices.Where(a => a.IsDeleted == 0 && a.TenantId == tempUser.TenantId && (status == -1 || a.StatusId == status) && (a.CreatedOn.Date >= startDateUtc.Date && a.CreatedOn.Date <= endDateUtc.Date) && (custId == -1 || a.CustId == custId) && a.ExtId == 0)
                                        join cust in _db.Customers.Where(c => c.isdeleted == 0 && c.tenantid == tempUser.TenantId) on inv.CustId equals cust.customerid
                                        join origin in _db.IStateTypes.Where(a => a.IsDeleted == 0) on inv.OriginationId equals origin.StateId
                                        join dest in _db.IStateTypes.Where(a => a.IsDeleted == 0) on inv.DestinationId equals dest.StateId
                                        join stat in _db.InvoiceStatuses.Where(s => s.IsDeleted == 0) on inv.StatusId equals stat.InvoiceStatusId
                                        join fbr in _db.IFBRScenarios.Where(s => s.IsDeleted == 0) on inv.FBRTestScenarioId equals fbr.FBRScenarioTestId
                                        select new
                                        {
                                            inv.InvoiceId,
                                            CustomerId = inv.CustId,
                                            inv.InvoiceDate,
                                            inv.TotalDiscount,
                                            inv.TotalTax,
                                            inv.StatusId,
                                            Status = stat.Title,
                                            inv.OriginationId,
                                            OriginationName = origin.StateName,
                                            inv.DestinationId,
                                            DestinationName = dest.StateName,
                                            inv.PaymentTerm,
                                            inv.CreatedOn,
                                            CustomerName = cust.name,
                                            inv.FBRTestScenarioId,
                                            FBRTestScenario = fbr.Title,
                                            InvoiceTypeId = inv.IDocTypeId,
                                            inv.SaleTypeId
                                        }).OrderByDescending(a => a.CreatedOn).ToListAsync();

            // STEP 2: Get invoice IDs for filtered invoices
            var invoiceIds = invoicesMaster.Select(x => x.InvoiceId).ToList();

            // STEP 3: Get invoice details ONLY for filtered invoices
            var allInvoiceDetails = await (
                from invdtl in _db.InvoiceDetails.Where(d => invoiceIds.Contains(d.InvoiceId))
                join item in _db.Products.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId)
                        on invdtl.ItemId equals item.itemid
                join hscode in _db.IHSCodes.Where(a => a.IsDeleted == 0)
                        on item.hscodeid equals hscode.HSCodeId
                join unit in _db.IUnitTypes.Where(a => a.IsDeleted == 0)
                        on item.unitid equals unit.UnitId
                select new
                {
                    invdtl.InvoiceId,
                    invdtl.Qty,
                    invdtl.Discount,
                    invdtl.Price,
                    TaxRate = invdtl.SaleTax
                }
            ).ToListAsync();

            // STEP 4: Calculate tax and total for each detail line
            var detailsWithTax = allInvoiceDetails.Select(det =>
            {
                decimal A = det.Qty;
                decimal B = det.Price;
                decimal C = det.Discount;
                decimal taxRate = det.TaxRate;

                decimal AB = A * B;                              // Qty * Price
                decimal D = AB * C / 100m;                       // Discount amount
                decimal valueExclST = AB - D;                    // Excluding sales tax
                decimal salesTax = Math.Round((valueExclST * taxRate) / 100m, 2);
                decimal valueIncTax = Math.Round(valueExclST + salesTax, 2);    // ✅ ADDED: Total with tax

                return new
                {
                    det.InvoiceId,
                    SalesTax = salesTax,
                    TotalDue = valueIncTax  
                };
            }).ToList();

            // STEP 5: Group by InvoiceId and sum the tax and total
            var invoiceTotals = detailsWithTax
                .GroupBy(x => x.InvoiceId)
                .ToDictionary(g => g.Key, g => new
                {
                    TotalTax = g.Sum(x => x.SalesTax),
                    TotalDue = g.Sum(x => x.TotalDue)  // ✅ ADDED: Sum total due
                });

            // STEP 6: Merge calculated tax and total back to master list
            var result = invoicesMaster.Select(inv => new
            {
                inv.InvoiceId,
                inv.CustomerId,
                inv.InvoiceDate,
                inv.TotalDiscount,
                TotalTax = invoiceTotals.ContainsKey(inv.InvoiceId) ? invoiceTotals[inv.InvoiceId].TotalTax : 0m,
                TotalDue = invoiceTotals.ContainsKey(inv.InvoiceId) ? invoiceTotals[inv.InvoiceId].TotalDue : 0m,  // ✅ ADDED
                inv.StatusId,
                inv.Status,
                inv.OriginationId,
                inv.OriginationName,
                inv.DestinationId,
                inv.DestinationName,
                inv.PaymentTerm,
                inv.CreatedOn,
                inv.CustomerName,
                inv.FBRTestScenarioId,
                inv.FBRTestScenario,
                inv.InvoiceTypeId,
                inv.SaleTypeId
            }).OrderByDescending(a => a.CreatedOn).ToList();

            return result;
        }

        public async Task<object> GetDeliveryFromInvoice(int deliveryId)
        {
            var tempUser = _jwtService.DecodeToken();
            var master = await (from sale in _db.SaleDeliveries.Where(a => a.DeliveryId == deliveryId && a.IsDeleted == 0 && a.TenantId == tempUser.TenantId)
                                join custId in _db.Customers.Where(a => a.isdeleted == 0) on sale.CustId equals custId.customerid
                                join delStatus in _db.IDeliveryStatuses.Where(a => a.IsDeleted == 0) on sale.DeliveryStatusId equals delStatus.DeliveryStatusId
                                select new
                                {
                                    sale.DeliveryId,
                                    sale.CustId,
                                    customerName = custId.name,
                                    sale.DeliveryNumber,
                                    sale.DeliveryDate,
                                    sale.ContactPerson,
                                    sale.Notes,
                                    sale.DeliveryStatusId,
                                    DeliveryStatus = delStatus.Title,
                                    invoiceDate = DateTime.Now
                                }).FirstOrDefaultAsync();
            if (master == null)
                return new { error = "Record not found" };

            var details = await (from sdtl in _db.SaleDeliveryDetails.Where(d => d.SaleDeliveryId == deliveryId)
                                 join item in _db.Products.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId) on sdtl.ItemId equals item.itemid
                                 select new
                                 {
                                     ProductName = item.name,
                                     ItemId = item.itemid,
                                     ProductCode = item.code,
                                     sdtl.SaleDeliveryId,
                                     sdtl.SaleDtlId,
                                     Qty = sdtl.Quantity,
                                     Price = sdtl.UnitPrice,
                                     Total = sdtl.TotalPrice
                                 }
                                ).ToListAsync();

            return new
            {
                master,
                details,
                SaleTotals = details.GroupBy(s => s.SaleDeliveryId).Select(g => new
                {
                    TotalQty = g.Sum(x => x.Qty),
                    TotalUnitPrice = g.Sum(x => x.Price),
                    TotalPrice = (g.Sum(x => x.Total))
                })
            };
        }

        public async Task<object> GetIFBRScenarioStatus()
        {
            var tempUser = _jwtService.DecodeToken();

            var list = await _db.IFBRScenarios
                .Where(a => a.IsDeleted == 0)
                .Select(a => new
                {
                    a.FBRScenarioTestId,
                    ScenarioTitle = a.ScenarioId + " - " + a.Title,
                    a.ScenarioId
                })
                .ToListAsync();

            return list;
        }

        public async Task<object> GetInvoice(int invoiceId)
        {
            var tempUser = _jwtService.DecodeToken();

            var master = await (from inv in _db.Invoices
                                where inv.InvoiceId == invoiceId
                                    && inv.IsDeleted == 0
                                    && inv.TenantId == tempUser.TenantId
                                join invoiceType in _db.IInvoiceTypes
                                    on inv.IDocTypeId equals invoiceType.InvoiceTypeId
                                    into invoiceTypeGroup
                                from invoiceType in invoiceTypeGroup.DefaultIfEmpty()
                                join saleType in _db.ISaleTypes.Where(a => a.IsDeleted == 0)
                                    on inv.SaleTypeId equals saleType.SaleTypeId
                                    into saleTypeGroup
                                from saleType in saleTypeGroup.DefaultIfEmpty()
                                select new
                                {
                                    inv.InvoiceId,
                                    inv.DeliveryId,
                                    inv.CustId,
                                    inv.OriginationId,
                                    inv.DestinationId,
                                    inv.InvoiceNumber,
                                    inv.InvoiceDate,
                                    inv.TotalDiscount,
                                    inv.TotalTax,
                                    inv.FBRTestScenarioId,
                                    inv.PaymentTerm,
                                    inv.Notes,
                                    inv.StatusId,
                                    InvoiceTypeId = inv.IDocTypeId,
                                    InvoiceTypeTitle = invoiceType != null ? invoiceType.InvoiceTypeDescription : (string?)null,
                                    inv.SaleTypeId,
                                    SaleTypeTitle = saleType != null ? saleType.SaleName : (string?)null
                                })
                .FirstOrDefaultAsync();

            if (master == null)
                return new { error = "Record not found" };


            var detailsRaw = await (
                from inv in _db.InvoiceDetails.Where(d => d.InvoiceId == invoiceId)
                join item in _db.Products.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId)
                    on inv.ItemId equals item.itemid
                join productLog in _db.InvoiceProductLogs.Where(p => p.InvoiceId == invoiceId)
                    on new { inv.InvoiceId, inv.ItemId } equals new { productLog.InvoiceId, ItemId = productLog.ProductId }
                    into logGroup
                from log in logGroup.DefaultIfEmpty()
                select new
                {
                    ProductName = item.name,
                    ProductCode = item.code,
                    inv.InvoiceId,
                    inv.InvoiceDtlId,
                    inv.ItemId,
                    inv.Qty,
                    inv.Discount,
                    inv.Price,
                    inv.SaleTax,
                    RateId = log != null ? log.RateId : (int?)null,
                    SroId = log != null ? log.SroId : (int?)null,
                    SchItemId = log != null ? log.ItemId : (int?)null // SchItemId from isroitemcode table
                }
            ).ToListAsync();


            // Apply manual calculations here
            var details = detailsRaw.Select(det =>
            {
                decimal A = det.Qty;        // Qty
                decimal B = det.Price;      // Price
                decimal C = det.Discount;   // Discount %

                decimal D = (B * A) * C / 100m;      // Discount Amount
                decimal valueExclST = (B * A) - D;   // Excluding Sales Tax

                decimal salesTax = Math.Round((valueExclST * det.SaleTax) / 100m, 2);
                decimal valueIncTax = Math.Round(valueExclST + salesTax, 2);

                return new
                {
                    det.ProductName,
                    det.ProductCode,
                    det.InvoiceId,
                    det.InvoiceDtlId,
                    SroItemId = det.SchItemId, // SchItemId from isroitemcode table (returned as SroItemId in response)
                    det.Qty,
                    det.Discount,
                    det.Price,
                    det.SaleTax,
                    det.RateId,
                    det.SroId,
                    // NEW CALCULATED FIELDS
                    ValueExclST = Math.Round(valueExclST, 2),
                    SalesTax = salesTax,
                    ValueIncTax = valueIncTax
                };
            }).ToList();


            return new
            {
                master,
                details
            };
        }

        public async Task<object> GetInvoiceSlipDetail(int invoiceId)
        {
            var tempUser = _jwtService.DecodeToken();

            // --- MASTER RECORD ----------------------------------------------------
            var list = await (
                from inv in _db.Invoices.Where(a =>
                        a.IsDeleted == 0 &&
                        a.TenantId == tempUser.TenantId &&
                        a.InvoiceId == invoiceId &&
                        a.ExtId == 0)
                join cust in _db.Customers.Where(s => s.isdeleted == 0 && s.tenantid == tempUser.TenantId)
                        on inv.CustId equals cust.customerid
                join origin in _db.IStateTypes.Where(a => a.IsDeleted == 0)
                        on inv.OriginationId equals origin.StateId
                join dest in _db.IStateTypes.Where(a => a.IsDeleted == 0)
                        on inv.DestinationId equals dest.StateId
                join stat in _db.InvoiceStatuses.Where(s => s.IsDeleted == 0)
                        on inv.StatusId equals stat.InvoiceStatusId
                join fbr in _db.IFBRScenarios.Where(s => s.IsDeleted == 0)
                        on inv.FBRTestScenarioId equals fbr.FBRScenarioTestId
                join org in _db.OrgAppSettings.Where(a => a.IsDeleted == 0 && a.TenantId == tempUser.TenantId)
                        on inv.TenantId equals org.TenantId
                join custState in _db.IStateTypes.Where(a => a.IsDeleted == 0)
                        on cust.stateprovince equals custState.StateId
                join invoiceType in _db.IInvoiceTypes
                        on inv.IDocTypeId equals invoiceType.InvoiceTypeId
                        into invoiceTypeGroup
                from invoiceType in invoiceTypeGroup.DefaultIfEmpty()
                join saleType in _db.ISaleTypes.Where(a => a.IsDeleted == 0)
                        on inv.SaleTypeId equals saleType.SaleTypeId
                        into saleTypeGroup
                from saleType in saleTypeGroup.DefaultIfEmpty()

                select new
                {
                    inv.InvoiceId,
                    CustomerId = inv.CustId,
                    inv.InvoiceNumber,
                    inv.InvoiceDate,
                    inv.TotalDiscount,
                    inv.TotalTax,
                    inv.StatusId,
                    cust.cnic,
                    Status = stat.Title,
                    StateProvinceId = cust.stateprovince,
                    SaleOrigin = custState.StateName,
                    inv.DestinationId,
                    DestinationName = dest.StateName,
                    inv.PaymentTerm,
                    inv.CreatedOn,
                    BillTo = cust.name,
                    BillFrom = org.OrganizationName,
                    BillFromAddress = org.Address,
                    BillToAddress = cust.address,
                    BuyerRegistrationType = "Unregistered",
                    inv.Notes,
                    inv.FBRTestScenarioId,
                    FBRTestScenario = fbr.Title,
                    ScenarioId = fbr.ScenarioId,
                    inv.FBRInvoiceNumber,
                    InvoiceTypeId = inv.IDocTypeId,
                    InvoiceTypeTitle = invoiceType != null ? invoiceType.InvoiceTypeDescription : (string?)null,
                    inv.SaleTypeId,
                    SaleTypeTitle = saleType != null ? saleType.SaleName : (string?)null
                }
            ).FirstOrDefaultAsync();


            // --- DETAILS WITH MANUAL CALCULATIONS -------------------------------
            var detailsRaw = await (
                from invdtl in _db.InvoiceDetails.Where(d => d.InvoiceId == invoiceId)
                join item in _db.Products.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId)
                        on invdtl.ItemId equals item.itemid
                join hscode in _db.IHSCodes.Where(a => a.IsDeleted == 0)
                        on item.hscodeid equals hscode.HSCodeId
                join unit in _db.IUnitTypes.Where(a => a.IsDeleted == 0)
                        on item.unitid equals unit.UnitId
                join productLog in _db.InvoiceProductLogs.Where(p => p.InvoiceId == invoiceId)
                        on new { invdtl.InvoiceId, invdtl.ItemId } equals new { productLog.InvoiceId, ItemId = productLog.ProductId }
                        into logGroup
                from log in logGroup.DefaultIfEmpty()
                join sroSchedule in _db.ISroSchedules.Where(a => a.IsDeleted == 0)
                        on log.SroId equals sroSchedule.SroId
                        into sroScheduleGroup
                from sroSchedule in sroScheduleGroup.DefaultIfEmpty()
                join sroItemCode in _db.ISroItemCodes.Where(a => a.IsDeleted == 0)
                        on log.ItemId equals sroItemCode.SchItemId
                        into sroItemCodeGroup
                from sroItemCode in sroItemCodeGroup.DefaultIfEmpty()
                join taxRate in _db.ITaxRates.Where(a => a.IsDeleted == 0)
                        on log.RateId equals taxRate.RateId
                        into taxRateGroup
                from taxRate in taxRateGroup.DefaultIfEmpty()

                select new
                {
                    ProductName = item.name,
                    ProductCode = item.code,
                    HsCode = hscode.Code,
                    uoM = unit.UnitName,
                    invdtl.InvoiceId,
                    invdtl.InvoiceDtlId,
                    invdtl.ItemId,
                    invdtl.Qty,
                    invdtl.Discount,
                    invdtl.Price,
                    TaxRate = invdtl.SaleTax,
                    RateId = log != null ? log.RateId : (int?)null,
                    RateTitle = taxRate != null ? taxRate.RateTitle : (string?)null,
                    SroId = log != null ? log.SroId : (int?)null,
                    SchItemId = log != null ? log.ItemId : (int?)null, // SchItemId from isroitemcode table
                    SroScheduleTitle = sroSchedule != null ? sroSchedule.Title : (string?)null,
                    SroItemTitle = sroItemCode != null ? sroItemCode.SroItemTitle : (string?)null
                }
            ).ToListAsync();


            // Now compute the required fields manually (using Version 1 calculations)
            var details = detailsRaw.Select(det =>
            {
                decimal A = det.Qty;
                decimal B = det.Price;
                decimal C = det.Discount;
                decimal taxRate = det.TaxRate;

                decimal AB = A * B;                 // Qty * Price
                decimal D = AB * C / 100m;          // Discount amount
                decimal valueExclST = AB - D;       // Excluding sales tax

                decimal salesTax = Math.Round((valueExclST * taxRate) / 100m, 2);
                decimal total = Math.Round(valueExclST + salesTax, 2);

                return new
                {
                    det.ProductName,
                    det.ProductCode,
                    det.HsCode,
                    det.uoM,
                    det.InvoiceId,
                    det.InvoiceDtlId,
                    SroItemId = det.SchItemId, // SchItemId from isroitemcode table (returned as SroItemId in response)
                    det.Qty,
                    det.Discount,
                    det.Price,
                    det.TaxRate,
                    det.RateId,
                    det.RateTitle,
                    det.SroId,
                    det.SroScheduleTitle,
                    det.SroItemTitle,
                    SubTotal = AB,
                    ValueExclST = Math.Round(valueExclST, 2),
                    SalesTax = salesTax,
                    Total = total,
                    SalesTaxApplicable = Math.Round(valueExclST, 2) * (det.TaxRate / 100m)
                };
            }).ToList();


            var invoiceTotals = details
                .GroupBy(s => s.InvoiceId)
                .Select(g => new
                {
                    SubTotalGroup = g.Sum(x => x.SubTotal),
                    TotalTaxGroup = g.Sum(x => x.SalesTax),
                    TotalDue = g.Sum(x => x.SubTotal + x.SalesTax)
                });


            return new
            {
                master = list,
                detail = details,
                invoiceTotals
            };
        }

        public async Task<object> GetProductDetail()
        {
            var temp = _jwtService.DecodeToken();
            var res = await _db.Products.Where(a => a.isdeleted == 0 && a.isactive == 1).Select(a => new
            {
                Name = a.name,
                StandardPrice = a.price
            }).ToListAsync();
            return res;
        }

        public async Task<object> SaveSubmitFBR(FBRResponseDTO dto)
        {
            var temp = _jwtService.DecodeToken();

            var invoice = await _db.Invoices.Where(s => s.IsDeleted == 0 && s.TenantId == temp.TenantId && s.ExtId != 0 && s.InvoiceId == dto.InvoiceId).FirstOrDefaultAsync();
            if (invoice == null)
            {
                return new
                {
                    statusCode = 200,
                    message = "Record not found",
                    success = false
                };
            }

            if (dto.response != null)
            {
                if (dto.StatusCode == 200)
                {
                    invoice.FBRInvoiceNumber = dto.FBRInvoiceNumber;
                    invoice.StatusId = (int)EnumInvoiceStatus.InvoiceStatus.SubmittedToFBR;
                }

                invoice.FBRResponse = dto.response.ToJsonString();
                await _db.SaveChangesAsync();

                return new
                {
                    statusCode = 200,
                    message = "FBR Response Saved Successfully",
                    success = true
                };
            }

            return new
            {
                statusCode = 200,
                message = "No FBR response provided",
                success = false
            };
        }


        public async Task<object> UpdateArchivedById(int invoiceId)
        {
            var tempUser = _jwtService.DecodeToken();
            var existingItem = await _db.Invoices.FirstOrDefaultAsync(a => a.InvoiceId == invoiceId && a.IsDeleted == 1 && a.TenantId == tempUser.TenantId);

            if (existingItem == null)
            {
                return new
                {
                    message = "Record not found!",
                    statusCode = 200,
                    success = false
                };
            }

            existingItem.IsDeleted = 0;
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Invoice restore successfully",
                data = existingItem
            };
        }

        public async Task<object> UpdateInvoice(InvoiceDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var master = await _db.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == dto.InvoiceId && i.TenantId == tempUser.TenantId && i.IsDeleted == 0);

                if (master == null)
                    return new { success = false, message = "Invoice not found" };

                master.DeliveryId = dto.DeliveryId;
                master.CustId = dto.CustId;
                master.OriginationId = dto.OriginationId;
                master.DestinationId = dto.DestinationId;
                master.InvoiceNumber = dto.InvoiceNumber;
                master.InvoiceDate = dto.InvoiceDate.ToUniversalTime();
                master.FBRTestScenarioId = dto.FBRTestScenarioId;
                master.PaymentTerm = dto.PaymentTerm;
                master.TotalDiscount = dto.TotalDiscount;
                master.TotalTax = dto.TotalTax;
                master.Notes = dto.Notes;
                master.IDocTypeId = dto.InvoiceTypeId;
                master.SaleTypeId = dto.SaleTypeId;
                master.ModifiedBy = tempUser.UserId;
                master.ModifiedOn = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                var oldDetails = _db.InvoiceDetails.Where(x => x.InvoiceId == master.InvoiceId);
                _db.InvoiceDetails.RemoveRange(oldDetails);
                
                // Remove old product logs
                var oldProductLogs = _db.InvoiceProductLogs.Where(x => x.InvoiceId == master.InvoiceId);
                _db.InvoiceProductLogs.RemoveRange(oldProductLogs);
                await _db.SaveChangesAsync();

                if (dto.Details != null && dto.Details.Count > 0)
                {
                    var productLogs = new List<InvoiceProductLog>();

                    foreach (var d in dto.Details)
                    {
                        var detail = new InvoiceDetail
                        {
                            InvoiceId = master.InvoiceId,
                            ItemId = d.ItemId,
                            Qty = d.Qty,
                            Price = d.Price,
                            Discount = d.Discount,
                            SaleTax = d.StRate
                        };
                        _db.InvoiceDetails.Add(detail);

                        // Prepare invoice product log
                        var productLog = new InvoiceProductLog
                        {
                            ProductId = d.ItemId,
                            InvoiceId = master.InvoiceId,
                            RateId = d.RateId,
                            SroId = d.SroId,
                            ItemId = d.SroItemId, // SchItemId from isroitemcode table (SroItemId in DTO maps to SchItemId in entity)
                            CreatedOn = DateTime.UtcNow
                        };
                        productLogs.Add(productLog);
                    }
                    await _db.SaveChangesAsync();

                    // Save all product logs
                    if (productLogs.Count > 0)
                    {
                        await _db.InvoiceProductLogs.AddRangeAsync(productLogs);
                        await _db.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                return new { success = true, message = "Invoice updated successfully" };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new { success = false, message = ex.Message };
            }
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var lastInvoice = await _db.Invoices
                .OrderByDescending(x => x.InvoiceId)
                .Select(x => x.InvoiceNumber)
                .FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(lastInvoice) && lastInvoice.StartsWith("INV-"))
            {
                var num = lastInvoice.Replace("INV-", "");
                int.TryParse(num, out next);
                next++;
            }

            return $"INV-{next:D5}";
        }

        public async Task<object> GetArchivedList()
        {
            var tempUser = _jwtService.DecodeToken();

            var list = await (from inv in _db.Invoices.Where(a => a.IsDeleted == 1 && a.TenantId == tempUser.TenantId && a.ExtId == 0)
                              join cust in _db.Customers.Where(s => s.isdeleted == 0 && s.tenantid == tempUser.TenantId) on inv.CustId equals cust.customerid
                              join origin in _db.IStateTypes.Where(a => a.IsDeleted == 0) on inv.OriginationId equals origin.StateId
                              join dest in _db.IStateTypes.Where(a => a.IsDeleted == 0) on inv.DestinationId equals dest.StateId
                              join stat in _db.InvoiceStatuses.Where(s => s.IsDeleted == 0) on inv.StatusId equals stat.InvoiceStatusId
                              join fbr in _db.IFBRScenarios.Where(s => s.IsDeleted == 0) on inv.FBRTestScenarioId equals fbr.FBRScenarioTestId
                              select new
                              {
                                  inv.InvoiceId,
                                  CustomerId = inv.CustId,
                                  inv.InvoiceDate,
                                  inv.TotalDiscount,
                                  inv.TotalTax,
                                  inv.StatusId,
                                  Status = stat.Title,
                                  inv.OriginationId,
                                  OriginationName = origin.StateName,
                                  inv.DestinationId,
                                  DestinationName = dest.StateName,
                                  inv.PaymentTerm,
                                  inv.CreatedOn,
                                  CustomerName = cust.name,
                                  inv.FBRTestScenarioId,
                                  FBRTestScenario = fbr.Title
                              }).OrderByDescending(a => a.CreatedOn).ToListAsync();


            return list;
        }

    }
    }
