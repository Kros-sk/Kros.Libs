using System;
using System.Data;

namespace Kros.Data.BulkActions
{
    /// <summary>
    /// Rozhranie pre rýchle hromadné vkladanie dát do databázy.
    /// </summary>
    public interface IBulkInsert : IDisposable
    {
        /// <summary>
        /// Počet riadkov v dávke, ktorá sa posiela do databázy. Ak je hodnota 0, veľkosť dávky nie je obmedzená.
        /// </summary>
        int BatchSize { get; set; }

        /// <summary>
        /// Počet sekúnd na dokončenie operácie. ak je hodnota 0, trvanie operácie nie je obmedzené.
        /// </summary>
        int BulkInsertTimeout { get; set; }

        /// <summary>
        /// Meno cieľovej tabuľky v databáze.
        /// </summary>
        string DestinationTableName { get; set; }

        /// <summary>
        /// Vloží všetky dáta zo zdroja <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">Zdroj dát.</param>
        void Insert(IBulkActionDataReader reader);

        /// <summary>
        /// Vloží všetky dáta zo zdroja <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">Zdroj dát.</param>
        void Insert(IDataReader reader);

        /// <summary>
        /// Vloží všetky riadky z tabuľky <paramref name="table"/>.
        /// </summary>
        /// <param name="table">Zdrojové dáta.</param>
        void Insert(DataTable table);
    }
}
