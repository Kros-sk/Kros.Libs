using Kros.Data.Schema.MsAccess;
using System.Collections.Generic;
using System.Data.OleDb;

namespace Kros.Data.BulkActions.MsAccess
{
    /// <summary>
    /// Zoznam stĺpcov pre hromadné vkladanie do MS Access tabuľky z CSV súboru
    /// (<see cref="MsAccessBulkInsert">MsAccessBulkInsert</see>).
    /// </summary>
    /// <remarks></remarks>
    public class MsAccessBulkInsertColumnCollection : List<MsAccessBulkInsertColumn>
    {

        #region Constructors

        /// <summary>
        /// Inicializuje novú inštanciu triedy <see cref="MsAccessBulkInsertColumnCollection"/>.
        /// </summary>
        public MsAccessBulkInsertColumnCollection() : base()
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu triedy <see cref="MsAccessBulkInsertColumnCollection"/> so zadanou počiatočnou kapacitou.
        /// </summary>
        /// <param name="capacity">Počet položiek, ktoré môže zoznam zo začiatku obsahovať.</param>
        public MsAccessBulkInsertColumnCollection(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu triedy <see cref="MsAccessBulkInsertColumnCollection"/> so zadaných dát.
        /// </summary>
        /// <param name="collection">Dáta, ktorými je zoznam naplnený.</param>
        public MsAccessBulkInsertColumnCollection(IEnumerable<MsAccessBulkInsertColumn> collection)
            : base(collection)
        {
        }

        #endregion


        #region Common

        /// <summary>
        /// Pridá stĺpec s názvom <paramref name="columnName">columnName</paramref>. Typ stĺpca nie je definovaný
        /// <see cref="BulkInsertColumnType">BulkInsertColumnType.Undefined</see>.
        /// </summary>
        /// <param name="columnName">Názov stĺpca.</param>
        /// <remarks></remarks>
        public void Add(string columnName)
        {
            Add(new MsAccessBulkInsertColumn(columnName));
        }

        /// <summary>
        /// Pridá stĺpec s názvom <paramref name="columnName">columnName</paramref>
        /// a typom <paramref name="columnType">columnType</paramref>.
        /// </summary>
        /// <param name="columnName">Názov stĺpca.</param>
        /// <param name="columnType">Typ stĺpca.</param>
        /// <remarks></remarks>
        public void Add(string columnName, BulkInsertColumnType columnType)
        {
            Add(new MsAccessBulkInsertColumn(columnName, columnType));
        }

        /// <summary>
        /// Pridá stĺpec podľa definície stĺpca z databázy. Typ stĺpca je určený automaticky podľa dátového typu
        /// v databáze.
        /// </summary>
        /// <param name="column">Definícia databázového stĺpca.</param>
        /// <remarks></remarks>
        public void Add(MsAccessColumnSchema column)
        {
            Add(new MsAccessBulkInsertColumn(column.Name, GetColumnType(column)));
        }

        /// <summary>
        /// Pridá viacero stĺpcov naraz v zozname <paramref name="columnNames">columnNames</paramref>. Všetky stĺpce
        /// nemajú definovaný typ (<see cref="BulkInsertColumnType">BulkInsertColumnType.Undefined</see>).
        /// </summary>
        /// <param name="columnNames">Zoznam názvov stĺpcov.</param>
        /// <remarks></remarks>
        public void AddRange(params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                Add(columnName);
            }
        }

        /// <summary>
        /// Pridá viacero stĺpcov naraz určených podľa definícií z databázy <paramref name="columns">columns</paramref>.
        /// Typy stĺpcov sú určené automaticky podľa dátového typu v databáze.
        /// </summary>
        /// <param name="columns">Zoznam databázových definícií stĺpcov.</param>
        /// <remarks></remarks>
        public void AddRange(IEnumerable<MsAccessColumnSchema> columns)
        {
            foreach (var column in columns)
            {
                Add(column);
            }
        }

        #endregion


        #region Pomocné

        // Na základe dátového typu v databáze vráti typ stĺpca pre hromadné vkladanie.
        private static BulkInsertColumnType GetColumnType(MsAccessColumnSchema column)
        {
            if ((column.OleDbType == OleDbType.VarChar) ||
                (column.OleDbType == OleDbType.VarWChar) ||
                (column.OleDbType == OleDbType.LongVarChar) ||
                (column.OleDbType == OleDbType.LongVarWChar) ||
                (column.OleDbType == OleDbType.Char) ||
                (column.OleDbType == OleDbType.WChar))
            {
                return BulkInsertColumnType.Text;
            }
            return BulkInsertColumnType.Undefined;
        }

        #endregion

    }
}
