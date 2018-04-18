using System;
using System.Data;

namespace Kros.Data.BulkActions
{
    /// <summary>
    /// Rozhranie pre rýchlu hromadnú editáciu dát v databáze.
    /// </summary>
    public interface IBulkUpdate : IDisposable
    {
        /// <summary>
        /// Meno cieľovej tabuľky v databáze.
        /// </summary>
        string DestinationTableName { get; set; }

        /// <summary>
        /// Akcia, ktorá sa má vykonať nad tempovou tabuľkou.
        /// </summary>
        /// <remarks>
        /// Akcia, ktorá sa zavolá nad tempovou tabuľkou (chcem ešte dodatočne upraviť dáta).
        /// <list type="bullet">
        /// <item>
        /// <c>IDbConnection</c> - connection nad tempovou tabuľkou
        /// </item>
        /// <item>
        /// <c>IDbTransaction</c> - transakcia nad tempovou tabuľkou,
        /// </item>
        /// <item>
        /// <c>string</c> - názov tempovej tabuľky.
        /// </item>
        /// </list>
        /// </remarks>
        Action<IDbConnection, IDbTransaction, string> TempTableAction { get; set; }

        /// <summary>
        /// Primárny kľúč.
        /// </summary>
        string PrimaryKeyColumn { get; set; }

        /// <summary>
        /// Zedituje všetky dáta zo zdroja <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">Zdroj dát.</param>
        void Update(IBulkActionDataReader reader);

        /// <summary>
        /// Zedituje všetky dáta zo zdroja <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">Zdroj dát.</param>
        void Update(IDataReader reader);

        /// <summary>
        /// Zedituje všetky riadky z tabuľky <paramref name="table"/>.
        /// </summary>
        /// <param name="table">Zdrojové dáta.</param>
        void Update(DataTable table);
    }
}
