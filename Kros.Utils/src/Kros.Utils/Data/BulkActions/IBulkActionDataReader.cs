using System;

namespace Kros.Data.BulkActions
{
    /// <summary>
    /// Interface for data source used in bulk actions (<see cref="IBulkInsert"/>, <see cref="IBulkUpdate"/>).
    /// </summary>
    public interface IBulkActionDataReader : IDisposable
    {

        /// <summary>
        /// Columns count of the data row.
        /// </summary>
        int FieldCount { get; }

        /// <summary>
        /// Returns column name at position <paramref name="i"/>.
        /// </summary>
        /// <param name="i">Index of column.</param>
        /// <returns>Column name.</returns>
        /// <exception cref="IndexOutOfRangeException">Zadaný index bol mimo rozsah stĺpcov 0 až <see cref="FieldCount"/>.
        /// </exception>
        string GetName(int i);

        /// <summary>
        /// Return index of column with name <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <returns>Index of column.</returns>
        int GetOrdinal(string name);

        /// <summary>
        /// Returns value of column at index <paramref name="i"/>.
        /// </summary>
        /// <param name="i">Column index.</param>
        /// <returns>Object - value of column.</returns>
        /// <exception cref="IndexOutOfRangeException">Defined index is not between 0 and <see cref="FieldCount"/>.
        /// </exception>
        object GetValue(int i);

        /// <summary>
        /// Moves reader to next record.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if next record exists and reader is moved,
        /// <see langword="false"/> if next record does not exist.
        /// </returns>
        bool Read();

    }
}
