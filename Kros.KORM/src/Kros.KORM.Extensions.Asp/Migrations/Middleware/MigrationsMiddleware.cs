using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly MigrationMiddlewareOptions _options;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationsMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next delegate.</param>
        /// <param name="cache">Memory cache.</param>
        /// <param name="options">Migration options.</param>
        public MigrationsMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            MigrationMiddlewareOptions options)
        {
            _next = next;
            _options = Check.NotNull(options, nameof(options));
            _cache = Check.NotNull(cache, nameof(cache));
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="migrationsRunner">Migrations runner.</param>
        public async Task Invoke(
            HttpContext context,
            IMigrationsRunner migrationsRunner)
        {
            if (CanMigrate())
            {
                SetupCache();

                await migrationsRunner.MigrateAsync();
            }
        }

        private bool CanMigrate()
            => !_cache.TryGetValue<bool>(WasMigrationExecutedKey, out var migrated) || !migrated;

        private void SetupCache()
        {
            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_options.SlidingExpirationBetweenMigrations);
            _cache.Set(WasMigrationExecutedKey, true, options);
        }
    }
}
