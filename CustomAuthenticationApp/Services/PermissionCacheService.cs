using CustomAuthenticationApp.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CustomAuthenticationApp.Services
{
    public interface IPermissionService
    {
        Task<bool> UserHasPermissionAsync(int userId, string permission);
        void InvalidateUserPermissions(int userId);
    }

    public class PermissionCacheService : IPermissionService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(20);

        public PermissionCacheService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<bool> UserHasPermissionAsync(int userId, string permission)
        {
            string cacheKey = $"user_perms_{userId}";

            var permissions = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                // Extract distinct string list of valid permissions via user assigned roles
                return await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => rp.Permission.Name)
                    .Distinct()
                    .ToListAsync();
            });

            return permissions?.Contains(permission) ?? false;
        }

        public void InvalidateUserPermissions(int userId)
        {
            _cache.Remove($"user_perms_{userId}");
        }
    }
}
