using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Application.Common.Interfaces
{
    public interface IOrgInfoService
    {
        Task<object> AddTenantRegistration(OrgInfoDTO registerUserRequest);

    }
}
