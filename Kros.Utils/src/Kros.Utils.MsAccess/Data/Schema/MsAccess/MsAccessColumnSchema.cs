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
