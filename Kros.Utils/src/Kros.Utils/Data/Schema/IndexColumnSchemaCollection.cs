using Kros.Properties;
using Kros.Utils;
using System;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Zoznam stĺpcov indexu tabuľky.
    /// </summary>
    /// <remarks>Stĺpcom pridaným do zoznamu je automaticky nastavený index <see cref="IndexColumnSchema.Index"/>.
    /// Do zoznamu nie je možné pridať stĺpec, ktorý už patrí inému indexu. V takom prípade je vyvolaná výnimka
    /// <see cref="InvalidOperationException"/>.</remarks>
    public class IndexColumnSchemaCollection
        : System.Collections.ObjectModel.KeyedCollection<string, IndexColumnSchema>
    {

        #region Constructors

        /// <summary>
        /// Vytvorí zoznam stĺpcov pre index <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="index"/> je <c>null</c>.</exception>
        public IndexColumnSchemaCollection(IndexSchema index)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            Check.NotNull(index, nameof(index));

            Index = index;
        }

        #endregion


        #region Common

        /// <summary>
        /// Index, ktorému zoznam patrí.
        /// </summary>
        public IndexSchema Index { get; }

        /// <summary>
        /// Vytvorí nový stĺpec v zozname s menom <paramref name="columnName"/> a zoradením
        /// <see cref="SortOrder.Ascending">SortOrder.Ascending</see>.
        /// </summary>
        /// <param name="columnName">Meno stĺpca.</param>
        /// <returns>Vytvorený stĺpec.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="columnName"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="columnName"/> je prázdny reťazec, alebo reťazec bielych
        /// znakov.</exception>
        public IndexColumnSchema Add(string columnName)
        {
            IndexColumnSchema indexColumn = new IndexColumnSchema(columnName);
            Add(indexColumn);
            return indexColumn;
        }

        /// <summary>
        /// Vytvorí nový stĺpec v zozname s menom <paramref name="columnName"/> a zoradením <paramref name="order"/>.
        /// </summary>
        /// <param name="columnName">Meno stĺpca.</param>
        /// <param name="order">Zoradenie stĺpca.</param>
        /// <returns>Vytvorený stĺpec.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="columnName"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="columnName"/> je prázdny reťazec, alebo reťazec bielych
        /// znakov.</exception>
        public IndexColumnSchema Add(string columnName, SortOrder order)
        {
            IndexColumnSchema indexColumn = new IndexColumnSchema(columnName, order);
            Add(indexColumn);
            return indexColumn;
        }

        #endregion


        #region KeyedCollection

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override string GetKeyForItem(IndexColumnSchema item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, IndexColumnSchema item)
        {
            if (item.Index == null)
            {
                item.Index = Index;
            }
            else if (item.Index != Index)
            {
                throw new InvalidOperationException(string.Format(Resources.Schema_ColumnBelongsToAnotherIndex,
                    item.Name, Index.Name, item.Index.Name));
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (index < Count)
            {
                base[index].Index = null;
            }
            base.RemoveItem(index);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

    }
}
