using Application.Common.Interfaces.Customers;
using Application.Common.Interfaces.JWT;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _db;
        private readonly IJWTService _jwtService;

        public CustomerService(AppDbContext context, IJWTService jWTService)
        {
            _db = context;
            _jwtService = jWTService;
        }
        public async Task<object> AddCustomer(CustomerDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();
            var IsCustomerExist = await _db.Customers.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId && a.regno == dto.regno).FirstOrDefaultAsync();
            if (IsCustomerExist != null)
            {
                return new
                {
                    success = false,
                    message = "Customer already exist!"
                };
            }
            var entity = new Customer
            {
                name = dto.name,
                address = dto.address,
                email = dto.email,
                phone = dto.phone,
                regno = dto.regno,
                customertypeid = dto.customertypeid,
                custregtypeid = dto.custregtypeid,
                stateprovince = dto.stateprovince,
                createdby = tempUser.UserId,
                createdon = DateTime.UtcNow,
                tenantid = tempUser.TenantId,
                cnic = dto.cnic,
                isdeleted = 0
            };

            await _db.Customers.AddAsync(entity);
            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Customer added successfully",
                data = entity
            };
        }

        public async Task<object> GetAllCustomer(int custRegId, int custTypeId)
        {
            var tempUser = _jwtService.DecodeToken();
            var res = await (from cust in _db.Customers.Where(a => a.isdeleted == 0 && a.tenantid == tempUser.TenantId && (custRegId == -1 || a.custregtypeid == custRegId) && (custTypeId == -1 || a.customertypeid == custTypeId))
                             join ctype in _db.ICustomerTypes.Where(a => a.IsDeleted == 0) on cust.customertypeid equals ctype.CustTypeId
                             join state in _db.IStateTypes.Where(a => a.IsDeleted == 0) on cust.stateprovince equals state.StateId
                             select new
                             {
                                 cust.customerid,
                                 cust.name,
                                 cust.address,
                                 cust.email,
                                 cust.customertypeid,
                                 customerType = ctype.CustType,
                                 cust.custregtypeid,
                                 stateProvinceId = cust.stateprovince,
                                 state.StateName,
                                 cust.phone,
                                 cust.cnic,
                                 cust.regno,
                                 cust.createdon
                             }).OrderByDescending(a => a.createdon).ToListAsync();
            return res;
        }

        public async Task<object> GetCustomerById(int CustId)
        {
            var tempUser = _jwtService.DecodeToken();
            var res = await _db.Customers.Where(a => a.customerid == CustId && a.isdeleted == 0 && a.tenantid == tempUser.TenantId).FirstOrDefaultAsync();
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

        public async Task<object> UpdateCustomer(CustomerDTO dto)
        {
            var tempUser = _jwtService.DecodeToken();

            var existingItem = await _db.Customers.FirstOrDefaultAsync(a => a.customerid == dto.customerid && a.isdeleted == 0 && a.tenantid == tempUser.TenantId);

            if (existingItem == null)
            {
                return new
                {
                    message = "Record not found!",
                    statusCode = 200,
                    success = false
                };
            }

            existingItem.name = dto.name;
            existingItem.address = dto.address;
            existingItem.email = dto.email;
            existingItem.phone = dto.phone;
            existingItem.customertypeid = dto.customertypeid;
            existingItem.custregtypeid = dto.custregtypeid;
            existingItem.regno = dto.regno;
            existingItem.stateprovince = dto.stateprovince;
            existingItem.modifiedon = DateTime.UtcNow;
            existingItem.modifiedby = tempUser.UserId;
            existingItem.cnic = dto.cnic;
            existingItem.tenantid = tempUser.TenantId;
            existingItem.isdeleted = 0;

            await _db.SaveChangesAsync();

            return new
            {
                success = true,
                message = "Customer updated successfully",
                data = existingItem
            };
        }

        public async Task<object> DeleteCustomer(int CustId)
        {
            var tempUser = _jwtService.DecodeToken();

            var existingItem = await _db.Customers.FirstOrDefaultAsync(a => a.customerid == CustId && a.isdeleted == 0 && a.tenantid == tempUser.TenantId);

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
                message = "Customer delete successfully",
                data = existingItem
            };
        }

        public async Task<object> GetCustRegType()
        {
            var tempUser = _jwtService.DecodeToken();
            return await _db.ICustRegTypes.Where(a => a.IsDeleted == 0).Select(a => new { a.RegTypeId, a.CustRegName }).ToListAsync();
        }

        public async Task<object> GetCustomerType()
        {
            var tempUser = _jwtService.DecodeToken();
            return await _db.ICustomerTypes.Where(a => a.IsDeleted == 0).Select(a => new { a.CustTypeId, a.CustType }).ToListAsync();
        }

        public async Task<object> GetArchivedList()
        {
            var tempUser = _jwtService.DecodeToken();
            var res = await _db.Customers.Where(a => a.isdeleted == 1 && a.tenantid == tempUser.TenantId).Select(a => new
            {
                a.customerid,
                a.name,
                a.address,
                a.email,
                a.customertypeid,
                a.custregtypeid,
                a.stateprovince,
                a.phone,
                a.regno,
                a.createdon,
                a.cnic
            }).OrderByDescending(s => s.createdon).ToListAsync();
            return res;
        }

        public async Task<object> UpdateArchivedById(int CustId)
        {
            var tempUser = _jwtService.DecodeToken();

            var existingItem = await _db.Customers.FirstOrDefaultAsync(a => a.customerid == CustId && a.isdeleted == 1 && a.tenantid == tempUser.TenantId);

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
                message = "Customer restore successfully",
                data = existingItem
            };
        }
    }
}
