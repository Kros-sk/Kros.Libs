using System;

namespace Kros.KORM.Migrations.Middleware
{
    /// <summary>
    /// Migration middleware options.
    /// </summary>
    public class MigrationMiddlewareOptions
    {
        /// <summary>
        /// Migrations endpoint url.
        /// </summary>
        public string EndpointUrl { get; set; } = "/kormmigrate";

        /// <summary>
        /// Minimum time between two migrations.
        /// </summary>
        public TimeSpan SlidingExirationBetweenTwoMigrations { get; set; } = TimeSpan.FromMinutes(1);
    }
}
