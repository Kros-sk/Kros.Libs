using Kros.Utils;
using Kros.MsAccess.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Kros.Data.MsAccess;

namespace Kros.Data.Schema.MsAccess
{
    /// <summary>
    /// Implementácia <see cref="IDatabaseSchemaLoader{T}"/>, ktorá načítava schému MS Access databáz.
    /// </summary>
    public partial class MsAccessSchemaLoader
        : IDatabaseSchemaLoader<OleDbConnection>
    {

        #region Helper mappings

        private static readonly Dictionary<OleDbType, object> _defaultValueMapping = new Dictionary<OleDbType, object>() {
            { OleDbType.BigInt, ColumnSchema.DefaultValues.Int64 },
            { OleDbType.Binary, ColumnSchema.DefaultValues.Null },
            { OleDbType.Boolean, ColumnSchema.DefaultValues.Boolean },
            { OleDbType.BSTR, ColumnSchema.DefaultValues.Text },
            { OleDbType.Currency, ColumnSchema.DefaultValues.Decimal },
            { OleDbType.Date, ColumnSchema.DefaultValues.DateTime },
            { OleDbType.DBDate, ColumnSchema.DefaultValues.Date },
            { OleDbType.DBTime, ColumnSchema.DefaultValues.Time },
            { OleDbType.DBTimeStamp, ColumnSchema.DefaultValues.DateTime },
            { OleDbType.Decimal, ColumnSchema.DefaultValues.Decimal },
            { OleDbType.Double, ColumnSchema.DefaultValues.Double },
            { OleDbType.Empty, ColumnSchema.DefaultValues.Null },
            { OleDbType.Error, ColumnSchema.DefaultValues.Int32 },
            { OleDbType.Filetime, ColumnSchema.DefaultValues.DateTime },
            { OleDbType.Guid, ColumnSchema.DefaultValues.Guid },
            { OleDbType.Char, ColumnSchema.DefaultValues.Text },
            { OleDbType.IDispatch, ColumnSchema.DefaultValues.Null },
            { OleDbType.Integer, ColumnSchema.DefaultValues.Int32 },
            { OleDbType.IUnknown, ColumnSchema.DefaultValues.Null },
            { OleDbType.LongVarBinary, ColumnSchema.DefaultValues.Null },
            { OleDbType.LongVarChar, ColumnSchema.DefaultValues.Text },
            { OleDbType.LongVarWChar, ColumnSchema.DefaultValues.Text },
            { OleDbType.Numeric, ColumnSchema.DefaultValues.Decimal },
            { OleDbType.PropVariant, ColumnSchema.DefaultValues.Null },
            { OleDbType.Single, ColumnSchema.DefaultValues.Single },
            { OleDbType.SmallInt, ColumnSchema.DefaultValues.Int16 },
            { OleDbType.TinyInt, ColumnSchema.DefaultValues.SByte },
            { OleDbType.UnsignedBigInt, ColumnSchema.DefaultValues.UInt64 },
            { OleDbType.UnsignedInt, ColumnSchema.DefaultValues.UInt32 },
            { OleDbType.UnsignedSmallInt, ColumnSchema.DefaultValues.UInt16 },
            { OleDbType.UnsignedTinyInt, ColumnSchema.DefaultValues.Byte },
            { OleDbType.VarBinary, ColumnSchema.DefaultValues.Null },
            { OleDbType.VarChar, ColumnSchema.DefaultValues.Text },
            { OleDbType.Variant, ColumnSchema.DefaultValues.Null },
            { OleDbType.VarNumeric, ColumnSchema.DefaultValues.Int32 },
            { OleDbType.VarWChar, ColumnSchema.DefaultValues.Text },
            { OleDbType.WChar, ColumnSchema.DefaultValues.Text }
        };

        #endregion


        #region Events

        /// <summary>
        /// Udalosť vyvolaná pri parsovaní predvolenej hodnoty stĺpca. V obsluhe je možné predvolenú hodnotu parsovať
        /// vlastným spôsobom, ak interné parsovanie zlyhalo.
        /// </summary>
        /// <remarks>V obsluhe udalosti je možné spraviť vlatné parsovanie predvolenej hodnoty stĺpca. Ak je v obsluhe
        /// predvolená hodnota rozparsovaná, je potrebné ju nastaviť v argumente
        /// <see cref="MsAccessParseDefaultValueEventArgs.DefaultValue"/> a zároveň je potrebné nastaviť
        /// <see cref="MsAccessParseDefaultValueEventArgs.Handled"/> na <see langword="true"/>.</remarks>
        public event EventHandler<MsAccessParseDefaultValueEventArgs> ParseDefaultValue;

        /// <summary>
        /// Vyvolá udalosť <see cref="ParseDefaultValue"/>.
        /// </summary>
        /// <param name="e">Argumenty udalosti.</param>
        protected virtual void OnParseDefaultValue(MsAccessParseDefaultValueEventArgs e)
        {
            ParseDefaultValue?.Invoke(this, e);
        }

        #endregion


        #region Schema loading

        /// <summary>
        /// Kontroluje, či dokáže načítať schému zo spojenia <paramref name="connection"/>, tzn. či zadané spojenie je
        /// spojenie na MS Access databázu.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns><see langword="true"/>, ak je možné načítať schému databázy, <see langword="false"/>, ak to možné nie je.</returns>
        bool IDatabaseSchemaLoader.SupportsConnectionType(object connection)
        {
            return SupportsConnectionType(connection as OleDbConnection);
        }

        /// <summary>
        /// Načíta celú schému databázy určenej spojením <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Vráti schému celej databázy.</returns>
        /// <remarks>Štandardne sa na získanie schémy vytvorí nové spojenie na databázu podľa vstupného spojenia
        /// <paramref name="connection"/>. Ak je však vstupné spojenie exkluzívne, použije sa priamo.</remarks>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Spojenie <paramref name="connection"/> nie je spojenie na MS Access databázu.
        /// </exception>
        DatabaseSchema IDatabaseSchemaLoader.LoadSchema(object connection)
        {
            CheckConnection(connection);
            return LoadSchema(connection as OleDbConnection);
        }

        /// <summary>
        /// Načíta schému tabuľky <paramref name="tableName"/> z databázy <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <param name="tableName">Meno tabuľky, ktorej schéma sa načíta.</param>
        /// <returns>Vráti načítanú schému tabuľky, alebo hodnotu <c>null</c>, ak taká tabuľka neexistuje.</returns>
        /// <remarks>Štandardne sa na získanie schémy vytvorí nové spojenie na databázu podľa vstupného spojenia
        /// <paramref name="connection"/>. Ak je však vstupné spojenie exkluzívne, použije sa priamo.</remarks>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Spojenie <paramref name="connection"/> nie je spojenie na MS Access databázu.
        /// </exception>
        /// <exception cref="ArgumentException">Názov tabuľky <paramref name="tableName"/> má hodnotu <c>null</c>,
        /// alebo je to prázdny reťazec, alebo je zložený len z bielych znakov.</exception>
        TableSchema IDatabaseSchemaLoader.LoadTableSchema(object connection, string tableName)
        {
            CheckConnection(connection);
            return LoadTableSchema(connection as OleDbConnection, tableName);
        }

        /// <summary>
        /// Kontroluje, či dokáže načítať schému zo spojenia <paramref name="connection"/>, tzn. či zadané spojenie je
        /// spojenie na MS Access databázu.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns><see langword="true"/>, ak je možné načítať schému databázy, <see langword="false"/>, ak to možné nie je.</returns>
        public bool SupportsConnectionType(OleDbConnection connection)
        {
            return (connection != null) && MsAccessDataHelper.IsMsAccessConnection(connection);
        }

        /// <summary>
        /// Načíta celú schému databázy určenej spojením <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Vráti schému celej databázy.</returns>
        /// <remarks>Štandardne sa na získanie schémy vytvorí nové spojenie na databázu podľa vstupného spojenia
        /// <paramref name="connection"/>. Ak je však vstupné spojenie exkluzívne, použije sa priamo.</remarks>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Spojenie <paramref name="connection"/> nie je spojenie na MS Access databázu.
        /// </exception>
        public DatabaseSchema LoadSchema(OleDbConnection connection)
        {
            CheckConnection(connection);

            if (MsAccessDataHelper.IsExclusiveMsAccessConnection(connection.ConnectionString))
            {
                return LoadSchemaCore(connection);
            }

            using (OleDbConnection cn = (OleDbConnection)(connection as ICloneable).Clone())
            {
                cn.Open();
                return LoadSchemaCore(cn);
            }
        }

        /// <summary>
        /// Načíta schému tabuľky <paramref name="tableName"/> z databázy <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <param name="tableName">Meno tabuľky, ktorej schéma sa načíta.</param>
        /// <returns>Vráti načítanú schému tabuľky, alebo hodnotu <c>null</c>, ak taká tabuľka neexistuje.</returns>
        /// <remarks>Štandardne sa na získanie schémy vytvorí nové spojenie na databázu podľa vstupného spojenia
        /// <paramref name="connection"/>. Ak je však vstupné spojenie exkluzívne, použije sa priamo.</remarks>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Spojenie <paramref name="connection"/> nie je spojenie na MS Access databázu.
        /// </exception>
        /// <exception cref="ArgumentException">Názov tabuľky <paramref name="tableName"/> má hodnotu <c>null</c>,
        /// alebo je to prázdny reťazec, alebo je zložený len z bielych znakov.</exception>
        public TableSchema LoadTableSchema(OleDbConnection connection, string tableName)
        {
            CheckConnection(connection);
            Check.NotNullOrWhiteSpace(tableName, nameof(tableName));

            if (MsAccessDataHelper.IsExclusiveMsAccessConnection(connection.ConnectionString))
            {
                return LoadTableSchemaCore(connection, tableName);
            }

            using (OleDbConnection cn = (OleDbConnection)(connection as ICloneable).Clone())
            {
                cn.Open();
                return LoadTableSchemaCore(cn, tableName);
            }
        }

        private DatabaseSchema LoadSchemaCore(OleDbConnection connection)
        {
            OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(connection.ConnectionString);
            DatabaseSchema database = new DatabaseSchema(builder.DataSource);
            LoadTables(connection, database);
            LoadColumns(connection, database);
            LoadIndexes(connection, database);

            return database;
        }

        #endregion


        #region Table schema loading

        private TableSchema LoadTableSchemaCore(OleDbConnection connection, string tableName)
        {
            TableSchema table = null;

            using (DataTable schemaData = GetSchemaTables(connection, tableName))
            {
                if (schemaData.Rows.Count == 1)
                {
                    table = new TableSchema(tableName);
                    using (DataTable columnsSchemaData = GetSchemaColumns(connection, tableName))
                    {
                        LoadColumns(table, columnsSchemaData);
                    }
                }
            }
            return table;
        }

        private void LoadTables(OleDbConnection connection, DatabaseSchema database)
        {
            using (DataTable schemaData = GetSchemaTables(connection))
            {
                foreach (DataRow row in schemaData.Rows)
                {
                    database.Tables.Add(row.Field<string>(TablesSchemaNames.TableName));
                }
            }
        }

        private void LoadColumns(OleDbConnection connection, DatabaseSchema database)
        {
            using (DataTable schemaData = GetSchemaColumns(connection))
            {
                foreach (TableSchema table in database.Tables)
                {
                    LoadColumns(table, schemaData);
                }
            }
        }

        private void LoadColumns(TableSchema table, DataTable columnsSchemaData)
        {
            columnsSchemaData.DefaultView.RowFilter = $"{ColumnsSchemaNames.TableName} = '{table.Name}'";
            foreach (DataRowView rowView in columnsSchemaData.DefaultView)
            {
                table.Columns.Add(CreateColumnSchema(rowView.Row, table));
            }
        }

        private MsAccessColumnSchema CreateColumnSchema(DataRow row, TableSchema table)
        {
            MsAccessColumnSchema column = new MsAccessColumnSchema(row.Field<string>(ColumnsSchemaNames.ColumnName));
            column.AllowNull = row.Field<bool>(ColumnsSchemaNames.IsNullable);
            column.OleDbType = GetOleDbType(row);
            column.DefaultValue = GetDefaultValue(row, column, table);
            if (!row.IsNull(ColumnsSchemaNames.CharacterMaximumLength))
            {
                column.Size = (int)row.Field<long>(ColumnsSchemaNames.CharacterMaximumLength);
            }

            return column;
        }

        private OleDbType GetOleDbType(DataRow row)
        {
            return (OleDbType)(row.Field<int>(ColumnsSchemaNames.DataType));
        }

        private object GetDefaultValue(DataRow row, MsAccessColumnSchema column, TableSchema table)
        {
            object defaultValue = null;
            string defaultValueString = null;

            if (row.IsNull(ColumnsSchemaNames.ColumnDefault))
            {
                defaultValue = column.AllowNull ? DBNull.Value : _defaultValueMapping[column.OleDbType];
            }
            else
            {
                defaultValueString = GetDefaultValueString(row.Field<string>(ColumnsSchemaNames.ColumnDefault));
                defaultValue = GetDefaultValueFromString(defaultValueString, column.OleDbType);
            }

            MsAccessParseDefaultValueEventArgs e = new MsAccessParseDefaultValueEventArgs(
                table.Name, column.Name, column.OleDbType, defaultValueString, defaultValue);
            OnParseDefaultValue(e);
            if (e.Handled)
            {
                defaultValue = e.DefaultValue;
            }

            if ((defaultValue == null) || (defaultValue == DBNull.Value))
            {
                return column.AllowNull ? DBNull.Value : _defaultValueMapping[column.OleDbType];
            }
            return defaultValue;
        }

        /// <summary>
        /// Upraví reťazec predvolenej hodnoty, ktorý je uložený priamo v databáze, aby bol vhodný na parsovanie.
        /// </summary>
        /// <param name="rawDefaultValueString">Reťazec predvolenej hodnoty stĺpca uložený priamo v databáze.</param>
        /// <returns>Upravený reťazec. Z oboch koncov vstupného reťazca odstráni apostrofy a úvodzovky.</returns>
        protected virtual string GetDefaultValueString(string rawDefaultValueString)
        {
            // Predvolená hodnota je uložená tak, že je uzatvorená v apostrofoch a v prípade textových stĺpcov
            // ešte v úvodzovkách. Ich odstránenie - Trim - je potrebné robiť na dvakrát.
            return rawDefaultValueString.Trim('\'').Trim('"');
        }

        private object GetDefaultValueFromString(string defaultValueString, OleDbType dataType)
        {
            object result = null;

            if ((dataType == OleDbType.VarChar) ||
                (dataType == OleDbType.LongVarChar) ||
                (dataType == OleDbType.VarWChar) ||
                (dataType == OleDbType.LongVarWChar) ||
                (dataType == OleDbType.Char) ||
                (dataType == OleDbType.WChar))
            {
                result = defaultValueString; ;
            }
            else
            {
                result = GetParseFunction(dataType)?.Invoke(defaultValueString);
            }

            return result;
        }

        private DefaultValueParsers.ParseDefaultValueFunction GetParseFunction(OleDbType dataType)
        {
            switch (dataType)
            {
                case OleDbType.BigInt:
                    return DefaultValueParsers.ParseInt64;

                case OleDbType.Integer:
                    return DefaultValueParsers.ParseInt32;

                case OleDbType.SmallInt:
                    return DefaultValueParsers.ParseInt16;

                case OleDbType.TinyInt:
                    return DefaultValueParsers.ParseSByte;

                case OleDbType.Double:
                case OleDbType.Numeric:
                    return DefaultValueParsers.ParseDouble;

                case OleDbType.Single:
                    return DefaultValueParsers.ParseSingle;

                case OleDbType.Decimal:
                case OleDbType.Currency:
                    return DefaultValueParsers.ParseDecimal;

                case OleDbType.UnsignedBigInt:
                    return DefaultValueParsers.ParseUInt64;

                case OleDbType.UnsignedInt:
                    return DefaultValueParsers.ParseUInt32;

                case OleDbType.UnsignedSmallInt:
                    return DefaultValueParsers.ParseUInt16;

                case OleDbType.UnsignedTinyInt:
                    return DefaultValueParsers.ParseByte;

                case OleDbType.Guid:
                    return DefaultValueParsers.ParseGuid;

                case OleDbType.Boolean:
                    return DefaultValueParsers.ParseBool;

                case OleDbType.Date:
                    return DefaultValueParsers.ParseDate;
            }

            return null;
        }

        #endregion


        #region Index schema loading

        private void LoadIndexes(OleDbConnection connection, DatabaseSchema database)
        {
            using (DataTable schemaData = connection.GetSchema(SchemaNames.Indexes))
            {
                foreach (TableSchema table in database.Tables)
                {
                    schemaData.DefaultView.RowFilter = $"{IndexesSchemaNames.TableName} = '{table.Name}'";
                    schemaData.DefaultView.Sort = $"{IndexesSchemaNames.IndexName}, {IndexesSchemaNames.OrdinalPosition}";
                    if (schemaData.DefaultView.Count > 0)
                    {
                        LoadIndexesForTable(table, schemaData.DefaultView);
                    }
                }
            }
        }

        private void LoadIndexesForTable(TableSchema table, DataView schemaData)
        {
            string lastIndexName = string.Empty;
            IndexSchema index = null;

            foreach (DataRowView rowView in schemaData)
            {
                string indexName = rowView.Row.Field<string>(IndexesSchemaNames.IndexName);
                if (indexName != lastIndexName)
                {
                    lastIndexName = indexName;
                    index = CreateIndexSchema(table, rowView.Row);
                }
                AddColumnToIndex(index, rowView.Row);
            }
        }

        private IndexSchema CreateIndexSchema(TableSchema table, DataRow row)
        {
            if (row.Field<bool>(IndexesSchemaNames.PrimaryKey))
            {
                return table.PrimaryKey;
            }
            else
            {
                return table.Indexes.Add(
                    row.Field<string>(IndexesSchemaNames.IndexName),
                    row.Field<bool>(IndexesSchemaNames.Unique) ? IndexType.UniqueKey : IndexType.Index,
                    row.Field<bool>(IndexesSchemaNames.Clustered));
            }
        }

        private void AddColumnToIndex(IndexSchema index, DataRow row)
        {
            index.Columns.Add(
                row.Field<string>(IndexesSchemaNames.ColumnName),
                row.Field<short>(IndexesSchemaNames.Collation) == 2 ? SortOrder.Descending : SortOrder.Ascending);
        }

        #endregion


        #region Helpers

        private void CheckConnection(object connection)
        {
            Check.NotNull(connection, nameof(connection));
            if (!(this as IDatabaseSchemaLoader).SupportsConnectionType(connection))
            {
                throw new ArgumentException(Resources.MsAccessSchemaLoader_UnsupportedConnectionType, nameof(connection));
            }
        }

        private DataTable GetSchemaTables(OleDbConnection connection)
        {
            return GetSchemaTables(connection, null);
        }

        private DataTable GetSchemaTables(OleDbConnection connection, string tableName)
        {
            return connection.GetSchema(SchemaNames.Tables, new string[] { null, null, tableName, TableTypes.Table });
        }

        private DataTable GetSchemaColumns(OleDbConnection connection)
        {
            return GetSchemaColumns(connection, null);
        }

        private DataTable GetSchemaColumns(OleDbConnection connection, string tableName)
        {
            DataTable schemaData = connection.GetSchema(SchemaNames.Columns, new string[] { null, null, tableName, null });
            schemaData.DefaultView.Sort =
                $"{ColumnsSchemaNames.TableSchema}, {ColumnsSchemaNames.TableName}, {ColumnsSchemaNames.OrdinalPosition}";
            return schemaData;
        }

        #endregion

    }
}
