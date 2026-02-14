using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Domain.DTOs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Repositories
{
    public class SaleDeliveryService : ISaleDeliveryService
    {
        private readonly AppDbContext _db;
        private readonly IJWTService _jwtService;

        public SaleDeliveryService(AppDbContext context, IJWTService jWTService)
        {
            _db = context;
            _jwtService = jWTService;
        }

        public async Task<object> GetSaleDetailById(int deliveryId)
        {
            var tempUser = _jwtService.DecodeToken();
            var master = await _db.SaleDeliveries.Where(a => a.DeliveryId == deliveryId && a.IsDeleted == 0 && a.TenantId == tempUser.TenantId).FirstOrDefaultAsync();
            if (master == null)
                return new { error = "Record not found" };

            var details = await _db.SaleDeliveryDetails.Where(d => d.SaleDeliveryId == deliveryId).ToListAsync();

            return new
            {
                master,
                details
            };
        }

        public async Task<object> GetAllSaleDelivery(SaleDeliveryFilter req)
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
            var list = await (from sale in _db.SaleDeliveries.Where(a => a.IsDeleted == 0 && a.TenantId == tempUser.TenantId && (req.statusId == -1 || a.DeliveryStatusId == req.statusId) && (req.custId == -1 || a.CustId == req.custId) && (a.CreatedOn >= startDateUtc && a.CreatedOn <= endDateUtc))
                        join cust in _db.Customers.Where(s => s.isdeleted == 0 && s.tenantid == tempUser.TenantId) on sale.CustId equals cust.customerid
                        join idelStatus in _db.IDeliveryStatuses.Where(a => a.IsDeleted == 0) on sale.DeliveryStatusId equals idelStatus.DeliveryStatusId
                        select new
                        {
                            sale.DeliveryId,
                            sale.DeliveryNumber,
                            sale.CustId,
                            sale.DeliveryDate,
                            sale.DeliveryStatusId,
                            sale.ShippingMethod,
                            sale.TrackingNumber,
                            sale.CreatedOn,
                            CustomerName = cust.name,
                            DeliveryStatus = idelStatus.Title
                        }).OrderByDescending(a => a.CreatedOn).ToListAsync();

            return list;
        }

        public async Task<object> AddSaleDelivery(SaleDeliveryDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var master = new SaleDelivery
                {
                    DeliveryNumber = await GenerateSaleDeliveryNumberAsync(),
                    CustId = dto.CustId,
                    DeliveryDate = dto.DeliveryDate.ToUniversalTime(),
                    ExpectedDeliveryDate = dto.ExpectedDeliveryDate.HasValue ? dto.ExpectedDeliveryDate.Value.ToUniversalTime(): (DateTime?)null,
                    DeliveryStatusId = dto.DeliveryStatusId,
                    ShippingAddress = dto.ShippingAddress,
                    ShippingMethod = dto.ShippingMethod,
                    TrackingNumber = dto.TrackingNumber,
                    ContactPerson = dto.ContactPerson,
                    ContactPhone = dto.ContactPhone,
                    Notes = dto.Notes,
                    TenantId = tempUser.TenantId,
                    CreatedBy = tempUser.UserId,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = 0
                };

                _db.SaleDeliveries.Add(master);
                await _db.SaveChangesAsync();

              
                if (dto.Details != null && dto.Details.Count > 0)
                {
                    foreach (var d in dto.Details)
                    {
                        var detail = new SaleDeliveryDetail
                        {
                            SaleDeliveryId = master.DeliveryId,
                            ItemId = d.ItemId,
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice,
                            TotalPrice = d.TotalPrice
                        };

                        _db.SaleDeliveryDetails.Add(detail);
                    }
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return new { success = true, message = "Sale Delivery Added Successfully", deliveryId = master.DeliveryId };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new { success = false, message = ex.Message };
            }
        }


        public async Task<object> GetIDeliveryStatus()
        {
            var tempUser = _jwtService.DecodeToken();
            var list = await _db.IDeliveryStatuses.Where(a => a.IsDeleted == 0).Select(a => new { a.DeliveryStatusId, a.Title }).ToListAsync();
            return list;
        }

        public async Task<object> UpdateSaleDelivery(SaleDeliveryDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var master = await _db.SaleDeliveries.FirstOrDefaultAsync(a => a.DeliveryId == dto.DeliveryId && a.TenantId == tempUser.TenantId && a.IsDeleted == 0);

                if (master == null)
                    return new { success = false, message = "Record not found!" };

                master.CustId = dto.CustId;
                master.DeliveryDate = dto.DeliveryDate.ToUniversalTime();
                master.ExpectedDeliveryDate = dto.ExpectedDeliveryDate.HasValue ? dto.ExpectedDeliveryDate.Value.ToUniversalTime() : (DateTime?)null;
                master.DeliveryStatusId = dto.DeliveryStatusId;
                master.ShippingAddress = dto.ShippingAddress;
                master.ShippingMethod = dto.ShippingMethod;
                master.TrackingNumber = dto.TrackingNumber;
                master.ContactPerson = dto.ContactPerson;
                master.ContactPhone = dto.ContactPhone;
                master.Notes = dto.Notes;
                master.ModifiedBy = tempUser.UserId;
                master.ModifiedOn = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                var oldDetails = _db.SaleDeliveryDetails.Where(d => d.SaleDeliveryId == master.DeliveryId);
                _db.SaleDeliveryDetails.RemoveRange(oldDetails);
                await _db.SaveChangesAsync();

                if (dto.Details != null)
                {
                    foreach (var d in dto.Details)
                    {
                        var detail = new SaleDeliveryDetail
                        {
                            SaleDeliveryId = master.DeliveryId,
                            ItemId = d.ItemId,
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice,
                            TotalPrice = d.TotalPrice
                        };
                        _db.SaleDeliveryDetails.Add(detail);
                    }

                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return new { success = true, message = "Sale Delivery Updated Successfully" };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new { success = false, message = ex.Message };
            }
        }

        public async Task<object> DeleteSale(int deliveryId)
        {
            var tempUser = _jwtService.DecodeToken();
            var existingItem = await _db.SaleDeliveries.FirstOrDefaultAsync(a => a.DeliveryId == deliveryId && a.IsDeleted == 0 && a.TenantId == tempUser.TenantId);

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
                message = "Sale delete successfully",
                data = existingItem
            };
        }

        public async Task<object> MarkAsDelivered(int deliveryId)
        {
            var tempUser = _jwtService.DecodeToken();
            var IsMarked = await _db.SaleDeliveries.Where(a => a.IsDeleted == 0 && a.TenantId == tempUser.TenantId && a.DeliveryStatusId == (int)EnumIDeliveryStatus.IDeliveryStatus.Shipped).FirstOrDefaultAsync();
            if (IsMarked == null)
            {
                return new
                {
                    success = false,
                    message = "record not found!"
                };
            }
            IsMarked.DeliveryStatusId = (int)EnumIDeliveryStatus.IDeliveryStatus.Delivered;
            await _db.SaveChangesAsync();
            return new
            {
                success = true,
                message = "Update Successfully"
            };
        }

        public async Task<object> GetArchivedList()
        {
            var tempUser = _jwtService.DecodeToken();

            var list = await (from sale in _db.SaleDeliveries.Where(a => a.IsDeleted == 1 && a.TenantId == tempUser.TenantId)
                              join cust in _db.Customers.Where(s => s.isdeleted == 0 && s.tenantid == tempUser.TenantId) on sale.CustId equals cust.customerid
                              join idelStatus in _db.IDeliveryStatuses.Where(a => a.IsDeleted == 0) on sale.DeliveryStatusId equals idelStatus.DeliveryStatusId
                              select new
                              {
                                  sale.DeliveryId,
                                  sale.DeliveryNumber,
                                  sale.CustId,
                                  sale.DeliveryDate,
                                  sale.DeliveryStatusId,
                                  sale.ShippingMethod,
                                  sale.TrackingNumber,
                                  sale.CreatedOn,
                                  CustomerName = cust.name,
                                  DeliveryStatus = idelStatus.Title
                              }).OrderByDescending(a => a.CreatedOn).ToListAsync();


            return list;
        }

        public async Task<object> UpdateArchivedById(int deliveryId)
        {
            var tempUser = _jwtService.DecodeToken();
            var existingItem = await _db.SaleDeliveries.FirstOrDefaultAsync(a => a.DeliveryId == deliveryId && a.IsDeleted == 1 && a.TenantId == tempUser.TenantId);

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
                message = "Restore record successfully",
                data = existingItem
            };
        }
        private async Task<string> GenerateSaleDeliveryNumberAsync()
        {
            var lastDeliveries = await _db.SaleDeliveries.OrderByDescending(x => x.DeliveryId).Select(x => x.DeliveryNumber).FirstOrDefaultAsync();

            int next = 1;

            if (!string.IsNullOrEmpty(lastDeliveries) && lastDeliveries.StartsWith("DEL-"))
            {
                var num = lastDeliveries.Replace("DEL-", "");
                int.TryParse(num, out next);
                next++;
            }

            return $"DEL-{next:D5}";
        }

    }
}
