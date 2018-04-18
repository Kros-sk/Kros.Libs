using Kros.Utils;
using System;
using System.Data;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Schéma stĺpca databázovej tabuľky.
    /// </summary>
    public abstract class ColumnSchema
    {

        #region Constants

        /// <summary>
        /// Predvolená hodnota stĺpca, ktorá sa použije, ak nie je žiadna definovaná. Hodnota je <see cref="DBNull"/>.
        /// </summary>
        public static readonly object DefaultDefaultValue = DBNull.Value;

        /// <summary>
        /// Predvolené nastavenie pre <see cref="ColumnSchema.AllowNull"/>. Hodnota je <see langword="false"/>.
        /// </summary>
        public const bool DefaultAllowNull = true;

        /// <summary>
        /// Predvolená hodnota pre <see cref="ColumnSchema.Size"/>. Hodnota je <c>0</c>.
        /// </summary>
        public const int DefaultSize = 0;

        /// <summary>
        /// Predvolené hodnoty stĺpcov pre jednotlivé dátové typy:
        /// <list type="bullet">
        /// <item>Boolean typ má predvolenú hodnotu <see langword="false"/>.</item>
        /// <item>Všetky číselné typy majú hodnotu <c>0</c>.</item>
        /// <item>Dátumové a časové typy sú nastavené na <c>1.1.1900 0:00:00</c></item>
        /// <item>Textové dátové typy sú nastavené na prázdny reťazec.</item>
        /// <item>GUID typ je nastavený na prázdny GUID (<see cref="Guid.Empty"/>)</item>
        /// </list>
        /// </summary>
        public static class DefaultValues
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public const bool Boolean = false;
            public const sbyte SByte = 0;
            public const short Int16 = 0;
            public const int Int32 = 0;
            public const long Int64 = 0L;
            public const byte Byte = 0;
            public const ushort UInt16 = 0;
            public const uint UInt32 = 0;
            public const ulong UInt64 = 0L;
            public const float Single = 0.0F;
            public const double Double = 0.0;
            public const decimal Decimal = 0;
            public const string Text = "";
            public static readonly Guid Guid = Guid.Empty;
            public static readonly DateTime DateTime = new DateTime(1900, 1, 1);
            public static readonly DateTime Date = DefaultValues.DateTime;
            public static readonly DateTime Time = DefaultValues.DateTime;
            public static readonly DBNull Null = DBNull.Value;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        #endregion


        #region Constructors

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
        public ColumnSchema(string name, bool allowNull, object defaultValue, int size)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            Name = name;
            AllowNull = allowNull;
            DefaultValue = defaultValue;
            Size = size;
        }

        #endregion


        #region Common

        /// <summary>
        /// Tabuľka, ktorej stĺpec patrí. Tabuľka je nastavená automaticky pri pridaní stĺpca do zoznamu
        /// <see cref="ColumnSchemaCollection"/>.
        /// </summary>
        public TableSchema Table { get; internal set; }

        /// <summary>
        /// Meno stĺpca.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Plné meno stĺpca aj s názvom tabuľky, ak stĺpec nejakej tabuľke patrí.
        /// </summary>
        public string FullName { get { return (Table == null) ? Name : $"{Table.Name}.{Name}"; } }

        /// <summary>
        /// Určuje, či stĺpec má povolenú <b>NULL</b> hodnotu.
        /// </summary>
        public bool AllowNull { get; set; } = DefaultAllowNull;

        /// <summary>
        /// Predvolená hodnota stĺpca.
        /// </summary>
        public object DefaultValue { get; set; } = DefaultDefaultValue;

        /// <summary>
        /// Maximálna dĺžka textových stĺpcov. Ak je neobmedzená, hodnota je <b>0</b>.
        /// </summary>
        public int Size { get; set; } = DefaultSize;

        /// <summary>
        /// Dátový typ stĺpca ako hodnota enumerátu <see cref="DbType"/>.
        /// </summary>
        public abstract DbType DbType { get; }

        /// <summary>
        /// Parametru <paramref name="param"/> nastaví dátový typ.
        /// </summary>
        /// <param name="param">Parameter pre databázové príkazy <see cref="IDbCommand"/>.</param>
        public abstract void SetParameterDbType(IDataParameter param);

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
        {
            return string.Format("Column {0}: DbType = {1}, AllowNull = {2}, DefaultValue = {3}, Size = {4}",
                FullName, DbType, AllowNull, ToStringDefaultValue(), Size);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Vráti predvolenú hodnotu stĺpca. Tá sapoužije v metóde <see cref="ToString"/>.
        /// </summary>
        /// <returns>Ak predvolená hodnota je <see cref="DBNull"/>, je vrátený reťazec <c>NULL</c>. Inak je vrátená
        /// samotná predvolená hodnota <see cref="DefaultValue"/>.</returns>
        protected virtual object ToStringDefaultValue()
        {
            return (DefaultValue == DBNull.Value) ? "NULL" : DefaultValue;
        }

        #endregion

    }
}
