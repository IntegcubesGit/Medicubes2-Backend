using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TaxConsolidationService : ITaxConsolidationService
    {
        private readonly AppDbContext _db;
        private readonly IJWTService _jwtService;

        public TaxConsolidationService(AppDbContext context, IJWTService jWTService)
        {
            _db = context;
            _jwtService = jWTService;
        }

        public async Task<object> GetInvoiceDetails(GetInvoiceDetailRequestDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();
            if (dto.CustomerIds == null || dto.CustomerIds.Length == 0)
                return new { success = false, message = "Customer ids not provided." };

            var fromDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
            var toDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);

            // Base invoice query
            var invoices = _db.Invoices
                .Where(inv =>
                    inv.IsDeleted == 0 &&
                    inv.TenantId == tempUser.TenantId &&
                    inv.StatusId != 7 && inv.StatusId != 8 &&
                    inv.ExtId == 0 &&
                    inv.CreatedOn.Date >= fromDate &&
                    inv.CreatedOn.Date <= toDate &&
                    dto.CustomerIds.Contains(inv.CustId));

            // Get invoice IDs first
            var invoiceIds = await invoices.Select(i => i.InvoiceId).ToListAsync();

            // Get invoice IDs that exist in AccTaxChallanDetail table
            var checkedInvoiceIds = await _db.AccTaxChallanDetail
                .Where(td => invoiceIds.Contains(td.InvoiceId))
                .Select(td => td.InvoiceId)
                .Distinct()
                .ToListAsync();

            // Create HashSet for faster lookup
            var checkedInvoiceIdsSet = new HashSet<int>(checkedInvoiceIds);

            // Group invoice details
            var invoiceDetailSummary = _db.InvoiceDetails
                .Where(d => invoiceIds.Contains(d.InvoiceId))
                .GroupBy(d => d.InvoiceId)
                .Select(g => new
                {
                    InvoiceId = g.Key,
                    TaxAmount = g.Sum(d =>
                        (((d.Price * d.Qty) - ((d.Price * d.Qty) * d.Discount / 100m)))
                        * d.SaleTax / 100m),
                    TaxableAmount = g.Sum(d => ((d.Price * d.Qty) - ((d.Price * d.Qty) * d.Discount / 100m)) +
                                              (((d.Price * d.Qty) - ((d.Price * d.Qty) * d.Discount / 100m)) * d.SaleTax / 100m))
                })
                .ToList(); // Materialize here

            // Fetch related data
            var tenantId = tempUser.TenantId;
            var customers = await _db.Customers
                .Where(c => c.isdeleted == 0 && c.tenantid == tenantId)
                .ToListAsync();

            var origins = await _db.IStateTypes
                .Where(s => s.IsDeleted == 0)
                .ToListAsync();

            var destinations = await _db.IStateTypes
                .Where(s => s.IsDeleted == 0)
                .ToListAsync();

            var statuses = await _db.InvoiceStatuses
                .Where(s => s.IsDeleted == 0)
                .ToListAsync();

            // Execute the invoice query
            var invoiceList = await invoices.ToListAsync();

            // Join in memory to avoid null reference issues
            var result = (from inv in invoiceList
                          join det in invoiceDetailSummary
                               on inv.InvoiceId equals det.InvoiceId into detJoined
                          from det in detJoined.DefaultIfEmpty()
                          join cust in customers
                               on inv.CustId equals cust.customerid
                          join origin in origins
                               on inv.OriginationId equals origin.StateId
                          join dest in destinations
                               on inv.DestinationId equals dest.StateId
                          join stat in statuses
                               on inv.StatusId equals stat.InvoiceStatusId
                          orderby inv.CreatedOn descending
                          select new
                          {
                              inv.InvoiceId,
                              CustomerId = inv.CustId,
                              CustomerName = cust?.name ?? "Unknown",
                              inv.InvoiceDate,
                              inv.StatusId,
                              Status = stat?.Title ?? "Unknown",
                              inv.OriginationId,
                              OriginationName = origin?.StateName ?? "Unknown",
                              inv.DestinationId,
                              DestinationName = dest?.StateName ?? "Unknown",
                              TaxAmount = det != null ? det.TaxAmount : 0,
                              TaxableAmount = det != null ? det.TaxableAmount : 0,
                              IsChecked = checkedInvoiceIdsSet.Contains(inv.InvoiceId)
                          })
                .ToList();

            return result;
        }

        public async Task<object> AddChallanDetails(List<ChallanRequestDTO> dtos)
        {
            var tempUser = _jwtService.DecodeToken();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Validate all items first
                var validationErrors = new List<object>();
                var validItems = new List<ChallanRequestDTO>();

                foreach (var dto in dtos)
                {
                    // Check if invoice exists
                    var invoice = await _db.Invoices
                        .Where(a => a.IsDeleted == 0
                            && a.TenantId == tempUser.TenantId
                            && a.InvoiceId == dto.InvoiceId).SingleOrDefaultAsync();

                    if (invoice == null)
                    {
                        validationErrors.Add(new
                        {
                            success = false,
                            message = $"Invoice with ID {dto.InvoiceId} does not exist",
                        });
                        continue;
                    }

                    // Check if invoice already has a challan
                    var invoiceHasChallan = await _db.AccTaxChallanDetail
                        .AnyAsync(a => a.InvoiceId == dto.InvoiceId
                            && _db.AccTaxChallan.Any(c => c.TaxChallanId == a.TaxChallanId
                                && c.IsDeleted == 0
                                && c.TenantId == tempUser.TenantId));

                    if (invoiceHasChallan && invoice.StatusId != 8 && invoice.StatusId !=7)
                    {
                        validationErrors.Add(new
                        {
                            success = false,
                            message = $"Invoice ID {dto.InvoiceId} already has a tax challan",
                        });
                        continue;
                    }

                    validItems.Add(dto);
                }

                // If any validation errors, return them
                if (validationErrors.Any())
                {
                    return new
                    {
                        success = false,
                        message = "Validation failed for some items",
                    };
                }

                if (dtos[0].TaxChallanId == -1)
                {
                    var challan = new AccTaxChallan
                    {
                        FromDate = dtos[0].FromDate.ToUniversalTime(),
                        ToDate = dtos[0].ToDate.ToUniversalTime(),
                        RefNumber = dtos[0].RefNumber ?? "N/A",
                        PSId = dtos[0].PSID ?? "N/A",
                        CPRNNo = null,
                        TenantId = tempUser.TenantId,
                        CreatedBy = tempUser.UserId,
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = 0,
                    };

                    _db.AccTaxChallan.Add(challan);
                    await _db.SaveChangesAsync();

                    // Process all valid items
                    var results = new List<object>();

                    foreach (var dto in validItems)
                    {
                        var invoice = await _db.Invoices
                            .Where(x => x.InvoiceId == dto.InvoiceId && x.IsDeleted == 0 && x.TenantId == tempUser.TenantId)
                            .SingleOrDefaultAsync();

                        var challanDetail = new AccTaxChallanDetail
                        {
                            TaxChallanId = challan.TaxChallanId,
                            InvoiceId = dto.InvoiceId,
                            TaxAmount = dto.TaxAmount,
                            TaxableAmount = dto.TaxableAmount,
                            CreatedBy = tempUser.UserId,
                            CreatedOn = DateTime.UtcNow
                        };

                        _db.AccTaxChallanDetail.Add(challanDetail);

                        results.Add(new
                        {
                            success = true,
                            message = $"Tax challan added successfully",
                        });

                        if (invoice != null)
                        {
                            invoice.StatusId = 8;
                            _db.Invoices.Update(invoice);
                        }
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new
                    {
                        success = true,
                        message = $"Successfully added {results.Count} challan details",
                    };
                }
                else
                {
                    // Update existing tax challan
                    //var challanToRemove = await _db.AccTaxChallan
                    //    .Where(c => c.TaxChallanId == dtos[0].TaxChallanId
                    //        && c.IsDeleted == 0
                    //        && c.TenantId == tempUser.TenantId)
                    //    .SingleOrDefaultAsync();



                    //if (challanToRemove != null)
                    //{
                    //    _db.AccTaxChallan.Remove(challanToRemove);
                    //    await _db.SaveChangesAsync();

                    var challanDetailsToRemove = await _db.AccTaxChallanDetail
                        .Where(d => d.TaxChallanId == dtos[0].TaxChallanId)
                        .ToListAsync();

                    var invoiceIds = challanDetailsToRemove
                                        .Select(d => d.InvoiceId)
                                        .Distinct()
                                        .ToList();

                        if (invoiceIds.Any())
                        {
                            // Option 1: Update invoices status in a single database call
                            var invoicesToUpdate = await _db.Invoices
                                .Where(i => invoiceIds.Contains(i.InvoiceId))
                                .ToListAsync();

                            foreach (var invoice in invoicesToUpdate)
                            {
                                invoice.StatusId = 4;
                                invoice.ModifiedOn = DateTime.UtcNow; 
                                invoice.ModifiedBy = tempUser.UserId;   
                            }

                            await _db.SaveChangesAsync();
                        }

                        if (challanDetailsToRemove.Any())
                        {
                            _db.AccTaxChallanDetail.RemoveRange(challanDetailsToRemove);
                            await _db.SaveChangesAsync();
                        }
                    //}

                    //var challan = new AccTaxChallan
                    //{
                    //    FromDate = dtos[0].FromDate.ToUniversalTime(),
                    //    ToDate = dtos[0].ToDate.ToUniversalTime(),
                    //    RefNumber = dtos[0].RefNumber ?? "N/A",
                    //    PSId = dtos[0].PSID ?? "N/A",
                    //    CPRNNo = null,
                    //    TenantId = tempUser.TenantId,
                    //    CreatedBy = tempUser.UserId,
                    //    CreatedOn = DateTime.UtcNow,
                    //    IsDeleted = 0,
                    //};

                    //_db.AccTaxChallan.Add(challan);
                    //await _db.SaveChangesAsync();

                    // Process all valid items
                    var results = new List<object>();

                    foreach (var dto in validItems)
                    {
                        var invoice = await _db.Invoices
                            .Where(x => x.InvoiceId == dto.InvoiceId && x.IsDeleted == 0 && x.TenantId == tempUser.TenantId)
                            .SingleOrDefaultAsync();

                        var challanDetail = new AccTaxChallanDetail
                        {
                            TaxChallanId = dtos[0].TaxChallanId,
                            InvoiceId = dto.InvoiceId,
                            TaxAmount = dto.TaxAmount,
                            TaxableAmount = dto.TaxableAmount,
                            CreatedBy = tempUser.UserId,
                            CreatedOn = DateTime.UtcNow
                        };

                        _db.AccTaxChallanDetail.Add(challanDetail);

                        results.Add(new
                        {
                            success = true,
                            message = $"Tax challan added successfully",
                        });

                        if (invoice != null)
                        {
                            invoice.StatusId = 8;
                            _db.Invoices.Update(invoice);
                        }
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new
                    {
                        success = true,
                        message = $"Successfully added {results.Count} challan details",
                    };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message,
                };
            }
        }

        public async Task<object> GetTaxConsolidationList()
        {
            var tempUser = _jwtService.DecodeToken();

            var result = await (from challan in _db.AccTaxChallan
                                .Where(c => c.IsDeleted == 0 && c.TenantId == tempUser.TenantId)
                                join user in _db.Users
                                    .Where(u => u.TenantId == tempUser.TenantId)
                                    on challan.CreatedBy equals user.Id
                                select new
                                {
                                    challan.TaxChallanId,
                                    RefNo = challan.RefNumber,
                                    challan.CreatedOn,
                                    AddedBy = user.UserName,
                                    Status = challan.CPRNNo != null ? "Paid" : "Pending",
                                    challan.CPRNNo,
                                    challan.PSId
                                }).ToListAsync();

            return result;
        }

        public async Task<object> UpdateTaxConsolidationRecord(UpdateTaxConsolidationRecsRequestDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();

            var challan = await _db.AccTaxChallan.AsNoTracking()
                .SingleOrDefaultAsync(c => c.TaxChallanId == dto.TaxChallanId
                    && c.IsDeleted == 0
                    && c.TenantId == tempUser.TenantId);

            var invoiceIds = await (
                                  from tax in _db.AccTaxChallan
                                      .Where(d => d.TaxChallanId == dto.TaxChallanId && d.IsDeleted == 0)
                                  join dtl in _db.AccTaxChallanDetail
                                      on tax.TaxChallanId equals dtl.TaxChallanId
                                  select dtl.InvoiceId
                              ).Distinct().ToListAsync();

            if (invoiceIds.Any())
            {
                // Update invoices using ExecuteUpdate
                await _db.Invoices
                    .Where(i => invoiceIds.Contains(i.InvoiceId) && i.IsDeleted == 0)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(i => i.StatusId, 7)
                        .SetProperty(i => i.ModifiedOn, DateTime.UtcNow)
                        .SetProperty(i => i.ModifiedBy, tempUser.UserId) 
                    );
            }


            if (challan == null)
            {
                return new
                {
                    success = false,
                    message = "Tax challan not found.",
                };
            }

            challan.PaymentDate = dto.PaymentDate.ToUniversalTime();
            challan.CPRNNo = dto.CPRNNo;
            challan.ChequeNo = dto.ChequeNo;
            challan.PaidRemarks = dto.PaidRemarks;
            challan.ModifiedBy = tempUser.UserId;
            challan.ModifiedOn = DateTime.UtcNow;


            _db.AccTaxChallan.Update(challan);

            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Tax challan updated successfully.",
            };
        }

        public async Task<object> GetTaxConsolidationByTaxChallanId(int TaxChallanId)
        {
            var tempUser = _jwtService.DecodeToken();

            var challan = await _db.AccTaxChallan.AsNoTracking()
                                                    .SingleOrDefaultAsync(c => c.TaxChallanId == TaxChallanId
                                                        && c.IsDeleted == 0
                                                        && c.TenantId == tempUser.TenantId);

            if (challan == null)
            {
                return new
                {
                    success = false,
                    message = "Tax challan not found.",
                };
            }

            return challan;
        }

        public async Task<object> GetTaxConsolidationDetailsByTaxChallanId(int TaxChallanId)
        {
            var tempUser = _jwtService.DecodeToken();

            // Get tax challan details with start and end dates and invoice IDs
            var taxChallanDetails = await (
                from tax in _db.AccTaxChallan
                    .Where(d => d.TaxChallanId == TaxChallanId && d.IsDeleted == 0)
                join dtl in _db.AccTaxChallanDetail
                    on tax.TaxChallanId equals dtl.TaxChallanId
                select new
                {
                    tax.FromDate,
                    tax.ToDate,
                    tax.PSId,
                    tax.RefNumber,
                    dtl.InvoiceId
                }
            ).ToListAsync();

            if (!taxChallanDetails.Any())
            {
                return new { Message = "Tax challan not found" };
            }

            // Get unique invoice IDs from tax challan details
            var invoiceIdsFromTaxChallan = taxChallanDetails
                .Select(d => d.InvoiceId)
                .Distinct()
                .ToList();

            // Get customer IDs from invoices using the invoice IDs from tax challan
            var customerIdsFromInvoices = await _db.Invoices
                .Where(inv =>
                    inv.IsDeleted == 0 &&
                    inv.TenantId == tempUser.TenantId &&
                    invoiceIdsFromTaxChallan.Contains(inv.InvoiceId))
                .Select(inv => inv.CustId)
                .Distinct()
                .ToListAsync();

            // Use the dates from the first tax challan record
            var taxChallan = taxChallanDetails.First();
            var fromDate = DateTime.SpecifyKind(taxChallan.FromDate, DateTimeKind.Utc);
            var toDate = DateTime.SpecifyKind(taxChallan.ToDate, DateTimeKind.Utc);
            var PSID = taxChallan.PSId;
            var RefNumber = taxChallan.RefNumber;

            // Base invoice query using customer IDs from invoices
            var invoices = _db.Invoices
                .Where(inv =>
                    inv.IsDeleted == 0 &&
                    inv.TenantId == tempUser.TenantId &&
                    inv.StatusId != 7 &&
                    inv.ExtId == 0 &&
                    inv.CreatedOn.Date >= fromDate &&
                    inv.CreatedOn.Date <= toDate &&
                    customerIdsFromInvoices.Contains(inv.CustId));

            // Get invoice IDs from the filtered invoices
            var invoiceIds = await invoices.Select(i => i.InvoiceId).ToListAsync();

            // Group invoice details for tax calculations
            var invoiceDetailSummary = _db.InvoiceDetails
                .Where(d => invoiceIds.Contains(d.InvoiceId))
                .GroupBy(d => d.InvoiceId)
                .Select(g => new
                {
                    InvoiceId = g.Key,
                    TaxAmount = g.Sum(d =>
                        (((d.Price * d.Qty) - ((d.Price * d.Qty) * d.Discount / 100m)))
                        * d.SaleTax / 100m),
                    TaxableAmount = g.Sum(d => ((d.Price * d.Qty) - ((d.Price * d.Qty) * d.Discount / 100m)) +
                                              (((d.Price * d.Qty) - ((d.Price * d.Qty) * d.Discount / 100m)) * d.SaleTax / 100m))
                })
                .ToList(); // Materialize here

            // Fetch related data
            var tenantId = tempUser.TenantId;
            var customers = await _db.Customers
                .Where(c => c.isdeleted == 0 && c.tenantid == tenantId)
                .ToListAsync();

            var origins = await _db.IStateTypes
                .Where(s => s.IsDeleted == 0)
                .ToListAsync();

            var destinations = await _db.IStateTypes
                .Where(s => s.IsDeleted == 0)
                .ToListAsync();

            var statuses = await _db.InvoiceStatuses
                .Where(s => s.IsDeleted == 0)
                .ToListAsync();

            // Execute the invoice query
            var invoiceList = await invoices.ToListAsync();

            // Create a HashSet for quick lookup of invoice IDs from tax challan
            var taxChallanInvoiceIdsSet = new HashSet<int>(invoiceIdsFromTaxChallan);

            // Join in memory to avoid null reference issues
            var result = (from inv in invoiceList
                          join det in invoiceDetailSummary
                               on inv.InvoiceId equals det.InvoiceId into detJoined
                          from det in detJoined.DefaultIfEmpty()
                          join cust in customers
                               on inv.CustId equals cust.customerid
                          join origin in origins
                               on inv.OriginationId equals origin.StateId
                          join dest in destinations
                               on inv.DestinationId equals dest.StateId
                          join stat in statuses
                               on inv.StatusId equals stat.InvoiceStatusId
                          orderby inv.CreatedOn descending
                          select new
                          {
                              inv.InvoiceId,
                              CustomerId = inv.CustId,
                              CustomerName = cust?.name ?? "Unknown",
                              inv.InvoiceDate,
                              inv.StatusId,
                              Status = stat?.Title ?? "Unknown",
                              inv.OriginationId,
                              OriginationName = origin?.StateName ?? "Unknown",
                              inv.DestinationId,
                              DestinationName = dest?.StateName ?? "Unknown",
                              TaxAmount = det != null ? det.TaxAmount : 0,
                              TaxableAmount = det != null ? det.TaxableAmount : 0,
                              IsChecked = taxChallanInvoiceIdsSet.Contains(inv.InvoiceId)
                          })
                .ToList();

            // Return result with dates, customer IDs, and all invoice data
            return new
            {
                StartDate = fromDate,
                EndDate = toDate,
                PSID,
                RefNumber,
                CustomerIds = customerIdsFromInvoices,
                Invoices = result
            };
        }

        public async Task<object> ArchiveTaxConsolidationByTaxChallanId(int TaxChallanId)
        {
            var tempUser = _jwtService.DecodeToken();
            var challan = await _db.AccTaxChallan
                .Where(c => c.TaxChallanId == TaxChallanId
                    && c.IsDeleted == 0
                    && c.TenantId == tempUser.TenantId)
                .SingleOrDefaultAsync();

            if (challan == null)
            {
                return new
                {
                    success = false,
                    message = "Tax challan not found.",
                };
            }

            challan.IsDeleted = 1;
            challan.ModifiedBy = tempUser.UserId;
            challan.ModifiedOn = DateTime.UtcNow;

            _db.AccTaxChallan.Update(challan);
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Tax challan archived successfully.",
            };
        }

        public async Task<object> UnArchiveTaxConsolidationByTaxChallanId(int TaxChallanId)
        {
            var tempUser = _jwtService.DecodeToken();
            var challan = await _db.AccTaxChallan
                .Where(c => c.TaxChallanId == TaxChallanId
                    && c.IsDeleted == 1
                    && c.TenantId == tempUser.TenantId)
                .SingleOrDefaultAsync();

            if (challan == null)
            {
                return new
                {
                    success = false,
                    message = "Tax challan not found.",
                };
            }

            challan.IsDeleted = 0;
            challan.ModifiedBy = tempUser.UserId;
            challan.ModifiedOn = DateTime.UtcNow;

            _db.AccTaxChallan.Update(challan);
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Tax challan un-archived successfully.",
            };
        }

        public async Task<object> GetAllArchivedTaxConsolidationList()
        {
            var tempUser = _jwtService.DecodeToken();

            var result = await (from challan in _db.AccTaxChallan
                              .Where(c => c.IsDeleted == 1 && c.TenantId == tempUser.TenantId)
                                join user in _db.Users
                                    .Where(u => u.TenantId == tempUser.TenantId)
                                    on challan.CreatedBy equals user.Id
                                select new
                                {
                                    challan.TaxChallanId,
                                    RefNo = challan.RefNumber,
                                    challan.CreatedOn,
                                    AddedBy = user.UserName,
                                    Status = challan.CPRNNo != null ? "Paid" : "Pending",
                                    challan.CPRNNo,
                                    challan.PSId
                                }).ToListAsync();

            return result;
        }
    }
}
