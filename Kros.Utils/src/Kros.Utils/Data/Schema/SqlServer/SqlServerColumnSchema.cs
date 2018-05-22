using Kros.Utils;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Kros.Data.Schema.SqlServer
{
    /// <summary>
    /// Table's column schema for Microsoft SQL Server.
    /// </summary>
    public class SqlServerColumnSchema
        : ColumnSchema
    {
        #region Constructors

        /// <inheritdoc/>
        public SqlServerColumnSchema(string name)
            : this(name, DefaultAllowNull, DefaultDefaultValue, DefaultSize)
        {
        }

        /// <inheritdoc/>
        public SqlServerColumnSchema(string name, bool allowNull)
            : this(name, allowNull, DefaultDefaultValue, DefaultSize)
        {
        }

        /// <inheritdoc/>
        public SqlServerColumnSchema(string name, bool allowNull, object defaultValue)
            : this(name, allowNull, defaultValue, DefaultSize)
        {
        }

        /// <inheritdoc/>
        public SqlServerColumnSchema(string name, bool allowNull, object defaultValue, int size)
            : base(name, allowNull, defaultValue, size)
        {
        }

        #endregion

        #region Common

        /// <summary>
        /// Data type of the column.
        /// </summary>
        public SqlDbType SqlDbType { get; set; }

        /// <inheritdoc/>
        /// <exception cref="System.ArgumentException">
        /// Value of <paramref name="param"/> is not of type <see cref="SqlParameter"/>.
        /// </exception>
        public override void SetParameterDbType(IDataParameter param)
        {
            Check.IsOfType<SqlParameter>(param, nameof(param));
            (param as SqlParameter).SqlDbType = SqlDbType;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
        {
            return string.Format("Column {0}: SqlDbType = {1}, AllowNull = {2}, DefaultValue = {3}, Size = {4}",
                FullName, SqlDbType, AllowNull, ToStringDefaultValue(), Size);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion
    }
}
