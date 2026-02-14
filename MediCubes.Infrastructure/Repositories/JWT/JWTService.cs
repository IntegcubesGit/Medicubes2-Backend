using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Interfaces.JWT;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Repositories.JWT
{
    public class JWTService : IJWTService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JWTService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GenerateToken(AppUser app, IConfiguration config, out DateTime expiry)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(config["Jwt:Key"]);
            var expireTime = Convert.ToInt32(config["Jwt:ExpireTimeInHour"]);

            var orgId = app.OrgId;
            var claims = new List<Claim>
            {
                new Claim("UserID", app.Id.ToString()),
                new Claim(ClaimTypes.Name, app.UserName ?? string.Empty),
                new Claim("Email", app.Email ?? string.Empty),
                new Claim("IsSuperAdmin", app.IsSuperAdmin.ToString()),
                new Claim("OrgId", orgId.ToString())
            };
            if (app.RegLocId.HasValue)
                claims.Add(new Claim("RegLocId", app.RegLocId.Value.ToString()));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            expiry = tokenDescriptor.Expires.Value;

            return tokenHandler.WriteToken(token);
        }


        public TempUser DecodeToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                throw new UnauthorizedAccessException("Authorization token is missing or invalid.");

            var token = authHeader.Substring("Bearer ".Length).Trim();

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            if (jwtToken == null)
                throw new UnauthorizedAccessException("Invalid JWT token.");

            var user = new TempUser
            {
                UserId = Convert.ToInt32(jwtToken.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value),
                Username = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == JwtRegisteredClaimNames.UniqueName ||
                    c.Type == "name" ||
                    c.Type == "username")?.Value,
                Email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value,
                IsSuperAdmin = Convert.ToInt32(jwtToken.Claims.FirstOrDefault(c => c.Type == "IsSuperAdmin")?.Value ?? "0"),
                OrgId = Convert.ToInt32(jwtToken.Claims.FirstOrDefault(c => c.Type == "OrgId")?.Value ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value ?? "0"),
                RegLocId = int.TryParse(jwtToken.Claims.FirstOrDefault(c => c.Type == "RegLocId")?.Value, out var regLocId) ? regLocId : null
            };

            return user;
        }

        public bool IsTokenValid(string token, IConfiguration config)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(config["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteToken(string token)
        {
            // ⚙️ This is a placeholder — in real scenarios,
            // you’d add the token to a blacklist (e.g., Redis or DB)
            // or simply let it expire.
            await Task.CompletedTask;
            return true;
        }

        public string GetTokenFromHeader()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
                return null;

            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return authHeader.Substring("Bearer ".Length).Trim();

            return authHeader?.Trim();
        }

    }
}
