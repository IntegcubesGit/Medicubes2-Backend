using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public JwtValidationMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context, AuthDbContext dbContext)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            await _next(context);
            return;
        }
        var path = context.Request.Path.Value?.ToLower();

        if (path != null && path.Contains("/api/authentication/authenticate"))
        {
            await _next(context);
            return;
        }

        if (!path.StartsWith("/api/"))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Missing or invalid Authorization header");
            return;
        }
        var token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var userAuth = await dbContext.UserAuths.FirstOrDefaultAsync(u => u.token == token && !u.isrevoked);

            if (userAuth == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Token not found or revoked");
                return;
            }

            if (userAuth.expires < DateTime.UtcNow)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Token expired");
                return;
            }

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name")?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                context.Items["UserID"] = userId;
                context.Items["Username"] = username;
                context.Items["Email"] = email;
            }

            await _next(context);
        }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Token validation failed");
        }
    }
}
