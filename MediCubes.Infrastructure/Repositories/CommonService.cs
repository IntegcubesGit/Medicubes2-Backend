using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CommonService : ICommonService
    {
        private readonly ICurrentUser _currentUser;
        private readonly AppDbContext _db;

        public CommonService(ICurrentUser currentUser, AppDbContext db)
        {
            _currentUser = currentUser;
            _db = db;
        }

        public async Task<object> GetConfigSettings()
        {
            var claims = _currentUser.GetCurrentUser();
            var settings = await _db.ConfigSettings
                .Where(s => s.OrgId == claims.OrgId)
                .ToListAsync();
            return settings;
        }

        public async Task<object> GetAllLocations()
        {
            var claims = _currentUser.GetCurrentUser();
            return await _db.OrgLocations
                .Where(a => a.OrgId == claims.OrgId)
                .ToListAsync();
        }
    }
}
