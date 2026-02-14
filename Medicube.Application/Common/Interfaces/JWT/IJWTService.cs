using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Application.Common.Interfaces.JWT
{
    public interface IJWTService
    {
        string GenerateToken(AppUser app, IConfiguration config, out DateTime expiry);
        TempUser DecodeToken();
        string GetTokenFromHeader();
        bool IsTokenValid(string token, IConfiguration config);
        Task<bool> DeleteToken(string token);
    }
}
