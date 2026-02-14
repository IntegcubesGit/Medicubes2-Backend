using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OrgAppSettingService : IOrgAppSettingService
    {
        private readonly ICurrentUser _currentUser;
        private readonly AppDbContext _db;

        public OrgAppSettingService(ICurrentUser currentUser, AppDbContext appDbContext)
        {
            _currentUser = currentUser;
            this._db = appDbContext;
        }

        public async Task<object> AddOrUpdate(OrgSettingDTO req)
        {
            var user = _currentUser.GetCurrentUser();

            var existing = await _db.OrgAppSettings.FirstOrDefaultAsync(x => x.OrgId == user.OrgId && x.IsDeleted == 0);

            if (existing != null)
            {
                existing.OrganizationName = req.OrganizationName;
                existing.Website = req.Website;
                existing.Address = req.Address;
                existing.City = req.City;
                existing.StateId = req.StateId;
                existing.Country = req.Country;
                existing.PostalZipCode = req.PostalZipCode;
                existing.TaxId = req.TaxId;
                existing.RegistrationNum = req.RegistrationNum;
                existing.CurrencyId = req.CurrencyId;
                existing.OrganizationLogo = req.OrganizationLogo;
                existing.FbrSandboxToken = req.FbrSandboxToken;
                existing.FbrProductionToken = req.FbrProductionToken;
                existing.InvoiceColor = req.InvoiceColor;
                existing.ModifiedBy = user.UserId;
                existing.ModifiedOn = DateTime.UtcNow;
                existing.isProduction = req.IsProduction;

                await _db.SaveChangesAsync();
                return new { message = "Organization settings updated successfully", settingId = existing.SettingId };
            }
            else
            {
                var data = new OrgAppSetting
                {
                    OrganizationName = req.OrganizationName,
                    Website = req.Website,
                    Address = req.Address,
                    City = req.City,
                    StateId = req.StateId,
                    Country = req.Country,
                    PostalZipCode = req.PostalZipCode,
                    TaxId = req.TaxId,
                    RegistrationNum = req.RegistrationNum,
                    CurrencyId = req.CurrencyId,
                    OrganizationLogo = req.OrganizationLogo,
                    FbrSandboxToken = req.FbrSandboxToken,
                    FbrProductionToken = req.FbrProductionToken,
                    InvoiceColor = req.InvoiceColor,
                    OrgId = user.OrgId,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = 0,
                    isProduction = req.IsProduction
                };

                _db.OrgAppSettings.Add(data);
                await _db.SaveChangesAsync();

                return new
                {
                    message = "Organization settings added successfully",
                    success = true,
                    statusCode = 200,
                    settingId = data.SettingId
                };
            }
        }

        public async Task<object> GetAllCurrency()
        {
            _ = _currentUser.GetCurrentUser();
            var data = await _db.ICurrencies.Where(a => a.IsDeleted == 0).Select(s => new
            {
                s.CurrencyId,
                CurrencyName = s.Title
            }).ToListAsync();
            return data;
        }

        public async Task<object> GetOrgSetting()
        {
            var tempUser = _currentUser.GetCurrentUser();
            var res = await (from org in _db.OrgAppSettings.Where(a => a.IsDeleted == 0 && a.OrgId == tempUser.OrgId)
                             join curr in _db.ICurrencies.Where(a => a.IsDeleted == 0) on org.CurrencyId equals curr.CurrencyId
                             select new
                             {
                                 org.SettingId,
                                 org.OrganizationName,
                                 org.Website,
                                 org.Address,
                                 org.City,
                                 org.StateId,
                                 org.Country,
                                 org.PostalZipCode,
                                 org.TaxId,
                                 org.RegistrationNum,
                                 org.CurrencyId,
                                 org.OrganizationLogo,
                                 org.FbrSandboxToken,
                                 org.FbrProductionToken,
                                 CurrencyName = curr.Title,
                                 org.InvoiceColor,
                                 org.isProduction
                             }).ToListAsync();
            return res;
        }
    }
}
