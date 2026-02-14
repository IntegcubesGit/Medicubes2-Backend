using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Domain.DTOs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CreditNoteService : ICreditNoteService
    {
        private readonly AppDbContext _db;
        private readonly IJWTService _jwtService;

        public CreditNoteService(AppDbContext context, IJWTService jWTService)
        {
            _db = context;
            _jwtService = jWTService;
        }

        public async Task<object> AddCreditNote(CreditNoteDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var master = new Invoice
                {
                    DeliveryId = dto.DeliveryId,
                    CustId = dto.CustId,
                    OriginationId = dto.OriginationId,
                    DestinationId = dto.DestinationId,
                    InvoiceNumber = dto.InvoiceNumber,
                    InvoiceDate = dto.InvoiceDate.ToUniversalTime(),
                    FBRTestScenarioId = dto.FBRTestScenarioId,
                    PaymentTerm = dto.PaymentTerm,
                    TotalDiscount = dto.TotalDiscount,
                    TotalTax = dto.TotalTax,
                    Notes = dto.Notes,
                    TenantId = tempUser.TenantId,
                    CreatedBy = tempUser.UserId,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = 0,
                    StatusId = (int)EnumInvoiceStatus.InvoiceStatus.Draft,
                    ExtId = dto.InvoiceId
                };

                _db.Invoices.Add(master);
                await _db.SaveChangesAsync();

                if (dto.Details != null && dto.Details.Count > 0)
                {
                    foreach (var d in dto.Details)
                    {
                        var detail = new InvoiceDetail
                        {
                            InvoiceId = master.InvoiceId,
                            ItemId = d.ItemId,
                            Qty = -Math.Abs(d.Qty),
                            Price = d.Price,
                            Discount = d.Discount,
                            SaleTax = d.StRate
                            //SaleTax = d.ValueExclST * d.StRate / 100,
                            //ValueInclST = d.ValueExclST + (d.ValueExclST * d.StRate / 100)
                        };
                        _db.InvoiceDetails.Add(detail);
                    }
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return new { success = true, message = "Credit notes added successfully", invoiceId = master.InvoiceId };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new { success = false, message = ex.Message };
            }
        }

        public async Task<object> DeleteCreditNotes(int invoiceId)
        {
            var tempUser = _jwtService.DecodeToken();
            var CreditNotes = await _db.Invoices.Where(a => a.IsDeleted == 0 && a.TenantId == tempUser.TenantId && a.ExtId != 0 && a.InvoiceId == invoiceId).FirstOrDefaultAsync();
            if (CreditNotes == null)
            {
                return new
                {
                    message = "Record not found!",
                    statusCode = 200,
                    success = false
                };
            }
            CreditNotes.IsDeleted = 1;
            await _db.SaveChangesAsync();
            return new
            {
                message = "Delete record successfully",
                statusCode = 200,
                success = true
            };
        }

        public async Task<object> GetAllCreditNoteList(CreditNoteListDTO req)
        {
            var tempUser = _jwtService.DecodeToken();

            var formats = new[]{
                                "MM-dd-yyyy",
                                "dd-MM-yyyy",
                                "yyyy-MM-dd",
                                "yyyy/MM/dd",
                                "MM/dd/yyyy"
                            };

            if (!DateTime.TryParseExact(req.startDate, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime startDateUtc))
            {
                throw new Exception("Invalid startDate format. Send ISO format: yyyy-MM-dd");
            }

            if (!DateTime.TryParseExact(req.endDate, formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime endDateUtc))
            {
                throw new Exception("Invalid endDate format. Send ISO format: yyyy-MM-dd");
            }

            endDateUtc = endDateUtc.AddDays(1).AddSeconds(-1).ToUniversalTime();
            startDateUtc = startDateUtc.ToUniversalTime();

            var list = await (from inv in _db.Invoices.Where(a => a.IsDeleted == 0 && a.TenantId == tempUser.TenantId && a.ExtId != 0 && (a.ReasonTypeId == req.reasonTypeId || req.reasonTypeId == -1) && (a.StatusId == req.statusId || req.statusId == -1) && (a.CreatedOn >= startDateUtc && a.CreatedOn <= endDateUtc) && (a.CustId == req.customerId || req.customerId == -1))
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
                                  inv.InvoiceNumber,
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

        public async Task<object> GetArchivedList()
        {
            var tempUser = _jwtService.DecodeToken();

            var list = await (from inv in _db.Invoices.Where(a => a.IsDeleted == 1 && a.TenantId == tempUser.TenantId && a.ExtId != 0)
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
                                  inv.InvoiceNumber,
                                  CustomerName = cust.name,
                                  inv.FBRTestScenarioId,
                                  FBRTestScenario = fbr.Title
                              }).OrderByDescending(a => a.CreatedOn).ToListAsync();

            return list;
        }

        public async Task<object> GetICreditStatus()
        {
            var tempUser = _jwtService.DecodeToken();

            List<dynamic> list = new List<dynamic>();

            list.Add(new { StatusId = (int)EnumIDeliveryStatus.IDeliveryStatus.Draft, Title = "Draft" });
            list.Add(new { StatusId = (int)EnumIDeliveryStatus.IDeliveryStatus.Cancelled, Title = "Cancel" });
            list.Add(new { StatusId = (int)EnumIDeliveryStatus.IDeliveryStatus.Delivered, Title = "Approved" });

            return list;
        }


        public async Task<object> GetInvoiceDetailById(int invoiceId)
        {
            var tempUser = _jwtService.DecodeToken();

            var master = await (from inv in _db.Invoices
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
                                where inv.InvoiceId == invoiceId
                                      && inv.IsDeleted == 0
                                      && inv.TenantId == tempUser.TenantId
                                select new
                                {
                                    inv.InvoiceId,
                                    CustomerId = inv.CustId,
                                    CustomerName = cust.name,
                                    inv.StatusId,
                                    Status = stat.Title,
                                    inv.InvoiceDate,
                                    inv.InvoiceNumber,
                                    inv.TotalDiscount,
                                    inv.TotalTax,
                                    inv.OriginationId,
                                    OriginationName = origin.StateName,
                                    inv.DestinationId,
                                    inv.DeliveryId,
                                    inv.ReasonTypeId,
                                    ReasonNotes = inv.Notes,
                                    DestinationName = dest.StateName,
                                    inv.PaymentTerm,
                                    inv.CreatedOn,
                                    inv.FBRTestScenarioId,
                                    FBRTestScenario = fbr.Title
                                }).FirstOrDefaultAsync();

            if (master == null)
                return new { error = "Record not found" };


            // FETCH DETAILS
            var detailsRaw = await (from det in _db.InvoiceDetails
                                    join itm in _db.Products on det.ItemId equals itm.itemid
                                    where det.InvoiceId == invoiceId
                                    select new
                                    {
                                        det.InvoiceDtlId,
                                        det.InvoiceId,
                                        det.ItemId,
                                        ItemName = itm.name,
                                        ItemCode = itm.code,
                                        det.Price,
                                        det.Qty,
                                        det.Discount,
                                        det.SaleTax
                                    }).ToListAsync();


            // MANUAL CALCULATION APPLIED HERE
            var details = detailsRaw.Select(det =>
            {
                decimal A = det.Qty;
                decimal B = det.Price;
                decimal C = det.Discount;

                decimal D = (B * A) * C / 100m;
                decimal valueExclSt = (B * A) - D;

                decimal salesTax = Math.Round((valueExclSt * det.SaleTax) / 100m, 2);
                decimal valueIncTax = Math.Round(valueExclSt + salesTax, 2);

                return new
                {
                    det.InvoiceDtlId,
                    det.InvoiceId,
                    det.ItemId,
                    det.ItemName,
                    det.ItemCode,
                    det.Price,
                    det.Qty,
                    det.Discount,
                    det.SaleTax,

                    // 🔥 Newly Calculated
                    ValueExclST = Math.Round(valueExclSt, 2),
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

        public async Task<object> GetIReasonType()
        {
            var tempUser = _jwtService.DecodeToken();
            var res = await _db.IReasonTypes.Where(a => a.IsDeleted == 0).Select(s => new { s.ReasonTypeId, s.Title }).ToListAsync();
            return res;
        }

        public async Task<object> UpdateArchivedById(int invoiceId)
        {
            var tempUser = _jwtService.DecodeToken();
            var CreditNotes = await _db.Invoices.Where(a => a.IsDeleted == 1 && a.TenantId == tempUser.TenantId && a.ExtId != 0 && a.InvoiceId == invoiceId).FirstOrDefaultAsync();
            if (CreditNotes == null)
            {
                return new
                {
                    message = "Record not found!",
                    statusCode = 200,
                    success = false
                };
            }
            CreditNotes.IsDeleted = 0;
            await _db.SaveChangesAsync();
            return new
            {
                message = "Delete record successfully",
                statusCode = 200,
                success = true
            };
        }

    }
}
