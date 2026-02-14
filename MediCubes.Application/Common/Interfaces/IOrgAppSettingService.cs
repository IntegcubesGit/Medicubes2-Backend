using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;
using Domain.Entities;

namespace Application.Common.Interfaces
{
    public interface IOrgAppSettingService
    {
        Task<object> AddOrUpdate(OrgSettingDTO req);
        Task<object> GetAllCurrency();
        Task<object> GetOrgSetting();
    }
}
