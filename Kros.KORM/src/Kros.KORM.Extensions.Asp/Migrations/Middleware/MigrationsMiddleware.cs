using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Kros.KORM.Migrations.Middleware
{
    /// <summary>
    /// Middleware for executing database migration.
    /// </summary>
    public class MigrationsMiddleware
    {
        private const string WasMigrationExecutedKey = "WasMigrationExecuted";

        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly IMigrationsRunner _migrationsRunner;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationsMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next delegate.</param>
        /// <param name="cache">Memory cache.</param>
        /// <param name="migrationsRunner">Migrations runner.</param>
        public MigrationsMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            IMigrationsRunner migrationsRunner)
        {
            _next = next;
            _cache = Check.NotNull(cache, nameof(cache));
            _migrationsRunner = Check.NotNull(migrationsRunner, nameof(migrationsRunner));
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public async Task Invoke(HttpContext context)
        {
            if (CanMigrate())
            {
                SetupCache();

                await _migrationsRunner.MigrateAsync();
            }
        }

        private bool CanMigrate()
            => !_cache.TryGetValue<bool>(WasMigrationExecutedKey, out var migrated) || !migrated;

        private void SetupCache()
        {
            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _cache.Set(WasMigrationExecutedKey, true, options);
        }
    }
}
