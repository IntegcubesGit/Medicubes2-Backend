using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.JWT;
using Azure.Core;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;
        private readonly AuthDbContext _authContext;
        private readonly AppDbContext _appContext;
        private readonly IJWTService _jwtService;


        public AuthenticationService(UserManager<AppUser> userManager, IConfiguration config, AuthDbContext authDbContext, AppDbContext appDbContext, IJWTService jWTService)
        {
            _userManager = userManager;
            _config = config;
            _authContext = authDbContext;
            _appContext = appDbContext;
            _jwtService = jWTService;
        }

        public async Task<object> LoginAsync(string username, string password)
        {


            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
            if (user == null)
            {
                return new
                {
                    message = "user not found!",
                    IsSucceeded = 0
                };

            }
            var checkAccess = _appContext.OrgInfos.Where(s => s.TenantId == user.TenantId && s.StatusId != 0).FirstOrDefaultAsync();
            if (checkAccess == null)
            {
                return new
                {
                    message = "user not found!",
                    IsSucceeded = 0
                };
            }
            var passwordCheck = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordCheck)
            {
                return new
                {
                    message = "Invalid username or password.",
                    IsSucceeded = 0
                };
            }
            var appUser = new AppUser
            {
                Id = user.Id,
                UserName = username,
                IsSuperAdmin = user.IsSuperAdmin,
                Email = user.Email,
                TenantId = user.TenantId
            };
            var token = _jwtService.GenerateToken(appUser, _config, out var expiry);

            var userAuth = new UserAuth
            {
                token = token,
                userid = user.Id,
                expires = expiry,
                created = DateTime.UtcNow
            };

            await _authContext.UserAuths.AddAsync(userAuth);
            await _authContext.SaveChangesAsync();
            return new
            {
                message = "Login successful!",
                IsSucceeded = 1,
                user,
                userName = user.UserName,
                tenantId = user.TenantId,
                AccessToken = token
            };
        }

        public async Task<object> Logout()
        {
            var token = _jwtService.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                return new
                {
                    success = false,
                    message = "Token not provided."
                };
            }
            var userData = _jwtService.DecodeToken();
            if (userData == null)
            {
                return new
                {
                    success = false,
                    message = "Invalid token."
                };
            }
            var authRecord = await _authContext.UserAuths.FirstOrDefaultAsync(x => x.token == token);
            if (authRecord != null)
            {
                _authContext.UserAuths.Remove(authRecord);
                await _authContext.SaveChangesAsync();
            }
            return new
            {
                success = true,
                message = "Logout successful."
            };
        }

        //public async Task<object> RegisterUserAsync(RegisterUserDTO dto)
        //{
        //    var existingUser = await _userManager.FindByNameAsync(dto.Username);
        //    if (existingUser != null)
        //        return new { success = false, message = "Username already exists." };

        //    var user = new AppUser
        //    {
        //        UserName = dto.Username,
        //        Email = dto.Email,
        //        Name = dto.Name,
        //        RoleId = dto.RoleId,
        //        RegLocId = dto.RegLocId,
        //        IsSuperAdmin = dto.IsSuperAdmin,
        //        CreatedOn = DateTime.UtcNow,
        //        IsDeleted = 0,
        //        BBOutletId = 0,
        //        BillAccess = -1,
        //        StaffId = -1,
        //        StoreId = -1,
        //        CreatedBy = 1,
        //        OpdCounterId = 0,
        //        OutLetId = 1,
        //        OrgId = 1,
        //        PlainTextPassword = dto.Password
        //    };

        //    var result = await _userManager.CreateAsync(user, dto.Password);

        //    if (!result.Succeeded)
        //    {
        //        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        //        return new { success = false, message = "User registration failed", errors };
        //    }

        //    return new { success = true, message = "User registered successfully", userId = user.Id };
        //}

    }



}
