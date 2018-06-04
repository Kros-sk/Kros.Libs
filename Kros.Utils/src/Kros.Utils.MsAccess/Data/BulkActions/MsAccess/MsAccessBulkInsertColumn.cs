using Kros.Utils;
using System;

namespace Kros.Data.BulkActions.MsAccess
{
    /// <summary>
    /// Definition of column for bulk insert into Microsoft Access database from CSV file.
    /// (<see cref="MsAccessBulkInsert">MsAccessBulkInsert</see>).
    /// </summary>
    public class MsAccessBulkInsertColumn
    {
        #region Constructors

        /// <summary>
        /// Creates a new definition for column <paramref name="columnName"/> with type set to
        /// <see cref="BulkInsertColumnType.Undefined">BulkInsertColumnType.Undefined</see>.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <exception cref="ArgumentNullException">Value of <paramref name="columnName"/> is <see langword="null"/>,
        /// empty string, or string containing whitespace characters only.</exception>
        public MsAccessBulkInsertColumn(string columnName)
            : this(columnName, BulkInsertColumnType.Undefined)
        {
        }

        /// <summary>
        /// Creates a new definition for column <paramref name="columnName"/> with type <paramref name="columnType"/>.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <param name="columnType">Column type.</param>
        /// <exception cref="ArgumentNullException">Value of <paramref name="columnName"/> is <see langword="null"/>,
        /// empty string, or string containing whitespace characters only.</exception>
        public MsAccessBulkInsertColumn(string columnName, BulkInsertColumnType columnType)
        {
            ColumnName = Check.NotNullOrWhiteSpace(columnName, nameof(columnName));
            ColumnType = columnType;
        }

        #endregion

        #region Common

        /// <summary>
        /// Column type.
        /// </summary>
        /// <value><see cref="BulkInsertColumnType">BulkInsertColumnType</see> value.</value>
        public BulkInsertColumnType ColumnType { get; }

        /// <summary>
        /// Column name.
        /// </summary>
        /// <value>String.</value>
        public string ColumnName { get; }

        #endregion
    }
}
