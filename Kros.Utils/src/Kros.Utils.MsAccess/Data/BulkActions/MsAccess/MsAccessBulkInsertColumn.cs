using Kros.Utils;
using System;

namespace Kros.Data.BulkActions.MsAccess
{

    /// <summary>
    /// Definícia stĺpca pre hromadné vkladanie do MS Access tabuľky z CSV súboru
    /// (<see cref="MsAccessBulkInsert">MsAccessBulkInsert</see>).
    /// </summary>
    /// <remarks></remarks>
    public class MsAccessBulkInsertColumn
    {

        #region Constructors

        /// <summary>
        /// Vytvorí novú definíciu pre stĺpec <paramref name="columnName">columnName</paramref>, s typom nastaveným
        /// na <see cref="BulkInsertColumnType">BulkInsertColumnType.Undefined</see>.
        /// </summary>
        /// <param name="columnName">Názov stĺpca.</param>
        /// <remarks></remarks>
        public MsAccessBulkInsertColumn(string columnName)
            : this(columnName, BulkInsertColumnType.Undefined)
        {
        }

        /// <summary>
        /// Vytvorí novú definíciu pre stĺpec <paramref name="columnName">columnName</paramref>, s typom
        /// <paramref name="columnType">columnType</paramref>.
        /// </summary>
        /// <param name="columnName">Názov stĺpca.</param>
        /// <param name="columnType">Typ stĺpca.</param>
        /// <exception cref="ArgumentNullException">Vyvolaná, ak názov stĺpca je <see langword="null"/>, alebo zložený
        /// len z bielych znakov.</exception>
        /// <remarks></remarks>
        public MsAccessBulkInsertColumn(string columnName, BulkInsertColumnType columnType)
        {
            Check.NotNullOrWhiteSpace(columnName, nameof(columnName));
            ColumnName = columnName;
            ColumnType = columnType;
        }

        #endregion


        #region Common

        /// <summary>
        /// Typ stĺpca.
        /// </summary>
        /// <value>Hodnota <see cref="BulkInsertColumnType">BulkInsertColumnType</see>.</value>
        /// <remarks></remarks>
        public BulkInsertColumnType ColumnType { get; }

        /// <summary>
        /// Názov stĺpca.
        /// </summary>
        /// <value>Reťazec.</value>
        /// <remarks></remarks>
        public string ColumnName { get; }

        #endregion

    }
}
