using Kros.Utils;
using System;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Schéma stĺpca v indexe tabuľky.
    /// </summary>
    public class IndexColumnSchema
    {

        #region Constructors

        /// <summary>
        /// Vytvorí inštanciu indexového stĺpca s menom <paramref name="name"/>.
        /// Zoradenie stĺpca je <see cref="SortOrder.Ascending">SortOrder.Ascending</see>.
        /// </summary>
        /// <param name="name">Meno stĺpca v indexe.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="name"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="name"/> je prázdny reťazec, alebo reťazec bielych znakov.
        /// </exception>
        public IndexColumnSchema(string name)
            : this(name, SortOrder.Ascending)
        {
        }

        /// <summary>
        /// Vytvorí inštanciu indexového stĺpca s menom <paramref name="name"/> a zoradením <paramref name="order"/>.
        /// </summary>
        /// <param name="name">Meno stĺpca v indexe.</param>
        /// <param name="order">Zoradenie stĺpca v indexe.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="name"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="name"/> je prázdny reťazec, alebo reťazec bielych znakov.
        /// </exception>
        public IndexColumnSchema(string name, SortOrder order)
        {
            Name = Check.NotNullOrWhiteSpace(name, nameof(name));
            Order = order;
        }

        #endregion


        #region Common

        /// <summary>
        /// Meno stĺpca v indexe.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Zoradenie stĺpca v indexe.
        /// </summary>
        public SortOrder Order { get; set; } = SortOrder.Ascending;

        /// <summary>
        /// Index, ktorému stĺpec patrí.
        /// </summary>
        public IndexSchema Index { get; internal set; }

        #endregion

    }
}
