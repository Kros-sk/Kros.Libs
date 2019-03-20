using System;

namespace Kros.KORM.Migrations.Middleware
{
    /// <summary>
    /// Migration middleware options.
    /// </summary>
    public class MigrationMiddlewareOptions
    {
        /// <summary>
        /// Migrations endpoint URL.
        /// </summary>
        public string EndpointUrl { get; set; } = "/kormmigrate";

        /// <summary>
        /// Minimum time between two migrations.
        /// </summary>
        public TimeSpan SlidingExpirationBetweenMigrations { get; set; } = TimeSpan.FromMinutes(1);
    }
}
