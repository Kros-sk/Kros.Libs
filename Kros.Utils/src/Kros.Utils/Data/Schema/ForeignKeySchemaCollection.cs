using Kros.Properties;
using Kros.Utils;
using System;
using System.Collections.Generic;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Zoznam cudzích kľúčov tabuľky <see cref="TableSchema"/>.
    /// </summary>
    /// <remarks>Cudzím kľúčom pridaným do zoznamu je automaticky nastavená tabuľka <see cref="IndexSchema.Table"/>.
    /// Do zoznamu nie je možné pridať cudzí kľúč, ktorý už patrí inej tabuľke. V takom prípade je vyvolaná výnimka
    /// <see cref="InvalidOperationException"/>.</remarks>
    public class ForeignKeySchemaCollection
        : System.Collections.ObjectModel.KeyedCollection<string, ForeignKeySchema>
    {

        #region Constructors

        /// <summary>
        /// Vytvorí zoznam cudzích kľúčov pre tabuľku <paramref name="table"/>.
        /// </summary>
        /// <param name="table">Tabuľka.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="table"/> je <c>null</c>.</exception>
        public ForeignKeySchemaCollection(TableSchema table)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            Check.NotNull(table, nameof(table));
            Table = table;
        }

        #endregion


        #region Common

        /// <summary>
        /// Tabuľka, ktorej zoznam cudzích kľúčov patrí.
        /// </summary>
        TableSchema Table { get; }

        /// <summary>
        /// Vytvorí a pridá do zoznamu novú definíciu cudzieho kľúča s menom <paramref name="name"/>. Cudzí kľúč je nad stĺpcom
        /// <paramref name="foreignKeyTableColumn"/> tabuľky <paramref name="foreignKeyTableName"/> a odkazuje sa na
        /// stĺpec <paramref name="primaryKeyTableColumn"/> tabuľky <paramref name="primaryKeyTableName"/>.
        /// </summary>
        /// <param name="name">Meno cudzieho kľúča.</param>
        /// <param name="primaryKeyTableName">Meno tabuľky s primárnym kľúčom.</param>
        /// <param name="primaryKeyTableColumn">Meno odkazovaného stĺpca v tabuľke s primárnym kľúčom.</param>
        /// <param name="foreignKeyTableName">Meno tabuľky s cudzím kľúčom.</param>
        /// <param name="foreignKeyTableColumn">Meno stĺpca v tabuľke cudzieho kľúča.</param>
        /// <returns>Vytvorený cudzí kľúč.</returns>
        /// <exception cref="ArgumentNullException">Hodnota ľubovoľného parametra je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota ľubovoľného parametra je prázdny reťazec, alebo reťazec bielych znakov.
        /// </exception>
        public ForeignKeySchema Add(
            string name,
            string primaryKeyTableName,
            string primaryKeyTableColumn,
            string foreignKeyTableName,
            string foreignKeyTableColumn)
        {
            ForeignKeySchema foreignKey = new ForeignKeySchema(
                name,
                primaryKeyTableName, primaryKeyTableColumn,
                foreignKeyTableName, foreignKeyTableColumn);
            Add(foreignKey);
            return foreignKey;
        }

        /// <summary>
        /// Vytvorí a pridá do zoznamu novú definíciu cudzieho kľúča s menom <paramref name="name"/>. Cudzí kľúč je nad stĺpcami
        /// <paramref name="foreignKeyTableColumns"/> tabuľky <paramref name="foreignKeyTableName"/> a odkazuje sa na
        /// stĺpce <paramref name="primaryKeyTableColumns"/> tabuľky <paramref name="primaryKeyTableName"/>.
        /// </summary>
        /// <param name="name">Meno cudzieho kľúča.</param>
        /// <param name="primaryKeyTableName">Meno tabuľky s primárnym kľúčom.</param>
        /// <param name="primaryKeyTableColumns">Zoznam odkazovaných stĺpcov v tabuľke s primárnym kľúčom.</param>
        /// <param name="foreignKeyTableName">Meno tabuľky s cudzím kľúčom.</param>
        /// <param name="foreignKeyTableColumns">Zoznam stĺpcov v tabuľke cudzieho kľúča.</param>
        /// <returns>Vytvorený cudzí kľúč.</returns>
        /// <exception cref="ArgumentNullException">Hodnota ľubovoľného parametra je <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><list type="bullet">
        /// <item>Hodnota parametrov <paramref name="name"/>, <paramref name="primaryKeyTableName"/> alebo
        /// <paramref name="foreignKeyTableName"/> je prázdny reťazec, alebo reťazec bielych znakov.</item>
        /// <item>Zoznam stĺpcov <paramref name="primaryKeyTableColumns"/> alebo <paramref name="foreignKeyTableColumns"/>
        /// je prázdny.</item>
        /// </list></exception>
        public ForeignKeySchema Add(
            string name,
            string primaryKeyTableName,
            IEnumerable<string> primaryKeyTableColumns,
            string foreignKeyTableName,
            IEnumerable<string> foreignKeyTableColumns)
        {
            ForeignKeySchema foreignKey = new ForeignKeySchema(
                name,
                primaryKeyTableName, primaryKeyTableColumns,
                foreignKeyTableName, foreignKeyTableColumns);
            Add(foreignKey);
            return foreignKey;
        }

        #endregion


        #region KeyedCollection

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override string GetKeyForItem(ForeignKeySchema item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, ForeignKeySchema item)
        {
            if (item.Table == null)
            {
                item.Table = Table;
            }
            else if (item.Table != Table)
            {
                throw new InvalidOperationException(string.Format(Resources.Schema_ForeignKeyBelongsToAnotherTable,
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
