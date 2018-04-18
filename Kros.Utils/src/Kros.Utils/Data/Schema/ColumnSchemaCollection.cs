using Kros.Utils;
using Kros.Properties;
using System;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Zoznam stĺpcov tabuľky <see cref="TableSchema"/>.
    /// </summary>
    /// <remarks>Stĺpcom pridaným do zoznamu je automaticky nastavená tabuľka <see cref="ColumnSchema.Table"/>.
    /// Do zoznamu nie je možné pridať stĺpec, ktorý už patrí inej tabuľke. V takom prípade je vyvolaná výnimka
    /// <see cref="InvalidOperationException"/>.</remarks>
    public class ColumnSchemaCollection
        : System.Collections.ObjectModel.KeyedCollection<string, ColumnSchema>
    {

        #region Constructors

        /// <summary>
        /// Vytvorí zoznam stĺpcov pre tabuľku <paramref name="table"/>.
        /// </summary>
        /// <param name="table">Tabuľka.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="table"/> je <c>null</c>.</exception>
        public ColumnSchemaCollection(TableSchema table)
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

        #endregion


        #region KeyedCollection

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override string GetKeyForItem(ColumnSchema item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, ColumnSchema item)
        {
            if (item.Table == null)
            {
                item.Table = Table;
            }
            else if (item.Table != Table)
            {
                throw new InvalidOperationException(string.Format(Resources.Schema_ColumnBelongsToAnotherTable,
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
