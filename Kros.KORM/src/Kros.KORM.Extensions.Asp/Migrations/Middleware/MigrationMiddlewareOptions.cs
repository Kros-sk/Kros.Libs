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
    }
}
