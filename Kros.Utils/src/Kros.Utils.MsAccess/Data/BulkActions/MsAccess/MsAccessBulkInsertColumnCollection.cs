using Kros.Data.Schema.MsAccess;
using System.Collections.Generic;
using System.Data.OleDb;

namespace Kros.Data.BulkActions.MsAccess
{
    /// <summary>
    /// Collection of columns for bulk insert into Microsoft Access database from CSV file
    /// (<see cref="MsAccessBulkInsert">MsAccessBulkInsert</see>).
    /// </summary>
    public class MsAccessBulkInsertColumnCollection : List<MsAccessBulkInsertColumn>
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="MsAccessBulkInsertColumnCollection"/>.
        /// </summary>
        public MsAccessBulkInsertColumnCollection() : base()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="MsAccessBulkInsertColumnCollection"/> with specified initial
        /// capacity <paramref name="capacity"/>.
        /// </summary>
        /// <param name="capacity">Initial capacity of the inner list.</param>
        public MsAccessBulkInsertColumnCollection(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="MsAccessBulkInsertColumnCollection"/> and adds into it data
        /// <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">Initial data added to the collection.</param>
        public MsAccessBulkInsertColumnCollection(IEnumerable<MsAccessBulkInsertColumn> collection)
            : base(collection)
        {
        }

        #endregion

        #region Common

        /// <summary>
        /// Adds a column with name <paramref name="columnName"/> with type set to
        /// <see cref="BulkInsertColumnType.Undefined">BulkInsertColumnType.Undefined</see>.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        public void Add(string columnName)
        {
            Add(new MsAccessBulkInsertColumn(columnName));
        }

        /// <summary>
        /// Adds a column with name <paramref name="columnName"/> and type <paramref name="columnType"/>.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <param name="columnType">Column type.</param>
        public void Add(string columnName, BulkInsertColumnType columnType)
        {
            Add(new MsAccessBulkInsertColumn(columnName, columnType));
        }

        /// <summary>
        /// Adds column based on database column schema <paramref name="column"/>.
        /// </summary>
        /// <param name="column">Database column schema.</param>
        public void Add(MsAccessColumnSchema column)
        {
            Add(new MsAccessBulkInsertColumn(column.Name, GetColumnType(column)));
        }

        /// <summary>
        /// Adds all columns in <paramref name="columnNames"/> with type set to
        /// <see cref="BulkInsertColumnType.Undefined">BulkInsertColumnType.Undefined</see>.
        /// </summary>
        /// <param name="columnNames">Column names.</param>
        public void AddRange(params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                Add(columnName);
            }
        }

        /// <summary>
        /// Adds columns based on database column schemas <paramref name="columns"/>.
        /// </summary>
        /// <param name="columns">Database column schemas.</param>
        public void AddRange(IEnumerable<MsAccessColumnSchema> columns)
        {
            foreach (var column in columns)
            {
                Add(column);
            }
        }

        #endregion

        #region Pomocné

        // Returns type of the column for bulk insert based on column's data type.
        private static BulkInsertColumnType GetColumnType(MsAccessColumnSchema column)
        {
            if ((column.OleDbType == OleDbType.VarChar) ||
                (column.OleDbType == OleDbType.VarWChar) ||
                (column.OleDbType == OleDbType.LongVarChar) ||
                (column.OleDbType == OleDbType.LongVarWChar) ||
                (column.OleDbType == OleDbType.Char) ||
                (column.OleDbType == OleDbType.WChar))
            {
                return BulkInsertColumnType.Text;
            }
            return BulkInsertColumnType.Undefined;
        }

        #endregion
    }
}
