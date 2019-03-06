using System.Threading.Tasks;

namespace Kros.KORM.Migrations
{
    /// <summary>
    /// Interface which describe class for execution database migrations.
    /// </summary>
    public interface IMigrationsRunner
    {
        /// <summary>
        /// Execute database migrations.
        /// </summary>
        Task MigrateAsync();
    }
}
