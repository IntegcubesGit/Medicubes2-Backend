using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Application.Common.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<MenuDTO>> GetAllMenusListForCreatingRoles();
        Task<object> CreateRole(CreateOrUpdateRoleDTO createRoleRequest);
        Task<object> UpdateRole(CreateOrUpdateRoleDTO updateRoleRequest);
        Task<object> GetRolesList();
        Task<object> GetRoleById(int RoleId);
        Task<object> DeleteRoleById(int RoleId);

    }
}
