using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace Kros.Data.Schema.MsAccess
{
    /// <summary>
    /// Schéma stĺpca MS Access databázy.
    /// </summary>
    public class MsAccessColumnSchema
        : ColumnSchema
    {
        #region Fields

        // Prevzaté z .NET z internej triedy System.Data.OleDb.NativeDBType.
        private static readonly Dictionary<OleDbType, DbType> _dbTypeMapping = new Dictionary<OleDbType, DbType>() {
            { OleDbType.BigInt, DbType.Int64 },
            { OleDbType.Binary, DbType.Binary },
            { OleDbType.Boolean, DbType.Boolean },
            { OleDbType.BSTR, DbType.String },
            { OleDbType.Currency, DbType.Currency },
            { OleDbType.Date, DbType.DateTime },
            { OleDbType.DBDate, DbType.Date },
            { OleDbType.DBTime, DbType.Time },
            { OleDbType.DBTimeStamp, DbType.DateTime },
            { OleDbType.Decimal, DbType.Decimal },
            { OleDbType.Double, DbType.Double },
            { OleDbType.Empty, DbType.Object },
            { OleDbType.Error, DbType.Int32 },
            { OleDbType.Filetime, DbType.DateTime },
            { OleDbType.Guid, DbType.Guid },
            { OleDbType.Char, DbType.AnsiStringFixedLength },
            { OleDbType.IDispatch, DbType.Object },
            { OleDbType.Integer, DbType.Int32 },
            { OleDbType.IUnknown, DbType.Object },
            { OleDbType.LongVarBinary, DbType.Binary },
            { OleDbType.LongVarChar, DbType.AnsiString },
            { OleDbType.LongVarWChar, DbType.String },
            { OleDbType.Numeric, DbType.Decimal },
            { OleDbType.PropVariant, DbType.Object },
            { OleDbType.Single, DbType.Single },
            { OleDbType.SmallInt, DbType.Int16 },
            { OleDbType.TinyInt, DbType.SByte },
            { OleDbType.UnsignedBigInt, DbType.UInt64 },
            { OleDbType.UnsignedInt, DbType.UInt32 },
            { OleDbType.UnsignedSmallInt, DbType.UInt16 },
            { OleDbType.UnsignedTinyInt, DbType.Byte },
            { OleDbType.VarBinary, DbType.Binary },
            { OleDbType.VarChar, DbType.AnsiString },
            { OleDbType.Variant, DbType.Object },
            { OleDbType.VarNumeric, DbType.VarNumeric },
            { OleDbType.VarWChar, DbType.String },
            { OleDbType.WChar, DbType.StringFixedLength }
        };

        #endregion


        #region Constructors

        /// <summary>
        /// Vytvorí inštanciu schémy stĺpca s menom <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Meno stĺpca.</param>
        /// <exception cref="ArgumentNullException">Meno stĺpca <paramref name="name"/> má hodnotu <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Meno stĺpca <paramref name="name"/> nie je zadané: je prázdny reťazec,
        /// alebo reťazec bielych znakov.</exception>
        public MsAccessColumnSchema(string name)
            : this(name, DefaultAllowNull, DefaultDefaultValue, DefaultSize)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu schémy stĺpca s menom <paramref name="name"/> a s povolením <b>NULL</b> hodnôt podľa
        /// <paramref name="allowNull"/>.
        /// </summary>
        /// <param name="name">Meno stĺpca.</param>
        /// <param name="allowNull">Určuje, či stĺpec má povolenú <b>NULL</b> hodnotu.</param>
        /// <exception cref="ArgumentNullException">Meno stĺpca <paramref name="name"/> má hodnotu <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Meno stĺpca <paramref name="name"/> nie je zadané: je prázdny reťazec,
        /// alebo reťazec bielych znakov.</exception>
        public MsAccessColumnSchema(string name, bool allowNull)
            : this(name, allowNull, DefaultDefaultValue, DefaultSize)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu schémy stĺpca s menom <paramref name="name"/> a ostatnými hodnotami.
        /// </summary>
        /// <param name="name">Meno stĺpca.</param>
        /// <param name="allowNull">Určuje, či stĺpec má povolenú <b>NULL</b> hodnotu.</param>
        /// <param name="defaultValue">Predvolená hodnota stĺpca.</param>
        /// <exception cref="ArgumentNullException">Meno stĺpca <paramref name="name"/> má hodnotu <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Meno stĺpca <paramref name="name"/> nie je zadané: je prázdny reťazec,
        /// alebo reťazec bielych znakov.</exception>
        public MsAccessColumnSchema(string name, bool allowNull, object defaultValue)
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
        /// <exception cref="ArgumentNullException">Meno stĺpca <paramref name="name"/> má hodnotu <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Meno stĺpca <paramref name="name"/> nie je zadané: je prázdny reťazec,
        /// alebo reťazec bielych znakov.</exception>
        public MsAccessColumnSchema(string name, bool allowNull, object defaultValue, int size)
            : base(name, allowNull, defaultValue, size)
        {
        }

        #endregion


        #region Common

        /// <summary>
        /// Dátový typ stĺpca ako hodnota enumerátu <see cref="OleDbType"/>.
        /// </summary>
        public OleDbType OleDbType { get; set; }

        /// <inheritdoc/>
        public override DbType DbType
        {
            get
            {
                return _dbTypeMapping[OleDbType];
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">Hodnota <paramref name="param"/> nie je typu <see cref="OleDbParameter"/>.
        /// </exception>
        public override void SetParameterDbType(IDataParameter param)
        {
            Check.IsOfType<OleDbParameter>(param, nameof(param));
            (param as OleDbParameter).OleDbType = OleDbType;
        }

        /// <summary>
        /// Vráti reťazec, popisujúci stĺpec.
        /// </summary>
        /// <returns>Reťazec, napríklad: "Column Invoices.Id: OleDbType = Integer, AllowNull = False, DefaultValue = NULL, Size = 0"</returns>
        public override string ToString()
        {
            return string.Format("Column {0}: OleDbType = {1}, AllowNull = {2}, DefaultValue = {3}, Size = {4}",
                FullName, OleDbType, AllowNull, ToStringDefaultValue(), Size);
        }

        #endregion
    }
}
