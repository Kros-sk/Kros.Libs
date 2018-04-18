using System.Data;

namespace Kros.Data.Schema.SqlServer
{
    /// <summary>
    /// Argumenty pre udalosť <see cref="SqlServerSchemaLoader.ParseDefaultValue"/>.
    /// </summary>
    public class SqlServerParseDefaultValueEventArgs
        : System.EventArgs
    {

        /// <summary>
        /// Vytvorí a inicializuje inštanciu argumentov.
        /// </summary>
        /// <param name="tableName">Meno tabuľky ktorej schéma sa načítava.</param>
        /// <param name="columnName">Meno stĺpca, ktorého predvolená hodnota sa parsuje.</param>
        /// <param name="sqlDbType">Dátový typ stĺpca, ktorého predvolená hodnota sa parsuje.</param>
        /// <param name="defaultValueString">Predvolená hodnota stĺpca - reťazec, ktorý sa parsuje.</param>
        /// <param name="defaultValue">Hodnota, ktorá bola získaná štandardným parserom.</param>
        public SqlServerParseDefaultValueEventArgs(
            string tableName,
            string columnName,
            SqlDbType sqlDbType,
            string defaultValueString,
            object defaultValue)
        {
            TableName = tableName;
            ColumnName = columnName;
            SqlDbType = sqlDbType;
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
        public SqlDbType SqlDbType { get; }

        /// <summary>
        /// Predvolená hodnota stĺpca - reťazec, ktorý sa parsuje.
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
