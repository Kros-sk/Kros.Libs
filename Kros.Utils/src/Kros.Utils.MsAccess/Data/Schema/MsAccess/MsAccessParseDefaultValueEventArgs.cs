using System.Data.OleDb;

namespace Kros.Data.Schema.MsAccess
{
    /// <summary>
    /// Argumenty pre udalosť <see cref="MsAccessSchemaLoader.ParseDefaultValue"/>.
    /// </summary>
    public class MsAccessParseDefaultValueEventArgs
        : System.EventArgs
    {
        /// <summary>
        /// Vytvorí inštanciu so zadanými parametrami.
        /// </summary>
        /// <param name="tableName">Meno tabuľky ktorej schéma sa načítava.</param>
        /// <param name="columnName">Meno stĺpca, ktorého predvolená hodnota sa parsuje.</param>
        /// <param name="oleDbType">Dátový typ stĺpca, ktorého predvolená hodnota sa parsuje.</param>
        /// <param name="defaultValueString">Predvolená hodnota stĺpca, tzn. reťazec, ktorý sa parsuje.</param>
        /// <param name="defaultValue">Hodnota, ktorá bola získaná štandardným parserom.</param>
        public MsAccessParseDefaultValueEventArgs(
            string tableName,
            string columnName,
            OleDbType oleDbType,
            string defaultValueString,
            object defaultValue)
        {
            TableName = tableName;
            ColumnName = columnName;
            OleDbType = oleDbType;
            DefaultValueString = defaultValueString;
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Meno tabuľky ktorej schéma sa načítava.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Meno stĺpca, ktorého predvolená hodnota sa parsuje.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Dátový typ stĺpca, ktorého predvolená hodnota sa parsuje.
        /// </summary>
        public OleDbType OleDbType { get; }

        /// <summary>
        /// Predvolená hodnota stĺpca, tzn. reťazec, ktorý sa parsuje.
        /// </summary>
        public string DefaultValueString { get; }

        /// <summary>
        /// Hodnota, ktorá bola získaná štandardným parserom. Pri vlastnej obsluhe je potrebné tu nastaviť vlastnú hodnotu.
        /// </summary>
        public object DefaultValue { get; set; } = null;

        /// <summary>
        /// Je potrebné nastaviť na <see langword="true"/>, ak bola v obsluhe udalosti nastavená vlastná hodnota <see cref="DefaultValue"/>.
        /// </summary>
        public bool Handled { get; set; } = false;

    }
}
