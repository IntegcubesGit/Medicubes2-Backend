using Domain.DTOs;

namespace Application.Common.Interfaces
{
    /// <summary>
    /// Single abstraction for current user/tenant from the request (JWT). Use this instead of calling IJWTService.DecodeToken() directly in controllers or services.
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>Current user and tenant from the request token. Throws if token is missing or invalid.</summary>
        TempUser GetCurrentUser();
    }
}
