using Kros.Utils;
using System;
using System.Text;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Schéma databázovej tabuľky.
    /// </summary>
    public class TableSchema
    {

        #region Constructors

        /// <summary>
        /// Vytvorí schému tabuľky <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Meno tabuľky. Je povinné.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="name"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="name"/> je prázdny reťazec, alebo reťazec bielych znakov.
        /// </exception>
        public TableSchema(string name)
            : this(null, name)
        {
        }

        /// <summary>
        /// Vytvorí schému tabuľky <paramref name="name"/> pre databázu <paramref name="database"/>.
        /// </summary>
        /// <param name="database">Databáza, v ktorej je tabuľka. Hodnota nie je povinná, môže byť <c>null</c>.</param>
        /// <param name="name">Meno tabuľky. Je povinné.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="name"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="name"/> je prázdny reťazec, alebo reťazec bielych znakov.
        /// </exception>
        public TableSchema(DatabaseSchema database, string name)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            Database = database;
            Name = name;
            Columns = new ColumnSchemaCollection(this);
            PrimaryKey = new IndexSchema($"PK_{name}", IndexType.PrimaryKey, true);
            Indexes = new IndexSchemaCollection(this);
            ForeignKeys = new ForeignKeySchemaCollection(this);
        }

        #endregion


        #region Common

        /// <summary>
        /// Meno tabuľky.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Databáza, ktorej tabuľka patrí.
        /// </summary>
        public DatabaseSchema Database { get; internal set; }

        /// <summary>
        /// Zoznam stĺpcov tabuľky.
        /// </summary>
        public ColumnSchemaCollection Columns { get; }

        /// <summary>
        /// Primárny kľúč tabuľky.
        /// </summary>
        /// <remarks>Inštancia je vždy vytvorená, aj ak tabuľka nemá primárny kľúč. V tomto prípade len nemá žiadny stĺpec.
        /// </remarks>
        public IndexSchema PrimaryKey { get; }

        /// <summary>
        /// Zoznam indexov tabuľky.
        /// </summary>
        public IndexSchemaCollection Indexes { get; }

        /// <summary>
        /// Zoznam cudzích kľúčov tabuľky.
        /// </summary>
        public ForeignKeySchemaCollection ForeignKeys { get; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(50);
            sb.Append("Table ");
            sb.Append(Name);
            sb.Append(": Primary Key = ");
            if (PrimaryKey.Columns.Count == 0)
            {
                sb.Append("*not set*");
            }
            else
            {
                bool first = true;
                foreach (IndexColumnSchema column in PrimaryKey.Columns)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                    sb.Append(column.Name);
                }
            }

            return sb.ToString();
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

    }
}
