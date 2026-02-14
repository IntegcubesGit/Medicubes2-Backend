using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Application.Common.Interfaces
{
    public interface IUserService
    {
        Task<object> RegisterUser(RegisterOrUpdateUserRequestDTO registerUserRequest, List<int> roles);
        Task<object> UpdateUser(RegisterOrUpdateUserRequestDTO updateUserRequest, List<int> roles);
        Task<object> DeleteUser(int userId);
        Task<object> GetUserInformationByParsingJWT();
        Task<object> GetAllUsers(int pageNumber, int pageSize, string sort, string order, string? search);
        Task<object> GetRoles();
        Task<object> GetUserById(int userId);
        Task<object> GetUserInfoById(int userId);
        Task<object> GetOrgLocationsById(int userId);
    }
}
