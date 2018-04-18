using Kros.Utils;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Kros.Data.Schema.SqlServer
{
    /// <summary>
    /// Schéma stĺpca tabuľky pre SQL Server.
    /// </summary>
    public class SqlServerColumnSchema
        : ColumnSchema
    {

        #region Fields

        // Prevzaté z .NET z internej triedy System.Data.SqlClient.MetaType.
        private static readonly Dictionary<SqlDbType, DbType> _dbTypeMapping = new Dictionary<SqlDbType, DbType>() {
            { SqlDbType.BigInt, DbType.Int64 },
            { SqlDbType.Binary, DbType.Binary },
            { SqlDbType.Bit, DbType.Boolean },
            { SqlDbType.Date, DbType.Date },
            { SqlDbType.DateTime, DbType.DateTime },
            { SqlDbType.DateTime2, DbType.DateTime2 },
            { SqlDbType.DateTimeOffset, DbType.DateTimeOffset },
            { SqlDbType.Decimal, DbType.Decimal },
            { SqlDbType.Float, DbType.Double },
            { SqlDbType.Char, DbType.AnsiStringFixedLength },
            { SqlDbType.Image, DbType.Binary },
            { SqlDbType.Int, DbType.Int32 },
            { SqlDbType.Money, DbType.Currency },
            { SqlDbType.NChar, DbType.StringFixedLength },
            { SqlDbType.NText, DbType.String },
            { SqlDbType.NVarChar, DbType.String },
            { SqlDbType.Real, DbType.Single },
            { SqlDbType.SmallDateTime, DbType.DateTime },
            { SqlDbType.SmallInt, DbType.Int16 },
            { SqlDbType.SmallMoney, DbType.Currency },
            { SqlDbType.Structured, DbType.Object },
            { SqlDbType.Text, DbType.AnsiString },
            { SqlDbType.Time, DbType.Time },
            { SqlDbType.Timestamp, DbType.Binary },
            { SqlDbType.TinyInt, DbType.Byte },
            { SqlDbType.Udt, DbType.Object },
            { SqlDbType.UniqueIdentifier, DbType.Guid },
            { SqlDbType.VarBinary, DbType.Binary },
            { SqlDbType.VarChar, DbType.AnsiString },
            { SqlDbType.Variant, DbType.Object },
            { SqlDbType.Xml, DbType.Xml }
        };

        #endregion


        #region Constructors

        /// <summary>
        /// Vytvorí inštanciu schémy stĺpca s menom <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Meno stĺpca.</param>
        /// <exception cref="System.ArgumentNullException">Meno stĺpca <paramref name="name"/> má hodnotu <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">Meno stĺpca <paramref name="name"/> nie je zadané: je prázdny reťazec,
        /// alebo reťazec bielych znakov.</exception>
        public SqlServerColumnSchema(string name)
            : this(name, DefaultAllowNull, DefaultDefaultValue, DefaultSize)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu schémy stĺpca s menom <paramref name="name"/> a s povolením <b>NULL</b> hodnôt podľa
        /// <paramref name="allowNull"/>.
        /// </summary>
        /// <param name="name">Meno stĺpca.</param>
        /// <param name="allowNull">Určuje, či stĺpec má povolenú <b>NULL</b> hodnotu.</param>
        /// <exception cref="System.ArgumentNullException">Meno stĺpca <paramref name="name"/> má hodnotu <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">Meno stĺpca <paramref name="name"/> nie je zadané: je prázdny reťazec,
        /// alebo reťazec bielych znakov.</exception>
        public SqlServerColumnSchema(string name, bool allowNull)
            : this(name, allowNull, DefaultDefaultValue, DefaultSize)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu schémy stĺpca s menom <paramref name="name"/> a ostatnými hodnotami.
        /// </summary>
        /// <param name="name">Meno stĺpca.</param>
        /// <param name="allowNull">Určuje, či stĺpec má povolenú <b>NULL</b> hodnotu.</param>
        /// <param name="defaultValue">Predvolená hodnota stĺpca.</param>
        /// <exception cref="System.ArgumentNullException">Meno stĺpca <paramref name="name"/> má hodnotu <c>null</c>
        /// alebo je to reťazec bielych znakov.</exception>
        public SqlServerColumnSchema(string name, bool allowNull, object defaultValue)
            : this(name, allowNull, defaultValue, DefaultSize)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu schémy stĺpca s menom <paramref name="name"/> a ostatnými hodnotami.
        /// </summary>
        /// <param name="name">Meno stĺpca.</param>
        /// <param name="allowNull">Určuje, či stĺpec má povolenú <b>NULL</b> hodnotu.</param>
        /// <param name="defaultValue">Predvolená hodnota stĺpca.</param>
        /// <param name="size">Maximálna dĺžka textových stĺpcov. Ak je neobmedzená, hodnota je <b>0</b>.</param>
        /// <exception cref="System.ArgumentNullException">Meno stĺpca <paramref name="name"/> má hodnotu <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">Meno stĺpca <paramref name="name"/> nie je zadané: je prázdny reťazec,
        /// alebo reťazec bielych znakov.</exception>
        public SqlServerColumnSchema(string name, bool allowNull, object defaultValue, int size)
            : base(name, allowNull, defaultValue, size)
        {
        }

        #endregion


        #region Common

        /// <summary>
        /// Dátový typ stĺpca ako hodnota enumerátu <see cref="SqlDbType"/>.
        /// </summary>
        public SqlDbType SqlDbType { get; set; }

        /// <inheritdoc/>
        public override DbType DbType
        {
            get {
                return _dbTypeMapping[SqlDbType];
            }
        }

        /// <inheritdoc/>
        /// <exception cref="System.ArgumentException">Hodnota <paramref name="param"/> nie je typu <see cref="SqlParameter"/>.
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
