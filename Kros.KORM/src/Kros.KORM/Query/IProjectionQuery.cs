using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kros.KORM.Query
{
    /// <summary>
    /// Represent result of projection operation.
    /// </summary>
    /// <typeparam name="T">Type of model class.</typeparam>
    /// <seealso cref="Kros.KORM.Query.IQueryBase{T}" />
    public interface IProjectionQuery<T> : IQueryBase<T>
    {

        /// <summary>
        /// Add where condition to sql.
        /// </summary>
        /// <param name="whereCondition">The where condition.</param>
        /// <param name="args">The arguments for where.</param>
        /// <returns>
        /// Query for enumerable models.
        /// </returns>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IQueryExample.cs" title="Projection" region="Where1" lang="C#"  />
        /// </example>
        /// <exception cref="ArgumentNullException">if <c>whereCondition</c> is null or white string.</exception>
        IFilteredQuery<T> Where(string whereCondition, params object[] args);

        /// <summary>
        /// Returns the first item of which match where condition, or a default value if item doesn't exist.
        /// </summary>
        /// <param name="whereCondition">The where condition.</param>
        /// <param name="args">The arguments for where.</param>
        /// <returns>
        ///  <b>null</b> if item doesn't exist; otherwise, the first item which match the condition.
        /// </returns>
        /// <example>
        /// <code>
        /// var item = query.FirstOrDefault("Id = @1", 22);
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">if <c>whereCondition</c> is null or white string.</exception>
        T FirstOrDefault(string whereCondition, params object[] args);

        /// <summary>
        /// Check if exist elements in the table which match condition.
        /// </summary>
        /// <param name="whereCondition">The where condition.</param>
        /// <param name="args">The arguments for where.</param>
        /// <returns>
        /// <see langword="true"/> if exist elements in the table which match condition; otherwise, false.
        /// </returns>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IQueryExample.cs" title="Check if exist elements in the table which match condition" region="Any" lang="C#"  />
        /// </example>
        /// <exception cref="ArgumentNullException">if <c>whereCondition</c> is null or white string.</exception>
        bool Any(string whereCondition, params object[] args);

        /// <summary>
        /// Add order by statement to sql.
        /// </summary>
        /// <param name="orderBy">The order by statement.</param>
        /// <returns>
        /// Query for enumerable models.
        /// </returns>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IQueryExample.cs" title="OrderBy" region="OrderBy" lang="C#"  />
        /// </example>
        /// <exception cref="ArgumentNullException">if <c>orderBy</c> is null or white string.</exception>
        IOrderedQuery<T> OrderBy(string orderBy);

        /// <summary>
        /// Add group by statement to sql query.
        /// </summary>
        /// <param name="groupBy">The group by statement.</param>
        /// <returns>
        /// Query for enumerable models.
        /// </returns>
        /// <remarks>
        /// You can also add HAVING statement.
        /// </remarks>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IQueryExample.cs" title="GroupBy" region="GroupBy" lang="C#"  />
        /// </example>
        /// <exception cref="ArgumentNullException">if <c>groupBy</c> is null or white string.</exception>
        IGroupedQuery<T> GroupBy(string groupBy);
    }
}
