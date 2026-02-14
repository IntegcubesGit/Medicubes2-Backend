using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class VendorService : IVendorService
    {
        private readonly AppDbContext _db;
        private readonly IJWTService _jwtService;

        public VendorService(AppDbContext context, IJWTService jWTService)
        {
            _db = context;
            _jwtService = jWTService;
        }
        public async Task<object> AddVendor(VendorDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();
            var IsVendorExist = await _db.Customers.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId && a.regno == dto.regno).FirstOrDefaultAsync();
            if (IsVendorExist != null)
            {
                return new
                {
                    success = false,
                    message = "Vendor already exist!"
                };
            }
            var entity = new Vendor
            {
                vendorname = dto.vendorname,
                vendortypeid = dto.vendortypeid,
                regno = dto.regno,
                vendoraddress = dto.vendoraddress,
                vendorphone = dto.vendorphone,
                vendoremail = dto.vendoremail,
                stateprovince = dto.stateprovince,
                createdby = tempUser.UserId,
                createdon = DateTime.UtcNow,
                tenantid = tempUser.TenantId,
                isdeleted = 0
            };

            await _db.Vendors.AddAsync(entity);
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Vendor added successfully",
                data = entity
            };
        }

        public async Task<object> GetAllVendor(int vendorTypeId, int StateTypeId)
        {
            var tempUser = _jwtService.DecodeToken();
            var res = await (from vend in _db.Vendors.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId && (vendorTypeId == -1 || a.vendortypeid == vendorTypeId) && (StateTypeId == -1 || a.stateprovince == StateTypeId))
                             join state in _db.IStateTypes.Where(a => a.IsDeleted == 0) on vend.stateprovince equals state.StateId
                             select new
                             {
                                 vendorId = vend.vendorid,
                                 vend.vendorname,
                                 vend.vendoraddress,
                                 vend.vendoremail,
                                 vend.vendortypeid,
                                 vend.stateprovince,
                                 vend.vendorphone,
                                 vend.regno,
                                 vend.createdon,
                                 state.StateName
                             }).OrderByDescending(a => a.createdon).ToListAsync();
            return res;
        }

        public async Task<object> GetVendorById(int VendorId)
        {
            var tempUser = _jwtService.DecodeToken();
            var res = await _db.Vendors.Where(a => a.vendorid == VendorId && a.isdeleted == 0 && a.tenantid == tempUser.TenantId).FirstOrDefaultAsync();
            if (res == null)
            {
                return new
                {
                    statusCode = 200,
                    message = "Data not Found"
                };
            }
            return res;
        }

        public async Task<object> UpdateVendor(VendorDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();

            var existingItem = await _db.Vendors.FirstOrDefaultAsync(a => a.vendorid == dto.vendorid && a.isdeleted == 0 && a.tenantid == tempUser.TenantId);

            if (existingItem == null)
            {
                return new
                {
                    success = false,
                    message = "Vendor not found"
                };
            }

            existingItem.vendorname = dto.vendorname;
            existingItem.vendortypeid = dto.vendortypeid;
            existingItem.regno = dto.regno;
            existingItem.vendoraddress = dto.vendoraddress;
            existingItem.vendorphone = dto.vendorphone;
            existingItem.vendoremail = dto.vendoremail;
            existingItem.stateprovince = dto.stateprovince;
            existingItem.modifiedby = tempUser.UserId;
            existingItem.modifiedon = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Vendor updated successfully",
                data = existingItem
            };
        }

        public async Task<object> DeleteVendor(int VendorId)
        {
            var tempUser = _jwtService.DecodeToken();

            var existingItem = await _db.Vendors.FirstOrDefaultAsync(a => a.vendorid == VendorId && a.isdeleted == 0 && a.tenantid == tempUser.TenantId);

            if (existingItem == null)
            {
                return new
                {
                    message = "Record not found!",
                    statusCode = 200,
                    success = false
                };
            }

            existingItem.isdeleted = 1;
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Vendor delete successfully",
                data = existingItem
            };
        }

        public async Task<object> GetVendorType()
        {
            var tempUser = _jwtService.DecodeToken();
            var list = await _db.IVendorTypes.Where(a => a.IsDeleted == 0).Select(a => new { a.VendorTypeId, a.VendorType }).ToListAsync();
            return list;
        }

        public async Task<object> GetStateType()
        {
            var tempUser = _jwtService.DecodeToken();
            var list = await _db.IStateTypes.Where(a => a.IsDeleted == 0).Select(a => new { a.StateId, a.StateName }).ToListAsync();
            return list;
        }

        public async Task<object> GetArchivedList()
        {
            var tempUser = _jwtService.DecodeToken();
            var res = await _db.Vendors.Where(a => a.isdeleted == 1 && a.tenantid == tempUser.TenantId).Select(a => new
            {
                a.vendorid,
                a.vendorname,
                a.vendoraddress,
                a.vendoremail,
                a.vendortypeid,
                a.stateprovince,
                a.vendorphone,
                a.regno,
                a.createdon
            }).OrderByDescending(a => a.createdon).ToListAsync();
            return res;
        }

        public async Task<object> UpdateArchivedById(int VendorId)
        {
            var tempUser = _jwtService.DecodeToken();

            var existingItem = await _db.Vendors.FirstOrDefaultAsync(a => a.vendorid == VendorId && a.isdeleted == 1 && a.tenantid == tempUser.TenantId);

            if (existingItem == null)
            {
                return new
                {
                    message = "Record not found!",
                    statusCode = 200,
                    success = false
                };
            }

            existingItem.isdeleted = 0;
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Vendor restore successfully",
                data = existingItem
            };
        }
    }
}
