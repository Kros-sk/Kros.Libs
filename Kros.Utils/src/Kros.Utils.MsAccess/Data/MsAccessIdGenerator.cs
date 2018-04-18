using Kros.Data.Schema.MsAccess;
using Kros.MsAccess.Properties;
using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Reflection;

namespace Kros.Data.MsAccess
{
    /// <summary>
    /// Generátor unikátnych identifikátorov pre Ms Access.
    /// </summary>
    /// <seealso cref="IdGeneratorFactories" />
    /// <seealso cref="MsAccessIdGeneratorFactory" />
    /// <remarks>Štandardne sa nevytvára priamo, ale cez <see cref="MsAccessIdGeneratorFactory"/>.</remarks>
    /// <example>
    /// <code language="cs" source="..\Examples\Kros.Utils\IdGeneratorExamples.cs" region="IdGeneratorFactory"/>
    /// </example>
    public class MsAccessIdGenerator
        : IdGeneratorBase
    {
        /// <summary>
        /// Konštruktor.
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, ktorý sa použije na vytvorenie conenction pre získavanie unikátnych identifikátorov.
        /// </param>
        /// <param name="tableName">Názov tabuľky, pre ktorú generujem identifikátory.</param>
        /// <param name="batchSize">Veľkosť dávky, ktorú si zarezervuje dopredu.</param>
        public MsAccessIdGenerator(string connectionString, string tableName, int batchSize)
            : base(connectionString, tableName, batchSize)
        {
        }

        /// <summary>
        /// Konštruktor.
        /// </summary>
        /// <param name="connection">Connection, ktorá sa použije pre získavanie unikátnych identifikátorov.</param>
        /// <param name="tableName">Názov tabuľky, pre ktorú generujem identifikátory.</param>
        /// <param name="batchSize">Veľkosť dávky, ktorú si zarezervuje dopredu.</param>
        public MsAccessIdGenerator(OleDbConnection connection, string tableName, int batchSize)
            : base(connection, tableName, batchSize)
        {
        }

        /// <inheritdoc/>
        protected override DbConnection CreateConnection(string connectionString) =>
            new OleDbConnection(connectionString);

        /// <inheritdoc/>
        protected override int GetNewIdFromDbCore()
        {
            int result = 0;

            var valueIsOk = GetNewIDMsAccessCore(Connection as OleDbConnection, TableName, BatchSize, ref result);
            while (!valueIsOk)
            {
                valueIsOk = GetNewIDMsAccessCore(Connection as OleDbConnection, TableName, BatchSize, ref result);
            }

            return result;
        }

        /// <summary>
        /// Získanie scriptu na vytvorenie IdStore tabuľky.
        /// </summary>
        public static string GetIdStoreTableCreationScript() =>
            Resources.SqlIdGeneratorTableScript;

        /// <inheritdoc/>
        public override void InitDatabaseForIdGenerator()
        {
            using (ConnectionHelper.OpenConnection(Connection))
            {
                var schemaLoader = new MsAccessSchemaLoader();
                if (schemaLoader.LoadTableSchema((OleDbConnection)Connection, "IdStore") == null)
                {
                    using (var cmd = Connection.CreateCommand())
                    {
                        string[] sqlCommands = GetIdStoreTableCreationScript()
                            .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string sqlCommand in sqlCommands)
                        {
                            string query = sqlCommand.Trim();
                            if (!string.IsNullOrEmpty(query))
                            {
                                cmd.CommandText = query;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        #region Private helpers
        // Všetko prevzaté z C4 - tam to prebrali z xLib-u

        private bool GetNewIDMsAccessCore(
            OleDbConnection cn,
            string tableName,
            int numberOfItems,
            ref int result)
        {
            DbTransaction existTransaction = GetExistingTransactionFromMsAccessConnection(cn);

            if (existTransaction == null)
            {
                return GetNewIDMsAccessWithNewTransaction(cn, tableName, numberOfItems, ref result);
            }
            else
            {
                return GetNewIDMsAccessWithExistTransaction(cn, tableName, numberOfItems, ref result, existTransaction);
            }
        }

        private DbTransaction GetExistingTransactionFromMsAccessConnection(OleDbConnection cn)
        {
            PropertyInfo pinfo = typeof(OleDbConnection)
                .GetProperty("InnerConnection", BindingFlags.Instance | BindingFlags.NonPublic);
            var innerConnection = pinfo.GetValue(cn, null);
            pinfo = innerConnection.GetType()
                .GetProperty("LocalTransaction", BindingFlags.Instance | BindingFlags.NonPublic);
            return pinfo == null ? null : (DbTransaction)pinfo.GetValue(innerConnection, null);
        }

        private bool GetNewIDMsAccessWithExistTransaction(
            OleDbConnection cn,
            string tableName,
            int numberOfItems,
            ref int result,
            DbTransaction transaction)
        {
            var valueIsOk = false;
            var actualValue = GetLastID(cn, transaction, tableName);
            var expectedValueInDB = (actualValue + numberOfItems); // Túto hodnotu očakávame po našom aktualizovaní počítadla.

            SaveChanges(cn, transaction, tableName, actualValue, numberOfItems);

            // Porovnáme, či aktuálna hodnota v databáze je taká, ako očakávame.
            // Ak nie je, niekto stihol počítadlo zdvihnúť a tak musíme začať odznova.
            var actualValueInDB = GetLastID(cn, transaction, tableName);
            if (actualValueInDB == expectedValueInDB)
            {
                result = (actualValue + 1);
                valueIsOk = true;
            }

            return valueIsOk;
        }

        private bool GetNewIDMsAccessWithNewTransaction(
            OleDbConnection cn,
            string tableName,
            int numberOfItems,
            ref int result)
        {
            var valueIsOk = false;

            using (var transaction = (DbTransaction)cn.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    valueIsOk = GetNewIDMsAccessWithExistTransaction(cn, tableName, numberOfItems, ref result, transaction);

                    if (valueIsOk)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        // Nastane to, ak niekto stihol hodnotu zdvihnúť.
                        // MS Access chvíľu v transakcii čaká - ak to iná transakcia
                        // stihne dostatočne rýchlo, táto nespadne, iba zdvihne už zdvihnutú hodnotu.
                        transaction.Rollback();
                    }
                }
                catch (OleDbException ex) when (ex.MsAccessErrorCode() == MsAccessErrorCode.CouldNotUpdateCurrentlyLocked)
                {
                    // Tu sa dostaneme, ak iná transakcia aktualizovala počítadlo, ale nie dostatočne rýchlo.
                    // Access spadne na tom, že dáta sú zamknuté.
                    transaction.Rollback();
                }
            }
            return valueIsOk;
        }

        private void SaveChanges(
            IDbConnection cn,
            DbTransaction transaction,
            string tableName,
            int actualValue,
            int numberOfItems)
        {
            if (actualValue == 0)
            {
                DeleteRecord(cn, transaction, tableName);
                InsertRecord(cn, transaction, tableName, numberOfItems);
            }
            else
            {
                IncrementLastId(cn, transaction, tableName, numberOfItems);
            }
        }

        private void DeleteRecord(
            IDbConnection cn,
            DbTransaction transaction,
            string tableName)
        {
            string query = "DELETE FROM IdStore WHERE TableName = @TableName";
            ExecuteNonQuery(cn, transaction, query, tableName);
        }

        private void InsertRecord(
            IDbConnection cn,
            DbTransaction transaction,
            string tableName,
            int lastID)
        {
            string query = "INSERT INTO IdStore (LastId, TableName) VALUES (@LastId, @TableName)";
            ExecuteNonQuery(cn, transaction, query, tableName, lastID);
        }

        private void IncrementLastId(
            IDbConnection cn,
            DbTransaction transaction,
            string tableName,
            int increment)
        {
            string query = "UPDATE IdStore SET LastID = LastID + @LastId WHERE (TableName = @TableName)";
            ExecuteNonQuery(cn, transaction, query, tableName, increment);
        }

        private void ExecuteNonQuery(
            IDbConnection cn,
            DbTransaction transaction,
            string query,
            string tableName,
            int numberOfItems)
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = query;
                AddParameterWithValue(cmd, "@LastId", numberOfItems, DbType.Int32);
                AddParameterWithValue(cmd, "@TableName", tableName, DbType.String);
                cmd.ExecuteNonQuery();
            }
        }

        private void ExecuteNonQuery(
            IDbConnection cn,
            DbTransaction transaction,
            string query,
            string tableName)
        {
            using (var cmd = cn.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = query;
                AddParameterWithValue(cmd, "@TableName", tableName, DbType.String);
                cmd.ExecuteNonQuery();
            }
        }

        private void AddParameterWithValue(IDbCommand cmd, string parameterName, object value, DbType dbType)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = parameterName;
            param.DbType = dbType;
            param.Value = value;
            cmd.Parameters.Add(param);
        }

        private int GetLastID(IDbConnection cn, DbTransaction transaction, string tableName)
        {
            string queryAccess = "SELECT LastID FROM IdStore WHERE (TableName = @TableName)";

            using (var cmd = cn.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = queryAccess;
                AddParameterWithValue(cmd, "@TableName", tableName, DbType.String);
                var actualValue = cmd.ExecuteScalar();
                return Convert.ToInt32(actualValue);
            }
        }

        #endregion
    }
}