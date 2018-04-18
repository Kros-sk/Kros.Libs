using System;

namespace Kros.Data.BulkActions
{
    /// <summary>
    /// Rozhranie pre zdroj dát pre <see cref="IBulkInsert"/>.
    /// </summary>
    public interface IBulkActionDataReader : IDisposable
    {

        /// <summary>
        /// Počet stĺpcov v dátovom riadku.
        /// </summary>
        int FieldCount { get; }

        /// <summary>
        /// Vráti názov stĺpca.
        /// </summary>
        /// <param name="i">Index hľadaného stĺpca.</param>
        /// <returns>Meno stĺpca.</returns>
        /// <exception cref="IndexOutOfRangeException">Zadaný index bol mimo rozsah stĺpcov 0 až <see cref="FieldCount"/>.
        /// </exception>
        string GetName(int i);

        /// <summary>
        /// Vráti index stĺpca s menom <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Meno hľadaného stĺpca.</param>
        /// <returns>Index hľadaného stĺpca.</returns>
        int GetOrdinal(string name);

        /// <summary>
        /// Vráti hodnotu zadaného stĺpca.
        /// </summary>
        /// <param name="i">Index stĺpca, ktorého hodnota sa vracia.</param>
        /// <returns>Objekt - hodnota daného stĺpca.</returns>
        /// <exception cref="IndexOutOfRangeException">Zadaný index bol mimo rozsah stĺpcov 0 až <see cref="FieldCount"/>.
        /// </exception>
        object GetValue(int i);

        /// <summary>
        /// Posunie reader na ďalší záznam.
        /// </summary>
        /// <returns><see langword="true"/>, ak existuje ďalší záznam a reader bol posunutý, <see langword="false"/> ak už ďalší záznam neexistuje.
        /// </returns>
        bool Read();

    }
}
