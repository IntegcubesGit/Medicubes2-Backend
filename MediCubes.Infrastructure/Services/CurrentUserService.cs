using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Domain.DTOs;

namespace Infrastructure.Services
{
    public class CurrentUserService : ICurrentUser
    {
        private readonly IJWTService _jwtService;

        public CurrentUserService(IJWTService jwtService)
        {
            _jwtService = jwtService;
        }

        public TempUser GetCurrentUser() => _jwtService.DecodeToken();
    }
}
