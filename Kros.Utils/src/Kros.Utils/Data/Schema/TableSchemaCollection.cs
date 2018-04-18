using Kros.Utils;
using Kros.Properties;
using System;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Zoznam tabuliek databázy.
    /// </summary>
    /// <remarks>Tabuľkám pridaným do zoznamu je automaticky nastavená databáza <see cref="TableSchema.Database"/>.
    /// Do zoznamu nie je možné pridať tabuľku, ktorá už patrí inej databáze. V takom prípade je vyvolaná výnimka
    /// <see cref="InvalidOperationException"/>.</remarks>
    public class TableSchemaCollection
        : System.Collections.ObjectModel.KeyedCollection<string, TableSchema>
    {

        #region Constructors

        /// <summary>
        /// Vytvorí zoznam tabuliek pre databázu <paramref name="database"/>.
        /// </summary>
        /// <param name="database">Databáza.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="database"/> je <c>null</c>.</exception>
        public TableSchemaCollection(DatabaseSchema database)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            Check.NotNull(database, nameof(database));

            Database = database;
        }

        #endregion


        #region Common

        /// <summary>
        /// Databáza, ktorej zoznam tabuliek patrí.
        /// </summary>
        public DatabaseSchema Database { get; }

        /// <summary>
        /// Vytvorí schému tabuľky s menom <paramref name="name"/> a pridá ju do zoznamu.
        /// </summary>
        /// <param name="name">Meno novej tabuľky.</param>
        /// <returns>Vytvorená schéma tabuľky.</returns>
        public TableSchema Add(string name)
        {
            TableSchema schema = new TableSchema(Database, name);
            Add(schema);
            return schema;
        }

        #endregion


        #region KeyedCollection

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override string GetKeyForItem(TableSchema item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, TableSchema item)
        {
            if (item.Database == null)
            {
                item.Database = Database;
            }
            else if (item.Database != Database)
            {
                throw new InvalidOperationException(string.Format(Resources.Schema_TableBelongsToAnotherDatabase,
                    item.Name, Database.Name, item.Database.Name));
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (index < Count)
            {
                base[index].Database = null;
            }
            base.RemoveItem(index);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

    }
}
