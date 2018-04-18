using System;
using System.Data;
using System.Text;

namespace Kros.Data.BulkActions
{
    /// <summary>
    /// Spoločný predok pre bulk update.
    /// </summary>
    public abstract class BulkUpdateBase : IBulkUpdate
    {

        #region Constants

        /// <summary>
        /// Prefix pre tempovú tabuľku.
        /// </summary>
        protected const char PrefixTempTable = '#';

        #endregion


        #region Private fields

        /// <summary>
        /// Connection.
        /// </summary>
        protected IDbConnection _connection;

        /// <summary>
        /// Či je potrebné disposnúť connection.
        /// </summary>
        protected bool _disposeOfConnection = false;

        #endregion


        #region Properties

        /// <summary>
        /// Externá transakcia, v ktorej sa vykoná editácia dát.
        /// </summary>
        public IDbTransaction ExternalTransaction { get; protected set; }

        #endregion


        #region IBulkUpdate Members

        /// <summary>
        /// Meno cieľovej tabuľky v databáze.
        /// </summary>
        public string DestinationTableName { get; set; }

        /// <inheritdoc/>
        public Action<IDbConnection, IDbTransaction, string> TempTableAction { get; set; }

        /// <summary>
        /// Primárny kľúč.
        /// </summary>
        public string PrimaryKeyColumn { get; set; }

        /// <summary>
        /// Zedituje všetky dáta zo zdroja <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">Zdroj dát.</param>
        public void Update(IBulkActionDataReader reader)
        {
            using (var bulkUpdateReader = new BulkActionDataReader(reader))
            {
                Update(bulkUpdateReader);
            }
        }

        /// <summary>
        /// Zedituje všetky dáta zo zdroja <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">Zdroj dát.</param>
        public void Update(IDataReader reader)
        {
            var tempTableName = CreateTempTable(reader);

            using (ConnectionHelper.OpenConnection(_connection))
            {
                InsertIntoTempTable(reader, tempTableName);
                InvokeAction(tempTableName);
                UpdateDestinationTable(reader, tempTableName);
                DoneTempTable(tempTableName);
            }
        }

        /// <summary>
        /// Zedituje všetky riadky z tabuľky <paramref name="table"/>.
        /// </summary>
        /// <param name="table">Zdrojové dáta.</param>
        public void Update(DataTable table)
        {
            using (var reader = table.CreateDataReader())
            {
                Update(reader);
            }
        }

        #endregion


        #region Protected Virtual Methods

        /// <summary>
        /// Vytvorí bulk insert.
        /// </summary>
        /// <returns>Bulk insert.</returns>
        protected abstract IBulkInsert CreateBulkInsert();

        /// <summary>
        /// Zavolá akciu nad tempovou databázou.
        /// </summary>
        /// <param name="tempTableName"></param>
        protected abstract void InvokeAction(string tempTableName);

        /// <summary>
        /// Vráti názov tempovej tabuľky.
        /// </summary>
        protected abstract string GetTempTableName();

        /// <summary>
        /// Vytvorí tempovú tabuľku podľa <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">Reader pre sprístupnenie dát.</param>
        /// <param name="tempTableName">Názov tempovej tabuľky.</param>
        protected abstract void CreateTempTable(IDataReader reader, string tempTableName);

        /// <summary>
        /// Vráti názov tempovej tabuľky pre bulk insert.
        /// </summary>
        /// <param name="name">Názov tempovej tabuľky.</param>
        protected virtual string GetTempTableNameForBulkInsert(string name) => $"[{name}]";

        /// <summary>
        /// Vráti command pre vytvorenie primárneho kľúča.
        /// </summary>
        protected abstract IDbCommand CreateCommandForPrimaryKey();

        /// <summary>
        /// Vykoná update nad cieľovou tabuľkou.
        /// </summary>
        /// <param name="reader">Reader pre sprístupnenie dát.</param>
        /// <param name="tempTableName">Názov tempovej tabuľky.</param>
        protected abstract void UpdateDestinationTable(IDataReader reader, string tempTableName);

        /// <summary>
        /// Ukončí prácu s tempovou tabuľkou.
        /// </summary>
        /// <param name="tempTableName">Názov tempovej tabuľky.</param>
        protected virtual void DoneTempTable(string tempTableName) { }

        #endregion


        #region Helpers

        private string CreateTempTable(IDataReader reader)
        {
            var tempTableName = GetTempTableName();

            CreateTempTable(reader, tempTableName);
            CreatePrimaryKey(tempTableName);

            return tempTableName;
        }



        private void CreatePrimaryKey(string tempTableName)
        {
            using (var cmd = CreateCommandForPrimaryKey())
            {
                cmd.CommandText = $"ALTER TABLE [{tempTableName}] " +
                                  $"ADD CONSTRAINT [PK_{tempTableName.Trim(PrefixTempTable)}] " +
                                  $"PRIMARY KEY NONCLUSTERED ({PrimaryKeyColumn})";
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Zoznam stĺpcov tempovej tabuľky.
        /// </summary>
        /// <param name="reader">Reader pre prístup k dátam.</param>
        protected string GetColumnNamesForTempTable(IDataReader reader)
        {
            var ret = new StringBuilder();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                ret.AppendFormat("[{0}], ", reader.GetName(i));
            }

            ret.Length -= 2;

            return ret.ToString();
        }

        private void InsertIntoTempTable(IDataReader reader, string tempTableName)
        {
            using (var bulkInsert = CreateBulkInsert())
            {
                bulkInsert.DestinationTableName = GetTempTableNameForBulkInsert(tempTableName);
                bulkInsert.Insert(reader);
            }
        }

        /// <summary>
        /// Zoznam stĺpcov tempovej tabuľky.
        /// </summary>
        /// <param name="reader">Reader pre prístup k dátam.</param>
        /// <param name="tempTableName">Názov tempovej tabuľky.</param>
        /// <returns></returns>
        protected string GetUpdateColumnNames(IDataReader reader, string tempTableName)
        {
            var ret = new StringBuilder();
            var columnName = string.Empty;

            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnName = reader.GetName(i);

                if (!PrimaryKeyColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    ret.AppendFormat("[{0}].[{1}] = [{2}].[{1}], ", DestinationTableName, columnName, tempTableName);
                }
            }

            ret.Length -= 2;

            return ret.ToString();
        }

        #endregion


        #region IDisposable

        private bool _disposedValue = false;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing && _disposeOfConnection)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

    }
}
