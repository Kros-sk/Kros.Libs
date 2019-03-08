using Kros.KORM.Migrations.Middleware;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extensions for adding <see cref="MigrationsMiddleware"/> into application pipeline.
    /// </summary>
    public static class BuilderExtensions
    {
        /// <summary>
        /// Adds database migrations middleware into application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>
        /// <see cref="IApplicationBuilder"/>.
        /// </returns>
        public static IApplicationBuilder UseKormMigrations(this IApplicationBuilder app)
            => app.UseKormMigrations(null);

        /// <summary>
        /// Adds database migrations middlewre into application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="setupAction">Configure middleware/</param>
        /// <returns>
        /// <see cref="IApplicationBuilder"/>.
        /// </returns>
        public static IApplicationBuilder UseKormMigrations(
            this IApplicationBuilder app,
            Action<MigrationMiddlewareOptions> setupAction)
        {
            var options = new MigrationMiddlewareOptions();
            setupAction?.Invoke(options);

            return app.Map(options.EndpointUrl, builder => builder.UseMiddleware<MigrationsMiddleware>(options));
        }
    }
}
