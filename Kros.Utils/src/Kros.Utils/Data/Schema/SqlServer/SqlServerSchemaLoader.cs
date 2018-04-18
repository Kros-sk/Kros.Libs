using Kros.Properties;
using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Kros.Data.Schema.SqlServer
{
    /// <summary>
    /// Implementácia <see cref="IDatabaseSchemaLoader{T}"/>, ktorá načítava schému SQL Server databáz.
    /// </summary>
    public partial class SqlServerSchemaLoader
        : IDatabaseSchemaLoader<SqlConnection>
    {

        #region Helper mappings

        private static readonly Dictionary<SqlDbType, object> _defaultValueMapping = new Dictionary<SqlDbType, object>() {
            { SqlDbType.BigInt, ColumnSchema.DefaultValues.Int64 },
            { SqlDbType.Binary, ColumnSchema.DefaultValues.Null },
            { SqlDbType.Bit, ColumnSchema.DefaultValues.Boolean },
            { SqlDbType.Char, ColumnSchema.DefaultValues.Text },
            { SqlDbType.DateTime, ColumnSchema.DefaultValues.DateTime },
            { SqlDbType.Decimal, ColumnSchema.DefaultValues.Decimal },
            { SqlDbType.Float, ColumnSchema.DefaultValues.Double },
            { SqlDbType.Image, ColumnSchema.DefaultValues.Null },
            { SqlDbType.Int, ColumnSchema.DefaultValues.Int32 },
            { SqlDbType.Money, ColumnSchema.DefaultValues.Decimal },
            { SqlDbType.NChar, ColumnSchema.DefaultValues.Text },
            { SqlDbType.NText, ColumnSchema.DefaultValues.Text },
            { SqlDbType.NVarChar, ColumnSchema.DefaultValues.Text },
            { SqlDbType.Real, ColumnSchema.DefaultValues.Single },
            { SqlDbType.UniqueIdentifier, ColumnSchema.DefaultValues.Guid },
            { SqlDbType.SmallDateTime, ColumnSchema.DefaultValues.DateTime },
            { SqlDbType.SmallInt, ColumnSchema.DefaultValues.Int16 },
            { SqlDbType.SmallMoney, ColumnSchema.DefaultValues.Decimal },
            { SqlDbType.Text, ColumnSchema.DefaultValues.Text },
            { SqlDbType.Timestamp, ColumnSchema.DefaultValues.Null },
            { SqlDbType.TinyInt, ColumnSchema.DefaultValues.Byte },
            { SqlDbType.VarBinary, ColumnSchema.DefaultValues.Null },
            { SqlDbType.VarChar, ColumnSchema.DefaultValues.Text },
            { SqlDbType.Variant, ColumnSchema.DefaultValues.Null },
            { SqlDbType.Xml, ColumnSchema.DefaultValues.Null },
            { SqlDbType.Udt, ColumnSchema.DefaultValues.Null },
            { SqlDbType.Structured, ColumnSchema.DefaultValues.Null },
            { SqlDbType.Date, ColumnSchema.DefaultValues.Date },
            { SqlDbType.Time, ColumnSchema.DefaultValues.Time },
            { SqlDbType.DateTime2, ColumnSchema.DefaultValues.DateTime },
            { SqlDbType.DateTimeOffset, ColumnSchema.DefaultValues.Null }
        };

        #endregion


        #region Events

        /// <summary>
        /// Udalosť vyvolaná pri parsovaní predvolenej hodnoty stĺpca. V obsluhe je možné predvolenú hodnotu parsovať
        /// vlastným spôsobom, ak interné parsovanie zlyhalo.
        /// </summary>
        /// <remarks>V obsluhe udalosti je možné spraviť vlatné parsovanie predvolenej hodnoty stĺpca. Ak je v obsluhe
        /// predvolená hodnota rozparsovaná, je potrebné ju nastaviť v argumente
        /// <see cref="SqlServerParseDefaultValueEventArgs.DefaultValue"/> a zároveň je potrebné nastaviť
        /// <see cref="SqlServerParseDefaultValueEventArgs.Handled"/> na <see langword="true"/>.</remarks>
        public event EventHandler<SqlServerParseDefaultValueEventArgs> ParseDefaultValue;

        /// <summary>
        /// Vyvolá udalosť <see cref="ParseDefaultValue"/> s argumentami <paramref name="e"/>.
        /// </summary>
        /// <param name="e">Argumenty udalosti.</param>
        protected virtual void OnParseDefaultValue(SqlServerParseDefaultValueEventArgs e)
        {
            ParseDefaultValue?.Invoke(this, e);
        }

        #endregion


        #region Schema loading

        /// <summary>
        /// Kontroluje, či dokáže načítať schému zo spojenia <paramref name="connection"/>, tzn. či zadané spojenie je
        /// spojenie na SQL Server databázu.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns><see langword="true"/>, ak je možné načítať schému databázy, <see langword="false"/>, ak to možné nie je.</returns>
        bool IDatabaseSchemaLoader.SupportsConnectionType(object connection)
        {
            return SupportsConnectionType(connection as SqlConnection);
        }

        /// <summary>
        /// Načíta celú schému databázy určenej spojením <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Vráti schému celej databázy.</returns>
        /// <remarks>Štandardne sa na získanie schémy vytvorí nové spojenie na databázu podľa vstupného spojenia
        /// <paramref name="connection"/>. Ak je však vstupné spojenie exkluzívne, použije sa priamo.</remarks>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><list type="bullet">
        /// <item>Spojenie <paramref name="connection"/> nie je spojenie na SQL Server databázu.</item>
        /// <item>Spojenie <paramref name="connection"/> nemá nastavené meno databázy (<b>Initial Catalog</b>).</item>
        /// </list></exception>
        DatabaseSchema IDatabaseSchemaLoader.LoadSchema(object connection)
        {
            CheckConnection(connection);
            return LoadSchema(connection as SqlConnection);
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
        /// <exception cref="ArgumentException"><list type="bullet">
        /// <item>Spojenie <paramref name="connection"/> nie je spojenie na SQL Server databázu.</item>
        /// <item>Spojenie <paramref name="connection"/> nemá nastavené meno databázy (<b>Initial Catalog</b>).</item>
        /// <item>Názov tabuľky <paramref name="tableName"/> má hodnotu <c>null</c>, alebo je to prázdny reťazec,
        /// alebo je zložený len z bielych znakov.</item>
        /// </list></exception>
        TableSchema IDatabaseSchemaLoader.LoadTableSchema(object connection, string tableName)
        {
            CheckConnection(connection);
            return LoadTableSchema(connection as SqlConnection, tableName);
        }

        /// <summary>
        /// Kontroluje, či dokáže načítať schému zo spojenia <paramref name="connection"/>, tzn. či zadané spojenie je
        /// spojenie na SQL Server databázu.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns><see langword="true"/>, ak je možné načítať schému databázy, <see langword="false"/>, ak to možné nie je.</returns>
        public bool SupportsConnectionType(SqlConnection connection)
        {
            return (connection != null);
        }

        /// <summary>
        /// Načíta celú schému databázy určenej spojením <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu.</param>
        /// <returns>Vráti schému celej databázy.</returns>
        /// <remarks>Štandardne sa na získanie schémy vytvorí nové spojenie na databázu podľa vstupného spojenia
        /// <paramref name="connection"/>. Ak je však vstupné spojenie exkluzívne, použije sa priamo.</remarks>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="connection"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><list type="bullet">
        /// <item>Spojenie <paramref name="connection"/> nie je spojenie na SQL Server databázu.</item>
        /// <item>Spojenie <paramref name="connection"/> nemá nastavené meno databázy (<b>Initial Catalog</b>).</item>
        /// </list></exception>
        public DatabaseSchema LoadSchema(SqlConnection connection)
        {
            CheckConnection(connection);

            try
            {
                using (SqlConnection cn = (SqlConnection)(connection as ICloneable).Clone())
                {
                    cn.Open();
                    return LoadSchemaCore(cn);
                }
            }
            catch (Exception)
            {
                // Pokus o načítanie pomocou pôvodného spojenia, ak by bol prístup k databáze len exkluzívny.
                // Ideálne by bolo keby som vedel zistiť, či pripojeine na databázu exkluzívne, aby to nebolo
                // potrebné riešiť try-catch blokom.
                return LoadSchemaCore(connection);
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
        /// <exception cref="ArgumentException"><list type="bullet">
        /// <item>Spojenie <paramref name="connection"/> nie je spojenie na SQL Server databázu.</item>
        /// <item>Spojenie <paramref name="connection"/> nemá nastavené meno databázy (<b>Initial Catalog</b>).</item>
        /// <item>Názov tabuľky <paramref name="tableName"/> má hodnotu <c>null</c>, alebo je to prázdny reťazec,
        /// alebo je zložený len z bielych znakov.</item>
        /// </list></exception>
        public TableSchema LoadTableSchema(SqlConnection connection, string tableName)
        {
            CheckConnection(connection);
            Check.NotNullOrWhiteSpace(tableName, nameof(tableName));

            try
            {
                using (SqlConnection cn = (SqlConnection)(connection as ICloneable).Clone())
                {
                    cn.Open();
                    return LoadTableSchemaCore(cn, tableName);
                }
            }
            catch (Exception)
            {
                // Pokus o načítanie pomocou pôvodného spojenia, ak by bol prístup k databáze len exkluzívny.
                // Ideálne by bolo keby som vedel zistiť, či pripojeine na databázu exkluzívne, aby to nebolo
                // potrebné riešiť try-catch blokom.
                return LoadTableSchemaCore(connection, tableName);
            }
        }

        private DatabaseSchema LoadSchemaCore(SqlConnection connection)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connection.ConnectionString);
            DatabaseSchema database = new DatabaseSchema(builder.InitialCatalog);
            LoadTables(connection, database);
            LoadColumns(connection, database);
            LoadIndexes(connection, database);
            LoadForeignKeys(connection, database);

            return database;
        }

        #endregion


        #region Tables

        private TableSchema LoadTableSchemaCore(SqlConnection connection, string tableName)
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

        private void LoadTables(SqlConnection connection, DatabaseSchema database)
        {
            using (DataTable schemaData = GetSchemaTables(connection))
            {
                foreach (DataRow row in schemaData.Rows)
                {
                    database.Tables.Add((string)row[TablesSchemaNames.TableName]);
                }
            }
        }

        private void LoadColumns(SqlConnection connection, DatabaseSchema database)
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

        private SqlServerColumnSchema CreateColumnSchema(DataRow row, TableSchema table)
        {
            SqlServerColumnSchema column = new SqlServerColumnSchema((string)row[ColumnsSchemaNames.ColumnName])
            {
                AllowNull = ((string)row[ColumnsSchemaNames.IsNullable]).Equals("yes", StringComparison.OrdinalIgnoreCase),
                SqlDbType = GetSqlDbType(row)
            };
            column.DefaultValue = GetDefaultValue(row, column, table);
            if (!row.IsNull(ColumnsSchemaNames.CharacterMaximumLength))
            {
                column.Size = (int)row[ColumnsSchemaNames.CharacterMaximumLength];
            }

            return column;
        }

        private SqlDbType GetSqlDbType(DataRow row)
        {
            SqlDbType sqlType = SqlDbType.Int;

            string dataType = (string)row[ColumnsSchemaNames.DataType];
            if (!Enum.TryParse(dataType, true, out sqlType))
            {
                if (dataType.Equals("numeric", StringComparison.OrdinalIgnoreCase))
                {
                    return SqlDbType.Decimal;
                }
                else
                {
                    return SqlDbType.Variant;
                }
            }

            return sqlType;
        }

        private object GetDefaultValue(DataRow row, SqlServerColumnSchema column, TableSchema table)
        {
            object defaultValue = null;
            string defaultValueString = null;

            if (row.IsNull(ColumnsSchemaNames.ColumnDefault))
            {
                defaultValue = column.AllowNull ? DBNull.Value : _defaultValueMapping[column.SqlDbType];
            }
            else
            {
                defaultValueString = GetDefaultValueString((string)row[ColumnsSchemaNames.ColumnDefault]);
                defaultValue = GetDefaultValueFromString(defaultValueString, column.SqlDbType);
            }

            SqlServerParseDefaultValueEventArgs e = new SqlServerParseDefaultValueEventArgs(
                table.Name, column.Name, column.SqlDbType, defaultValueString, defaultValue);
            OnParseDefaultValue(e);
            if (e.Handled)
            {
                defaultValue = e.DefaultValue;
            }

            if ((defaultValue == null) || (defaultValue == DBNull.Value))
            {
                return column.AllowNull ? DBNull.Value : _defaultValueMapping[column.SqlDbType];
            }
            return defaultValue;
        }

        /// <summary>
        /// Upraví reťazec <paramref name="rawDefaultValueString"/> tak, aby z neho bolo možné získať predvolenú hodnotu stĺpca.
        /// </summary>
        /// <param name="rawDefaultValueString">Reťazec predvolenej hodnoty stĺpca tak, ako je uložený v databáze.</param>
        /// <returns>Upravený reťazec.</returns>
        protected virtual string GetDefaultValueString(string rawDefaultValueString)
        {
            // Predvolené hodnoty sú v schéme uložené tak zvláštne, že sú v obyčajných zátvorkách (v niektorých
            // prípadoch dokonca zdvojených) a v prípade textu ešte v apostrofoch. Teda predvolená hodnota 0 pre
            // číselný stĺpec je uložená ako "(0)" (prípadne "((0))"). Predvolená hodnota "hello" pre textový
            // stĺpec je uložená ako "('hello')", resp. "(N'hello')".
            rawDefaultValueString = rawDefaultValueString.Trim('(', ')');
            if (rawDefaultValueString.Length >= 2)
            {
                if ((rawDefaultValueString[0] == '\'') && (rawDefaultValueString[rawDefaultValueString.Length - 1] == '\''))
                {
                    return rawDefaultValueString.Substring(1, rawDefaultValueString.Length - 2);
                }
                if (((rawDefaultValueString[0] == 'N') || (rawDefaultValueString[0] == 'n')) &&
                    (rawDefaultValueString[1] == '\'') &&
                    (rawDefaultValueString[rawDefaultValueString.Length - 1] == '\''))
                {
                    return rawDefaultValueString.Substring(2, rawDefaultValueString.Length - 3);
                }
            }
            return rawDefaultValueString;
        }

        private object GetDefaultValueFromString(string defaultValueString, SqlDbType dataType)
        {
            object result = null;

            if ((dataType == SqlDbType.NText) ||
                (dataType == SqlDbType.NVarChar) ||
                (dataType == SqlDbType.Text) ||
                (dataType == SqlDbType.VarChar) ||
                (dataType == SqlDbType.NChar) ||
                (dataType == SqlDbType.Char))
            {
                result = defaultValueString;
            }
            else
            {
                result = GetParseFunction(dataType)?.Invoke(defaultValueString);
            }

            return result;
        }

        private DefaultValueParsers.ParseDefaultValueFunction GetParseFunction(SqlDbType dataType)
        {
            switch (dataType)
            {
                case SqlDbType.BigInt:
                    return DefaultValueParsers.ParseInt64;

                case SqlDbType.Int:
                    return DefaultValueParsers.ParseInt32;

                case SqlDbType.SmallInt:
                    return DefaultValueParsers.ParseInt16;

                case SqlDbType.TinyInt:
                    return DefaultValueParsers.ParseByte;

                case SqlDbType.Bit:
                    return DefaultValueParsers.ParseBool;

                case SqlDbType.Decimal:
                case SqlDbType.Money:
                    return DefaultValueParsers.ParseDecimal;

                case SqlDbType.Float:
                    return DefaultValueParsers.ParseDouble;

                case SqlDbType.Real:
                    return DefaultValueParsers.ParseSingle;

                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                    return DefaultValueParsers.ParseDateSql;

                case SqlDbType.UniqueIdentifier:
                    return DefaultValueParsers.ParseGuid;
            }

            return null;
        }

        #endregion


        #region Indexes

        private static class IndexesQueryNames
        {
            public const string TableName = "TableName";
            public const string IndexName = "IndexName";
            public const string IndexId = "IndexId";
            public const string IsUnique = "IsUnique";
            public const string IsPrimaryKey = "IsPrimaryKey";
            public const string IsDisabled = "IsDisabled";
            public const string TypDesc = "TypDesc";
            public const string ColumnName = "ColumnName";
            public const string IsDesc = "IsDesc";
            public const string ColumnId = "ColumnId";
            public const string ColumnOrdinal = "ColumnOrdinal";
        }

        private readonly string LoadIndexesQuery =
$@"SELECT
    tables.name AS {IndexesQueryNames.TableName},
    indexes.name AS {IndexesQueryNames.IndexName},
    indexes.index_id AS {IndexesQueryNames.IndexId},
    indexes.is_unique AS {IndexesQueryNames.IsUnique},
    indexes.is_primary_key AS {IndexesQueryNames.IsPrimaryKey},
    indexes.is_disabled AS {IndexesQueryNames.IsDisabled},
    indexes.type_desc AS {IndexesQueryNames.TypDesc},
    columns.name AS {IndexesQueryNames.ColumnName},
    index_columns.is_descending_key AS {IndexesQueryNames.IsDesc},
    index_columns.index_column_id AS {IndexesQueryNames.ColumnId},
    index_columns.key_ordinal AS {IndexesQueryNames.ColumnOrdinal}

FROM sys.indexes indexes

INNER JOIN sys.index_columns index_columns
    ON indexes.object_id = index_columns.object_id and indexes.index_id = index_columns.index_id

INNER JOIN sys.columns columns
    ON index_columns.object_id = columns.object_id and index_columns.column_id = columns.column_id

INNER JOIN sys.tables tables
    ON indexes.object_id = tables.object_id

ORDER BY tables.name, indexes.name, index_columns.key_ordinal
";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private void LoadIndexes(SqlConnection connection, DatabaseSchema database)
        {
            using (DataTable schemaData = new DataTable())
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(LoadIndexesQuery, connection))
                {
                    adapter.Fill(schemaData);
                }
                foreach (TableSchema table in database.Tables)
                {
                    schemaData.DefaultView.RowFilter = $"{IndexesQueryNames.TableName} = '{table.Name}'";
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
                string indexName = (string)rowView.Row[IndexesQueryNames.IndexName];
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
            if ((bool)row[IndexesQueryNames.IsPrimaryKey])
            {
                return table.PrimaryKey;
            }
            else
            {
                return table.Indexes.Add(
                    (string)row[IndexesQueryNames.IndexName],
                    (bool)row[IndexesQueryNames.IsUnique] ? IndexType.UniqueKey : IndexType.Index,
                    ((string)row[IndexesQueryNames.TypDesc]).Equals("CLUSTERED", StringComparison.OrdinalIgnoreCase));
            }
        }

        private void AddColumnToIndex(IndexSchema index, DataRow row)
        {
            index.Columns.Add(
                (string)row[IndexesQueryNames.ColumnName],
                (bool)row[IndexesQueryNames.IsDesc] ? SortOrder.Descending : SortOrder.Ascending);
        }

        #endregion


        #region Foreign keys

        private static class ForeignKeyQueryNames
        {
            public const string ForeignKeyId = "ForeignKeyId";
            public const string ForeignKeyName = "ForeignKeyName";
            public const string ReferencedTableName = "ReferencedTableName";
            public const string ParentTableName = "ParentTableName";
            public const string DeleteRule = "DeleteRule";
            public const string UpdateRule = "UpdateRule";
        }

        private static class ForeignKeyColumnsQueryNames
        {
            public const string ForeignKeyId = "ForeignKeyId";
            public const string ParentColumnName = "ParentColumnName";
            public const string ReferencedColumnName = "ReferencedColumnName";
        }

        private readonly string LoadForeignKeysQuery =
$@"SELECT
    [object_id] AS {ForeignKeyQueryNames.ForeignKeyId},
    [name] AS {ForeignKeyQueryNames.ForeignKeyName},
    OBJECT_NAME([referenced_object_id]) AS {ForeignKeyQueryNames.ReferencedTableName},
    OBJECT_NAME([parent_object_id]) AS {ForeignKeyQueryNames.ParentTableName},
    [delete_referential_action_desc] AS {ForeignKeyQueryNames.DeleteRule},
    [update_referential_action_desc] AS {ForeignKeyQueryNames.UpdateRule}

FROM sys.foreign_keys

WHERE [type_desc] = 'FOREIGN_KEY_CONSTRAINT'
";

        private readonly string LoadForeignKeyColumnsQuery =
$@"SELECT
    foreign_key_columns.constraint_object_id AS {ForeignKeyColumnsQueryNames.ForeignKeyId},
    ParentColumns.name AS {ForeignKeyColumnsQueryNames.ParentColumnName},
    ReferencedColumns.name AS {ForeignKeyColumnsQueryNames.ReferencedColumnName}

FROM sys.foreign_key_columns foreign_key_columns

INNER JOIN sys.columns ParentColumns ON
    foreign_key_columns.parent_object_id = ParentColumns.object_id AND
    foreign_key_columns.parent_column_id = ParentColumns.column_id

INNER JOIN sys.columns ReferencedColumns ON
    foreign_key_columns.referenced_object_id = ReferencedColumns.object_id AND
    foreign_key_columns.referenced_column_id = ReferencedColumns.column_id

WHERE foreign_key_columns.constraint_object_id IN (
    SELECT [object_id] FROM sys.foreign_keys WHERE [type_desc] = 'FOREIGN_KEY_CONSTRAINT'
)

ORDER BY foreign_key_columns.constraint_object_id
";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private void LoadForeignKeys(SqlConnection connection, DatabaseSchema database)
        {
            using (DataTable foreignKeysData = new DataTable("ForeignKeys"))
            using (DataTable foreignKeyColumnsData = new DataTable("ForeignKeys"))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(LoadForeignKeysQuery, connection))
                {
                    adapter.Fill(foreignKeysData);
                }
                using (SqlDataAdapter adapter = new SqlDataAdapter(LoadForeignKeyColumnsQuery, connection))
                {
                    adapter.Fill(foreignKeyColumnsData);
                }
                LoadForeignKeysSchema(database, foreignKeysData, foreignKeyColumnsData);
            }
        }

        private void LoadForeignKeysSchema(DatabaseSchema database, DataTable foreignKeysData, DataTable foreignKeyColumnsData)
        {
            DataView columnsView = foreignKeyColumnsData.DefaultView;
            List<string> primaryKeyColumns = new List<string>();
            List<string> foreignKeyColumns = new List<string>();
            foreach (DataRow fkRow in foreignKeysData.Rows)
            {
                int foreignKeyId = (int)fkRow[ForeignKeyQueryNames.ForeignKeyId];
                columnsView.RowFilter = $"[{ForeignKeyColumnsQueryNames.ForeignKeyId}] = {foreignKeyId}";

                primaryKeyColumns.Clear();
                foreignKeyColumns.Clear();
                foreach (DataRowView fkColumnRow in columnsView)
                {
                    primaryKeyColumns.Add((string)fkColumnRow.Row[ForeignKeyColumnsQueryNames.ReferencedColumnName]);
                    foreignKeyColumns.Add((string)fkColumnRow.Row[ForeignKeyColumnsQueryNames.ParentColumnName]);
                }
                ForeignKeySchema foreignKey = CreateForeignKey(fkRow, primaryKeyColumns, foreignKeyColumns);
                database.Tables[(string)fkRow[ForeignKeyQueryNames.ParentTableName]].ForeignKeys.Add(foreignKey);
            }
        }

        private ForeignKeySchema CreateForeignKey(
            DataRow foreignKeyData,
            List<string> primaryKeyColumns,
            List<string> foreignKeyColumns)
        {
            ForeignKeySchema foreignKey = new ForeignKeySchema(
                (string)foreignKeyData[ForeignKeyQueryNames.ForeignKeyName],
                (string)foreignKeyData[ForeignKeyQueryNames.ReferencedTableName],
                primaryKeyColumns,
                (string)foreignKeyData[ForeignKeyQueryNames.ParentTableName],
                foreignKeyColumns);
            foreignKey.DeleteRule = GetForeignKeyRule((string)foreignKeyData[ForeignKeyQueryNames.DeleteRule]);
            foreignKey.UpdateRule = GetForeignKeyRule((string)foreignKeyData[ForeignKeyQueryNames.UpdateRule]);

            return foreignKey;
        }

        private ForeignKeyRule GetForeignKeyRule(string ruleDesc)
        {
            if (ruleDesc.Equals("CASCADE", StringComparison.OrdinalIgnoreCase))
            {
                return ForeignKeyRule.Cascade;
            }
            else if (ruleDesc.Equals("SET_NULL", StringComparison.OrdinalIgnoreCase))
            {
                return ForeignKeyRule.SetNull;
            }
            else if (ruleDesc.Equals("SET_DEFAULT", StringComparison.OrdinalIgnoreCase))
            {
                return ForeignKeyRule.SetDefault;
            }
            else
            {
                return ForeignKeyRule.NoAction;
            }
        }

        #endregion


        #region Helpers

        private void CheckConnection(object connection)
        {
            Check.NotNull(connection, nameof(connection));
            if (!(this as IDatabaseSchemaLoader).SupportsConnectionType(connection))
            {
                throw new ArgumentException(Resources.SqlServerSchemaLoader_UnsupportedConnectionType, nameof(connection));
            }
            SqlConnectionStringBuilder cnBuilder = new SqlConnectionStringBuilder((connection as SqlConnection).ConnectionString);
            Check.NotNullOrWhiteSpace(
                cnBuilder.InitialCatalog, nameof(connection), Resources.SqlServerSchemaLoader_NoInitialCatalog);
        }

        private DataTable GetSchemaTables(SqlConnection connection)
        {
            return GetSchemaTables(connection, null);
        }

        private DataTable GetSchemaTables(SqlConnection connection, string tableName)
        {
            return connection.GetSchema(SchemaNames.Tables, new string[] { null, null, tableName, null });
        }

        private DataTable GetSchemaColumns(SqlConnection connection)
        {
            return GetSchemaColumns(connection, null);
        }

        private DataTable GetSchemaColumns(SqlConnection connection, string tableName)
        {
            DataTable schemaData = connection.GetSchema(SchemaNames.Columns, new string[] { null, null, tableName, null });
            schemaData.DefaultView.Sort =
                $"{ColumnsSchemaNames.TableSchema}, {ColumnsSchemaNames.TableName}, {ColumnsSchemaNames.OrdinalPosition}";
            return schemaData;
        }

        #endregion

    }
}
