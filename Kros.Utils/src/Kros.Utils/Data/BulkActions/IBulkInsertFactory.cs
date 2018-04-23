using System.Data.Common;

namespace Kros.Data.BulkActions
{
    /// <summary>
    /// Creates instances of <see cref="IBulkInsert"/> for bulk inserting.
    /// </summary>
    public interface IBulkInsertFactory
    {
        /// <summary>
        /// Creates a new <see cref="IBulkInsert"/> class.
        /// </summary>
        /// <returns>The Bulk Insert.</returns>
        IBulkInsert GetBulkInsert();
    }
}
