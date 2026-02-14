using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.DTOs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Repositories
{
    public class OrgInfoService : IOrgInfoService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> userManager;

        public OrgInfoService(AppDbContext db, UserManager<AppUser> _userManager)
        {
            _db = db;
            userManager = _userManager;
        }

        public async Task<object> AddTenantRegistration(OrgInfoDTO req)
        {
            if (req != null)
            {
                OrgInfo tenant = new OrgInfo();
                tenant.Code = req.Code;
                tenant.Name = req.Name;
                tenant.StatusId = 0;
                tenant.CreatedAt = DateTime.Now;
                await _db.OrgInfos.AddAsync(tenant);
                await _db.SaveChangesAsync();




                if (!string.IsNullOrWhiteSpace(req.Email))
                {
                    var existingEmail = await userManager.FindByEmailAsync(req.Email);
                    if (existingEmail != null)
                    {
                        return new { message = "Email is already in use!", IsSucceeded = 0 };
                    }
                }

                var existingName = await userManager.FindByNameAsync(req.Username);
                if (existingName != null)
                {
                    return new { message = "Username is already in use!", IsSucceeded = 0 };
                }

                // Create new user
                var user = new AppUser
                {
                    Name = req.FirstName + " " + req.LastName,
                    UserName = req.Username,
                    NormalizedUserName = req.Username?.ToUpper(),
                    Email = req.Email,
                    NormalizedEmail = req.Email?.ToUpper(),
                    // PhoneNumber = req.Phone,
                    PlainTextPassword = req.Password,
                    OrgId = tenant.OrgId,
                    CreatedBy = 1,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = 0,
                    CanSignRpt = req.CanSignRpt == 0 ? false : true,
                    ShowNameOnRpt = req.ShowNameOnRpt,
                    StaffId = req.RelevantStaffId,
                    DiscountLimit = req.DiscountLimit,
                    MinCash = req.MinCash,
                    EmpId = req.EmpNo,
                    Qualification = req.Qualification,
                    ReportNote = req.ReportNote,
                    RegLocId = req.RegLocId,
                    OpdCounterId = 0,
                    StoreId = -1,
                    ShareOn = 0,
                    OutLetId = 0,
                    UserStamp = !string.IsNullOrEmpty(req.DoctorStamp) ? Convert.FromBase64String(req.DoctorStamp) : null
                };

                // Create user with password
                var result = await userManager.CreateAsync(user, req.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new { message = errors, IsSucceeded = 0 };
                }
                var data = new OrgAppSetting
                {
                    OrganizationName = req.Name,
                    Website = null,
                    Address = null,
                    City = null,
                    StateId = null,
                    Country = null,
                    PostalZipCode = null,
                    TaxId = null,
                    RegistrationNum = null,
                    CurrencyId = (int)EnumOrgInfo.ICurrency.PKR,
                    OrganizationLogo = null,
                    OrgId = tenant.OrgId,
                    CreatedBy = 1,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = 0
                };

                _db.OrgAppSettings.Add(data);
                await _db.SaveChangesAsync();

                return new
                {
                    success = true,
                    statusCode = 200,
                    message = "Request Submit successfully"
                };

            }
            return new
            {
                success = false,
                statusCode = 200,
                message = "Payload is not correct!."
            };
        }
    }
}
