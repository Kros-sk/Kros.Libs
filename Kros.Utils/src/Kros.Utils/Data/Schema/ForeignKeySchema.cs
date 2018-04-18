using Kros.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Schéma cudzieho kľúča tabuľky.
    /// </summary>
    public class ForeignKeySchema
    {

        #region Fields

        private readonly List<string> _primaryKeyTableColumns = new List<string>();
        private readonly List<string> _foreignKeyTableColumns = new List<string>();

        #endregion


        #region Constructors

        /// <summary>
        /// Vytvorí novú definíciu cudzieho kľúča s menom <paramref name="name"/>. Cudzí kľúč je nad stĺpcom
        /// <paramref name="foreignKeyTableColumn"/> tabuľky <paramref name="foreignKeyTableName"/> a odkazuje sa na
        /// stĺpec <paramref name="primaryKeyTableColumn"/> tabuľky <paramref name="primaryKeyTableName"/>.
        /// </summary>
        /// <param name="name">Meno cudzieho kľúča.</param>
        /// <param name="primaryKeyTableName">Meno tabuľky s primárnym kľúčom.</param>
        /// <param name="primaryKeyTableColumn">Meno odkazovaného stĺpca v tabuľke s primárnym kľúčom.</param>
        /// <param name="foreignKeyTableName">Meno tabuľky s cudzím kľúčom.</param>
        /// <param name="foreignKeyTableColumn">Meno stĺpca v tabuľke cudzieho kľúča.</param>
        /// <exception cref="ArgumentNullException">Hodnota ľubovoľného parametra je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota ľubovoľného parametra je prázdny reťazec, alebo reťazec bielych znakov.
        /// </exception>
        public ForeignKeySchema(
            string name,
            string primaryKeyTableName,
            string primaryKeyTableColumn,
            string foreignKeyTableName,
            string foreignKeyTableColumn)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            Check.NotNullOrWhiteSpace(primaryKeyTableName, nameof(primaryKeyTableName));
            Check.NotNullOrWhiteSpace(primaryKeyTableColumn, nameof(primaryKeyTableColumn));
            Check.NotNullOrWhiteSpace(foreignKeyTableName, nameof(foreignKeyTableName));
            Check.NotNullOrWhiteSpace(foreignKeyTableColumn, nameof(foreignKeyTableColumn));

            Name = name;
            PrimaryKeyTableName = primaryKeyTableName;
            ForeignKeyTableName = foreignKeyTableName;
            PrimaryKeyTableColumns = new ReadOnlyCollection<string>(_primaryKeyTableColumns);
            ForeignKeyTableColumns = new ReadOnlyCollection<string>(_foreignKeyTableColumns);
            _primaryKeyTableColumns.Add(primaryKeyTableColumn);
            _foreignKeyTableColumns.Add(foreignKeyTableColumn);
        }

        /// <summary>
        /// Vytvorí novú definíciu cudzieho kľúča s menom <paramref name="name"/>. Cudzí kľúč je nad stĺpcami
        /// <paramref name="foreignKeyTableColumns"/> tabuľky <paramref name="foreignKeyTableName"/> a odkazuje sa na
        /// stĺpce <paramref name="primaryKeyTableColumns"/> tabuľky <paramref name="primaryKeyTableName"/>.
        /// </summary>
        /// <param name="name">Meno cudzieho kľúča.</param>
        /// <param name="primaryKeyTableName">Meno tabuľky s primárnym kľúčom.</param>
        /// <param name="primaryKeyTableColumns">Zoznam odkazovaných stĺpcov v tabuľke s primárnym kľúčom.</param>
        /// <param name="foreignKeyTableName">Meno tabuľky s cudzím kľúčom.</param>
        /// <param name="foreignKeyTableColumns">Zoznam stĺpcov v tabuľke cudzieho kľúča.</param>
        /// <exception cref="ArgumentNullException">Hodnota ľubovoľného parametra je <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><list type="bullet">
        /// <item>Hodnota parametrov <paramref name="name"/>, <paramref name="primaryKeyTableName"/> alebo
        /// <paramref name="foreignKeyTableName"/> je prázdny reťazec, alebo reťazec bielych znakov.</item>
        /// <item>Zoznam stĺpcov <paramref name="primaryKeyTableColumns"/> alebo <paramref name="foreignKeyTableColumns"/>
        /// je prázdny.</item>
        /// </list></exception>
        public ForeignKeySchema(
            string name,
            string primaryKeyTableName,
            IEnumerable<string> primaryKeyTableColumns,
            string foreignKeyTableName,
            IEnumerable<string> foreignKeyTableColumns)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            Check.NotNullOrWhiteSpace(primaryKeyTableName, nameof(primaryKeyTableName));
            Check.NotNull(primaryKeyTableColumns, nameof(primaryKeyTableColumns));
            Check.NotNullOrWhiteSpace(foreignKeyTableName, nameof(foreignKeyTableName));
            Check.NotNull(foreignKeyTableColumns, nameof(foreignKeyTableColumns));
            Check.GreaterThan(primaryKeyTableColumns.Count(), 0, nameof(primaryKeyTableColumns));
            Check.GreaterThan(foreignKeyTableColumns.Count(), 0, nameof(foreignKeyTableColumns));

            Name = name;
            PrimaryKeyTableName = primaryKeyTableName;
            ForeignKeyTableName = foreignKeyTableName;
            PrimaryKeyTableColumns = new ReadOnlyCollection<string>(_primaryKeyTableColumns);
            ForeignKeyTableColumns = new ReadOnlyCollection<string>(_foreignKeyTableColumns);
            _primaryKeyTableColumns.AddRange(primaryKeyTableColumns);
            _foreignKeyTableColumns.AddRange(foreignKeyTableColumns);
        }

        #endregion


        #region Common

        /// <summary>
        /// Meno cudzieho kľúča.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Meno tabuľky z ktorej je primárny kľúč.
        /// </summary>
        public string PrimaryKeyTableName { get; }

        /// <summary>
        /// Zoznam stĺpcov tabuľky z ktorej je primárny kľúč.
        /// </summary>
        public ReadOnlyCollection<string> PrimaryKeyTableColumns { get; }

        /// <summary>
        /// Meno tabuľky, v ktorej je definovaný cudzí kľúč.
        /// </summary>
        public string ForeignKeyTableName { get; }

        /// <summary>
        /// Zoznam stĺpcov cudzieho kľúča v tabuľke, kde je definovaný.
        /// </summary>
        public ReadOnlyCollection<string> ForeignKeyTableColumns { get; }

        /// <summary>
        /// Pravidlo ako sa správať, ak je v hlavnej tabuľke vymazaný príslušný záznam.
        /// </summary>
        public ForeignKeyRule DeleteRule { get; set; } = ForeignKeyRule.NoAction;

        /// <summary>
        /// Pravidlo ako sa správať, ak je v hlavnej tabuľke aktualizovaný príslušný záznam.
        /// </summary>
        public ForeignKeyRule UpdateRule { get; set; } = ForeignKeyRule.NoAction;

        /// <summary>
        /// Tabuľka, ktorej cudzí kľúč patrí.
        /// </summary>
        public TableSchema Table { get; internal set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(50);

            sb.AppendFormat("Foreign Key {0}: ", Name);
            ToStringAddTable(sb, PrimaryKeyTableName, PrimaryKeyTableColumns);
            sb.Append(", ");
            ToStringAddTable(sb, ForeignKeyTableName, ForeignKeyTableColumns);
            sb.AppendFormat(", On Delete = {0}, On Update = {1}", DeleteRule.ToString(), UpdateRule.ToString());

            return sb.ToString();
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        private void ToStringAddTable(StringBuilder sb, string tableName, IEnumerable<string> columns)
        {
            sb.Append(tableName);
            sb.Append(" (");
            bool first = true;
            foreach (string column in columns)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }
                sb.Append(column);
            }
            sb.Append(")");
        }

        #endregion

    }
}
