using Kros.Utils;
using System.Data.Common;

namespace Kros.Data
{
    /// <summary>
    /// Základná trieda, ktorú stači zdediť aby sme mohli jednoduchšie vytvárať implementácie <see cref="IIdGenerator"/>.
    /// Stará sa o dispose connection.
    /// </summary>
    /// <seealso cref="Kros.Data.IIdGenerator" />
    public abstract class IdGeneratorBase
        : IIdGenerator
    {

        private bool _disposeOfConnection = false;

        /// <summary>
        /// Konštruktor.
        /// </summary>
        /// <param name="connection">Connection, ktorá sa použije pre získavanie unikátnych identifikátorov.</param>
        /// <param name="tableName">Názov tabuľky, pre ktorú generujem identifikátory.</param>
        /// <param name="bathSize">Veľkosť dávky, ktorú si zarezervuje dopredu.</param>
        public IdGeneratorBase(DbConnection connection, string tableName, int bathSize)
            : this(tableName, bathSize)
        {
            Connection = Check.NotNull(connection, nameof(connection));
        }

        /// <summary>
        /// Konštruktor.
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, ktorý sa použije na vytvorenie conenction pre získavanie unikátnych identifikátorov.
        /// </param>
        /// <param name="tableName">Názov tabuľky, pre ktorú generujem identifikátory.</param>
        /// <param name="batchSize">Veľkosť dávky, ktorú si zarezervuje dopredu.</param>
        public IdGeneratorBase(string connectionString, string tableName, int batchSize)
            : this(tableName, batchSize)
        {
            Connection = CreateConnection(connectionString);
            _disposeOfConnection = true;
        }

        private IdGeneratorBase(string tableName, int batchSize)
        {
            TableName = Check.NotNull(tableName, nameof(tableName));
            BatchSize = Check.GreaterOrEqualThan(batchSize, 0, nameof(tableName));
        }

        /// <summary>
        /// Vytvorenie connection.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <returns>Špecifická inštancia <see cref="DbConnection"/>.</returns>
        protected abstract DbConnection CreateConnection(string connectionString);

        /// <summary>
        /// Názov tabuľky, pre ktorú generujem identifikátory.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Veľkosť dávky, ktorú si zarezevuje dopredu.
        /// </summary>
        public int BatchSize { get; }

        /// <summary>
        /// Connection, ktorá sa použije pre získavanie unikátnych identifikátorov.
        /// </summary>
        protected DbConnection Connection { get; }

        private int _nextId = 0;
        private int _nextAccessToDb = -1;

        /// <inheritdoc/>
        public virtual int GetNext()
        {
            if (_nextAccessToDb <= _nextId)
            {
                _nextId = GetNewIdFromDb();
                _nextAccessToDb = _nextId + BatchSize;
            }

            return _nextId++;
        }

        private int GetNewIdFromDb()
        {
            using (ConnectionHelper.OpenConnection(Connection))
            {
                return GetNewIdFromDbCore();
            }
        }

        /// <summary>
        /// Získa nový identifikátor z databázy. V tejto metóde je zabezpečné, že spojenie na databázu <see cref="Connection"/>
        /// je otvorené.
        /// </summary>
        /// <returns>Ďalší identifikátor, ktorý sa môže použiť.</returns>
        protected abstract int GetNewIdFromDbCore();

        /// <summary>
        /// Inicializuje databázu tak, aby sa dal v nej používať generátor ID. Znamená to napríklad vytvorenie príslušnej
        /// tabuľky a stored procedúry.
        /// </summary>
        public abstract void InitDatabaseForIdGenerator();

        #region IDisposable Support

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_disposeOfConnection)
                    {
                        Connection.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }


        public void Dispose() => Dispose(true);

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion
    }
}
