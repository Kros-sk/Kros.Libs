using Kros.Utils;
using Kros.Properties;
using System;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Zoznam indexov tabuľky <see cref="TableSchema"/>.
    /// </summary>
    /// <remarks>Indexom pridaným do zoznamu je automaticky nastavená tabuľka <see cref="IndexSchema.Table"/>.
    /// Do zoznamu nie je možné pridať index, ktorý už patrí inej tabuľke. V takom prípade je vyvolaná výnimka
    /// <see cref="InvalidOperationException"/>.</remarks>
    public class IndexSchemaCollection
        : System.Collections.ObjectModel.KeyedCollection<string, IndexSchema>
    {

        #region Constructors

        /// <summary>
        /// Vytvorí zoznam stĺpcov pre tabuľku <paramref name="table"/>.
        /// </summary>
        /// <param name="table">Tabuľka.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="table"/> je <c>null</c>.</exception>
        public IndexSchemaCollection(TableSchema table)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            Check.NotNull(table, nameof(table));
            Table = table;
        }

        #endregion


        #region Common

        /// <summary>
        /// Tabuľka, ktorej zoznam stĺpcov patrí.
        /// </summary>
        public TableSchema Table { get; }

        /// <summary>
        /// Vytvorí a pridá do zoznamu novú schému indexu s menom <paramref name="indexName"/>.
        /// </summary>
        /// <param name="indexName">Meno indexu.</param>
        /// <returns>Vytvorenú schému indexu.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="indexName"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="indexName"/> je prázdny reťazec, alebo reťazec bielych
        /// znakov.</exception>
        public IndexSchema Add(string indexName)
        {
            IndexSchema index = new IndexSchema(indexName);
            Add(index);
            return index;
        }

        /// <summary>
        /// Vytvorí a pridá do zoznamu novú schému indexu s menom <paramref name="indexName"/>
        /// a typom <paramref name="indexType"/>.
        /// </summary>
        /// <param name="indexName">Meno indexu.</param>
        /// <param name="indexType">Typ indexu.</param>
        /// <returns>Vytvorenú schému indexu.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="indexName"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="indexName"/> je prázdny reťazec, alebo reťazec bielych
        /// znakov.</exception>
        public IndexSchema Add(string indexName, IndexType indexType)
        {
            IndexSchema index = new IndexSchema(indexName, indexType);
            Add(index);
            return index;
        }

        /// <summary>
        /// Vytvorí a pridá do zoznamu novú schému indexu s menom <paramref name="indexName"/>,
        /// typom <paramref name="indexType"/> a príznakom <paramref name="clustered"/>.
        /// </summary>
        /// <param name="indexName">Meno indexu.</param>
        /// <param name="indexType">Typ indexu.</param>
        /// <param name="clustered">Príznak, či index je <c>CLUSTERED</c>.</param>
        /// <returns>Vytvorenú schému indexu.</returns>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="indexName"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="indexName"/> je prázdny reťazec, alebo reťazec bielych
        /// znakov.</exception>
        public IndexSchema Add(string indexName, IndexType indexType, bool clustered)
        {
            IndexSchema index = new IndexSchema(indexName, indexType, clustered);
            Add(index);
            return index;
        }

        #endregion


        #region KeyedCollection

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override string GetKeyForItem(IndexSchema item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, IndexSchema item)
        {
            if (item.Table == null)
            {
                item.Table = Table;
            }
            else if (item.Table != Table)
            {
                throw new InvalidOperationException(string.Format(Resources.Schema_IndexBelongsToAnotherTable,
                    item.Name, Table.Name, item.Table.Name));
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (index < Count)
            {
                base[index].Table = null;
            }
            base.RemoveItem(index);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

    }
}
