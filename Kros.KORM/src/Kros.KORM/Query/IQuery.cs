using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Kros.KORM.Query
{
    /// <summary>
    /// Interface, which describe class for executing query.
    /// <para>
    /// Instance which implement this interface can be used for creating and executing query for T model.
    /// </para>
    /// </summary>
    /// <typeparam name="T">Type of model class.</typeparam>
    /// <seealso cref="Kros.KORM.Query.IQueryBase{T}" />
    /// <remarks>
    /// <para>
    /// When you don't use <c>Select</c> or <c>From</c> function, than default values are taken from model.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///     You can use standard string sql query for querying data.
    ///     <code source="..\Examples\Kros.KORM.Examples\IQueryExample.cs" title="Query data by sql" region="Sql" lang="C#"  />
    ///   </para>
    ///   <para>
    ///     You can use sql query builder.
    ///     <code source="..\Examples\Kros.KORM.Examples\IQueryExample.cs" title="Query data by query builder" region="Select" lang="C#"  />
    /// </para>
    /// </example>
    public interface IQuery<T> : IProjectionQuery<T>
    {
        /// <summary>
        /// Create query from sql statement.
        /// </summary>
        /// <param name="sql">The SQL for executing in server.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>
        /// Query from sql.
        /// </returns>
        /// <remarks>
        /// Sql must be server specific. Because no translation is provide.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if <c>sql</c> is null or white string.</exception>
        IQueryBase<T> Sql(string sql, params object[] args);

        /// <summary>
        /// Add columns to sql.
        /// </summary>
        /// <param name="columns">The columns for select clausule.</param>
        /// <returns>
        /// Query for enumerable models.
        /// </returns>
        /// <remarks>
        ///  When Select method is not call, query take columns by T model.
        /// </remarks>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IQueryExample.cs" title="Projection" region="Select12" lang="C#"  />
        /// </example>
        IQuery<T> Select(params string[] columns);

        /// <summary>
        /// Add select part to sql.
        /// </summary>
        /// <param name="selectPart">The columns for select clausule. (Separate by ,)</param>
        /// <returns>
        /// Query for enumerable models.
        /// </returns>
        /// <remarks>
        ///  When <c>Select</c> method is not call, query take columns by T model.
        /// </remarks>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IQueryExample.cs" title="Projection" region="Select11" lang="C#"  />
        /// </example>
        /// <exception cref="ArgumentNullException">if <c>sqlPart</c> is null or white string.</exception>
        IQuery<T> Select(string selectPart);

        /// <summary>
        /// Add columns to sql
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// Query for enumerable models.
        /// </returns>
        /// <remarks>
        ///  When <c>Select</c> method is not call, query take columns by T model.
        /// </remarks>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IQueryExample.cs" title="Projection" region="Select13" lang="C#"  />
        /// </example>
        /// <exception cref="ArgumentNullException">if <c>selector</c> is null.</exception>
        IQuery<T> Select<TResult>(Func<T, TResult> selector);

        /// <summary>
        /// Add FROM part to sql query.
        /// </summary>
        /// <param name="table">Table name or join.</param>
        /// <returns>
        /// Query for enumerable models.
        /// </returns>
        /// <remarks>
        /// When <c>From</c> method is not call, query take table by T model type.
        /// </remarks>
        /// <example>
        /// <code source="..\Examples\Kros.KORM.Examples\IQueryExample.cs" title="From table" region="From" lang="C#"  />
        /// </example>
        /// <exception cref="ArgumentNullException">if <c>table</c> is null or white string.</exception>
        IProjectionQuery<T> From(string table);

    }
}
