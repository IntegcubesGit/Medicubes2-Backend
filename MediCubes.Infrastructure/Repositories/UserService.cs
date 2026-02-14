using Application.Common.Interfaces;
using Domain.DTOs;
using Domain.Entities;
using EFCore.BulkExtensions;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserService : IUserService
    {
        private readonly ICurrentUser _currentUser;
        private readonly AppDbContext _db;
        private readonly AuthDbContext _authdb;
        private readonly ITimeService timeService;
        private readonly UserManager<AppUser> userManager;

        public UserService(ICurrentUser currentUser, AppDbContext db, ITimeService service, AuthDbContext authdb, UserManager<AppUser> _userManager)
        {
            _currentUser = currentUser;
            _db = db;
            timeService = service;
            _authdb = authdb;
            userManager = _userManager;
        }

        public async Task<object> RegisterUser(RegisterOrUpdateUserRequestDTO registerUserRequest, List<int> roles)
        {
            var jwtClaims = _currentUser.GetCurrentUser();
            var orgId = jwtClaims.OrgId;

            if (!string.IsNullOrWhiteSpace(registerUserRequest.Email))
            {
                var existingEmail = await userManager.FindByEmailAsync(registerUserRequest.Email);
                if (existingEmail != null)
                {
                    return new { message = "Email is already in use!", IsSucceeded = 0 };
                }
            }

            var existingName = await userManager.FindByNameAsync(registerUserRequest.Username);
            if (existingName != null)
            {
                return new { message = "Username is already in use!", IsSucceeded = 0 };
            }

            // Create new user
            var user = new AppUser
            {
                Name = registerUserRequest.FirstName + " " + registerUserRequest.LastName,
                UserName = registerUserRequest.Username,
                NormalizedUserName = registerUserRequest.Username?.ToUpper(),
                Email = registerUserRequest.Email,
                NormalizedEmail = registerUserRequest.Email?.ToUpper(),
                // PhoneNumber = registerUserRequest.Phone,
                PlainTextPassword = registerUserRequest.Password,
                OrgId = orgId,
                CreatedBy = jwtClaims.UserId,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = 0,
                CanSignRpt = registerUserRequest.CanSignRpt == 0 ? false : true,
                ShowNameOnRpt = registerUserRequest.ShowNameOnRpt,
                StaffId = registerUserRequest.RelevantStaffId,
                DiscountLimit = registerUserRequest.DiscountLimit,
                MinCash = registerUserRequest.MinCash,
                EmpId = registerUserRequest.EmpNo,
                Qualification = registerUserRequest.Qualification,
                ReportNote = registerUserRequest.ReportNote,
                RegLocId = registerUserRequest.RegLocId,
                OpdCounterId = 0,
                StoreId = -1,
                ShareOn = 0,
                OutLetId = 0,
                UserStamp = !string.IsNullOrEmpty(registerUserRequest.DoctorStamp)
                                ? Convert.FromBase64String(registerUserRequest.DoctorStamp)
                                : null
            };

            // Create user with password
            var result = await userManager.CreateAsync(user, registerUserRequest.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new { message = errors, IsSucceeded = 0 };
            }

            // --------- Save Related Entities -----------

            var maxLocationId = await _db.AppUserLocation.MaxAsync(x => (int?)x.id) ?? 0;
            var locationEntries = registerUserRequest.LocationIDs.Select((locId, index) => new AppUserLocation
            {
                id = maxLocationId + index + 1,
                userid = user.Id,
                locid = locId
            }).ToList();

            _db.AppUserLocation.RemoveRange(
                await _db.AppUserLocation.Where(x => x.userid == user.Id).ToListAsync()
            );
            await _db.AppUserLocation.AddRangeAsync(locationEntries);

            // UserRoles
            var userRoles = roles.Select(roleId => new AppUserAppRole
            {
                UserId = user.Id,
                RoleId = roleId,
                CreatedBy = jwtClaims.UserId,
                CreatedOn = DateTime.UtcNow,
                OrgId = orgId,
                IsDeleted = 0
            }).ToList();

            _db.UserRoles.AddRange(userRoles);

            // Save changes
            await _db.SaveChangesAsync();

            return new
            {
                message = "User created successfully!",
                IsSucceeded = 1
            };
        }

        public async Task<object> UpdateUser(RegisterOrUpdateUserRequestDTO updateUserRequest, List<int> roles)
        {
            var jwtClaims = _currentUser.GetCurrentUser();
            var orgId = jwtClaims.OrgId;

            var user = await userManager.FindByIdAsync(updateUserRequest.UserId.ToString());
            if (user == null)
            {
                return new { message = "User not found!", IsSucceeded = 0 };
            }

            if (!string.Equals(user.Email, updateUserRequest.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailConflict = await userManager.FindByEmailAsync(updateUserRequest.Email);
                if (emailConflict != null && emailConflict.Id != user.Id)
                {
                    return new { message = "Email is already in use!", IsSucceeded = 0 };
                }

                user.Email = updateUserRequest.Email;
                user.NormalizedEmail = updateUserRequest.Email?.ToUpperInvariant();
            }

            if (!string.Equals(user.UserName, updateUserRequest.Username, StringComparison.OrdinalIgnoreCase))
            {
                var usernameConflict = await userManager.FindByNameAsync(updateUserRequest.Username);
                if (usernameConflict != null && usernameConflict.Id != user.Id)
                {
                    return new { message = "Username is already in use!", IsSucceeded = 0 };
                }

                user.UserName = updateUserRequest.Username;
                user.NormalizedUserName = updateUserRequest.Username?.ToUpperInvariant();
            }

            string fullName = $"{updateUserRequest.FirstName} {updateUserRequest.LastName}".Trim();

            user.Name = fullName;
            user.PlainTextPassword = updateUserRequest.Password;

            if (!string.IsNullOrWhiteSpace(updateUserRequest.Password))
            {
                var passwordMatches = await userManager.CheckPasswordAsync(user, updateUserRequest.Password);
                if (!passwordMatches)
                {
                    var removeResult = await userManager.RemovePasswordAsync(user);
                    if (!removeResult.Succeeded)
                        return new { message = "Failed to reset password!", IsSucceeded = 0 };

                    var addResult = await userManager.AddPasswordAsync(user, updateUserRequest.Password);
                    if (!addResult.Succeeded)
                        return new { message = "Failed to update password!", IsSucceeded = 0 };
                }
            }

            user.CanSignRpt = updateUserRequest.CanSignRpt == 0 ? false : true;
            user.ShowNameOnRpt = updateUserRequest.ShowNameOnRpt;
            user.StaffId = updateUserRequest.RelevantStaffId;
            user.DiscountLimit = updateUserRequest.DiscountLimit;
            user.MinCash = updateUserRequest.MinCash;
            user.EmpId = updateUserRequest.EmpNo;
            user.Qualification = updateUserRequest.Qualification;
            user.ReportNote = updateUserRequest.ReportNote;
            user.RegLocId = updateUserRequest.RegLocId;
            user.ModifiedBy = jwtClaims.UserId;
            user.ModifiedOn = DateTime.UtcNow;
            user.UserStamp = !string.IsNullOrEmpty(updateUserRequest.DoctorStamp) ? Convert.FromBase64String(updateUserRequest.DoctorStamp) : null;



            var existingLocations = await _db.AppUserLocation.Where(l => l.userid == user.Id).ToListAsync();
            if (existingLocations.Any())
                _db.AppUserLocation.RemoveRange(existingLocations);

            var maxLocId = await _db.AppUserLocation.MaxAsync(x => (int?)x.id) ?? 0;
            var locationsToAdd = updateUserRequest.LocationIDs.Select((locId, index) => new AppUserLocation
            {
                id = maxLocId + index + 1,
                userid = user.Id,
                locid = locId
            }).ToList();

            await _db.AppUserLocation.AddRangeAsync(locationsToAdd);

            var existingUserRoles = await _db.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();

            var rolesToRemove = existingUserRoles.Where(ur => !roles.Contains(ur.RoleId)).ToList();
            if (rolesToRemove.Any())
                _db.UserRoles.RemoveRange(rolesToRemove);

            var rolesToAdd = roles.Where(roleId => !existingUserRoles.Any(ur => ur.RoleId == roleId)).Select(roleId => new AppUserAppRole
            {
                UserId = user.Id,
                RoleId = roleId,
                IsDeleted = 0,
                OrgId = orgId,
                CreatedBy = jwtClaims.UserId,
                CreatedOn = DateTime.UtcNow
            }).ToList();

            _db.UserRoles.AddRange(rolesToAdd);

            await _db.SaveChangesAsync();

            return new
            {
                message = "User information updated successfully!",
                IsSucceeded = 1
            };
        }

        public async Task<object> GetAllUsers(int pageNumber, int pageSize, string sort, string order, string? search)
        {
            var currentUser = _currentUser.GetCurrentUser();
            var rawData = await (from users in _db.Users.Where(u => u.IsDeleted == 0 && u.OrgId == currentUser.OrgId)
                                 join createdby in _db.Users on users.CreatedBy equals createdby.Id into createdbyGroup
                                 from createdby in createdbyGroup.DefaultIfEmpty()
                                 select new
                                 {
                                     users.Id,
                                     FullName = users.Name,
                                     users.Email,
                                     users.UserName,
                                     users.Status,
                                     CreatedBy = createdby.UserName,
                                     users.CreatedOn
                                 }).AsNoTracking().ToListAsync();

            var processedData = rawData.Select(u => new
            {
                u.Id,

                FirstName = u.FullName.Split(' ').FirstOrDefault() ?? "",
                LastName = u.FullName.Contains(" ") ? u.FullName.Split(' ').LastOrDefault() : "",
                u.Email,
                u.UserName,
                u.Status,
                u.CreatedBy,
                CreatedOn = u.CreatedOn
            }).ToList();

            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort.ToLower())
                {
                    case "createdon":
                        processedData = order.ToLower() == "asc" ? processedData.OrderBy(x => x.CreatedOn).ToList() : processedData.OrderByDescending(x => x.CreatedOn).ToList();
                        break;
                    case "firstname":
                        processedData = order.ToLower() == "asc" ? processedData.OrderBy(x => x.FirstName).ToList() : processedData.OrderByDescending(x => x.FirstName).ToList();
                        break;
                    case "lastname":
                        processedData = order.ToLower() == "asc" ? processedData.OrderBy(x => x.LastName).ToList() : processedData.OrderByDescending(x => x.LastName).ToList();
                        break;
                    case "username":
                        processedData = order.ToLower() == "asc" ? processedData.OrderBy(x => x.UserName).ToList() : processedData.OrderByDescending(x => x.UserName).ToList();
                        break;
                    case "email":
                        processedData = order.ToLower() == "asc" ? processedData.OrderBy(x => x.Email).ToList() : processedData.OrderByDescending(x => x.Email).ToList();
                        break;
                    case "createdby":
                        processedData = order.ToLower() == "asc" ? processedData.OrderBy(x => x.CreatedBy).ToList() : processedData.OrderByDescending(x => x.CreatedBy).ToList();
                        break;
                    default:
                        processedData = order.ToLower() == "asc" ? processedData.OrderBy(x => x.CreatedOn).ToList() : processedData.OrderByDescending(x => x.CreatedOn).ToList();
                        break;
                }
            }
            processedData = processedData.OrderByDescending(x => x.CreatedOn).ToList();

            var data = processedData.ToList();

            if (!string.IsNullOrEmpty(search) && search != "null")
            {
                data = data.Where(x =>
                    (x.UserName != null && x.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (x.Email != null && x.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (x.FirstName != null && x.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (x.LastName != null && x.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (x.CreatedBy != null && x.CreatedBy.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            var totalCount = data.Count();
            var _users = data.Skip(pageNumber * pageSize).Take(pageSize).ToList();

            var TotalPages = Math.Max((int)Math.Ceiling((double)totalCount / pageSize), 1);
            var startIndex = pageNumber * pageSize;
            var endIndex = Math.Min(startIndex + pageSize, totalCount) - 1;

            if (totalCount == 0)
            {
                startIndex = 0;
                endIndex = 0;
            }

            var pagination = new
            {
                Length = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                LastPage = TotalPages,
                StartIndex = startIndex,
                EndIndex = endIndex
            };

            return new
            {
                users = _users,
                pagination
            };
        }

        public async Task<object> GetRoles()
        {
            var currentUser = _currentUser.GetCurrentUser();
            var roles = await _db.Roles.Where(r => r.OrgId == currentUser.OrgId && r.IsDeleted == 0).ToListAsync();
            return roles;
        }

        public async Task<object> GetUserById(int userId)
        {
            var currentUser = _currentUser.GetCurrentUser();
            var rawData = await (from _user in _db.Users
                                 where _user.IsDeleted == 0 && _user.OrgId == currentUser.OrgId && _user.Id == userId
                                 select new
                                 {
                                     _user.Id,
                                     FullName = _user.Name,
                                     UserName = _user.UserName,
                                     _user.Email,
                                     Password = _user.PlainTextPassword,
                                     Phone = _user.PhoneNumber,
                                     _user.CreatedOn,
                                     CreatedBy = _user.CreatedBy
                                 }).FirstOrDefaultAsync();

            if (rawData == null)
                return null;

            var user = new
            {
                rawData.Id,
                FirstName = rawData.FullName.Split(' ').FirstOrDefault() ?? "",
                LastName = rawData.FullName.Contains(" ") ? rawData.FullName.Split(' ').LastOrDefault() : "",
                rawData.Email,
                rawData.UserName,
                rawData.Password,
                rawData.Phone,
                rawData.CreatedBy,
                rawData.CreatedOn
            };
            var roles = await (from userRole in _db.UserRoles.Where(x => x.UserId == userId && x.IsDeleted == 0 && x.OrgId == currentUser.OrgId)
                               select userRole.RoleId
                               ).ToListAsync();


            return new
            {
                user,
                roles
            };
        }
        public async Task<object> DeleteUser(int userId)
        {
            var currentUser = _currentUser.GetCurrentUser();
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId && u.OrgId == currentUser.OrgId);
            if (user == null)
            {
                return new
                {
                    message = "User not found.",
                    IsSucceeded = 0
                };
            }

            user.IsDeleted = 1;

            var userTokens = await _authdb.UserAuths.Where(jwt => jwt.userid == user.Id).ToListAsync();

            await _authdb.BulkDeleteAsync(userTokens);

            // Save changes to the databases
            await _db.SaveChangesAsync();
            await _authdb.SaveChangesAsync();


            return new
            {
                message = "User inactivated and tokens blacklisted successfully!",
                IsSucceeded = 1
            };
        }

        public async Task<object> GetUserInfoById(int userId)
        {
            var jwtClaims = _currentUser.GetCurrentUser();
            var orgId = jwtClaims.OrgId;

            var user = await _db.Users.Where(u => u.Id == userId && u.OrgId == orgId && u.IsDeleted == 0).FirstOrDefaultAsync();

            if (user == null)
            {
                return new { Message = "User not found", Status = 404 };
            }

            var roleIds = await _db.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToListAsync();


            return new
            {
                user.Id,
                FullName = $"{user.Name}",
                user.UserName,
                Password = user.PlainTextPassword,
                user.LockoutEnd,
                RoleIds = roleIds,
                user.RegLocId,
                user.CanSignRpt,
                user.Email,
                user.PhoneNumber,
                user.EmpId,
                user.ShowNameOnRpt,
                user.Qualification,
                user.StaffId,
                user.ReportNote,
                user.DiscountLimit,
                user.MinCash,
                UserStamp = user.UserStamp != null ? Convert.ToBase64String(user.UserStamp) : null,
                user.OpdCounterId
            };
        }
        public async Task<object> GetOrgLocationsById(int userId)
        {
            var jwtClaims = _currentUser.GetCurrentUser();

            var panelDetails = await (from location in _db.OrgLocations.Where(l => l.StatusId == 1 && l.OrgId == jwtClaims.OrgId)
                                      join userLoc in _db.AppUserLocation.Where(p => p.userid == userId) on location.LocId equals userLoc.locid into userLocGroup
                                      from ul in userLocGroup.DefaultIfEmpty()
                                      select new
                                      {
                                          location.LocId,
                                          location.Code,
                                          location.Title,
                                          location.Address,
                                          Selected = ul != null
                                      }).ToListAsync();

            return panelDetails;
        }
        public async Task<object> GetUserInformationByParsingJWT()
        {
            var jwtClaims = _currentUser.GetCurrentUser();
            var userName = jwtClaims.Username;
            var email = jwtClaims.Email;
            var user = await userManager.FindByNameAsync(userName) ?? await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new StatusCodeResult(401);
            }
            return new
            {
                user.Id,
                Name = $"{user.Name}",
                user.Email,
                Status = user.IsDeleted == 0 ? 1 : 0,
                Avatar = ""
            };
        }
    }
}
