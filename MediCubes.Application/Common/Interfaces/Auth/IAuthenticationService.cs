using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;
using Domain.Entities;

namespace Application.Common.Interfaces.Auth
{
    public interface IAuthenticationService
    {
        Task<object> LoginAsync(string username, string password);
        //Task<object> RegisterUserAsync(RegisterUserDTO appUser);
        Task<object> Logout();
    }
}
