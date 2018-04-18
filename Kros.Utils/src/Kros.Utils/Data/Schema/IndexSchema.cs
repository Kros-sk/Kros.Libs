using Kros.Properties;
using Kros.Utils;
using System;
using System.Text;

namespace Kros.Data.Schema
{
    /// <summary>
    /// Schéma indexu tabuľky.
    /// </summary>
    public class IndexSchema
    {

        #region Constructors

        /// <summary>
        /// Vytvorí schmému indexu typu <see cref="IndexType.Index">IndexType.Index</see> s menom <paramref name="indexName"/>.
        /// </summary>
        /// <param name="indexName">Meno indexu. Musí byť zadané.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="indexName"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="indexName"/> je prázdny reťazec, alebo reťazec bielych
        /// znakov.</exception>
        public IndexSchema(string indexName)
            : this(indexName, IndexType.Index, false)
        {
        }

        /// <summary>
        /// Vytvorí schmému indexu typu <paramref name="indexType"/> s menom <paramref name="indexName"/>.
        /// </summary>
        /// <param name="indexName">Meno indexu. Musí byť zadané.</param>
        /// <param name="indexType">Typ indexu.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="indexName"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="indexName"/> je prázdny reťazec, alebo reťazec bielych
        /// znakov.</exception>
        public IndexSchema(string indexName, IndexType indexType)
            : this(indexName, indexType, false)
        {
        }

        /// <summary>
        /// Vytvorí schmému indexu typu <paramref name="indexType"/> s menom <paramref name="indexName"/>
        /// a nastavením či je <paramref name="clustered"/>.
        /// </summary>
        /// <param name="indexName">Meno indexu. Musí byť zadané.</param>
        /// <param name="indexType">Typ indexu.</param>
        /// <param name="clustered">Určuje, či je index <c>CLUSTERED</c>.</param>
        /// <exception cref="ArgumentNullException">Hodnota <paramref name="indexName"/> je <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Hodnota <paramref name="indexName"/> je prázdny reťazec, alebo reťazec bielych
        /// znakov.</exception>
        public IndexSchema(string indexName, IndexType indexType, bool clustered)
        {
            Check.NotNullOrWhiteSpace(indexName, nameof(indexName));

            Name = indexName;
            IndexType = indexType;
            Clustered = clustered;
            Columns = new IndexColumnSchemaCollection(this);
        }

        #endregion


        #region Common

        private string _name;

        /// <summary>
        /// Meno indexu. Ak index už patrí nejakej tabuľke (hodnota <see cref="Table"/> je nastavená), meno indexu nie je možné
        /// zmeniť. V takom prípade je vyvolaná výnimka <see cref="InvalidOperationException"/>.
        /// </summary>
        public string Name
        {
            get {
                return _name;
            }
            set {
                if (Table != null)
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.IndexSchema_CannotChangeIndexNameWhenBelongsToTable, Table.Name));
                }
                _name = Check.NotNullOrWhiteSpace(value, nameof(value));
            }
        }

        /// <summary>
        /// Typ indexu.
        /// </summary>
        public IndexType IndexType { get; }

        /// <summary>
        /// Určuje, či je index <c>CLUSTERED</c>.
        /// </summary>
        bool Clustered { get; set; }

        /// <summary>
        /// Zoznam stĺpcov indexu.
        /// </summary>
        public IndexColumnSchemaCollection Columns { get; }

        /// <summary>
        /// Tabuľka, ktorej index patrí.
        /// </summary>
        public TableSchema Table { get; internal set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(50);

            sb.AppendFormat("Index {0}", Name);
            if ((IndexType != IndexType.Index) || Clustered)
            {
                sb.Append(" (");
                switch (IndexType)
                {
                    case IndexType.PrimaryKey:
                        sb.Append("primary key");
                        break;

                    case IndexType.UniqueKey:
                        sb.Append("unique");
                        break;
                }

                if (Clustered)
                {
                    if (IndexType != IndexType.Index)
                    {
                        sb.Append(", ");
                    }
                    sb.Append("clustered");
                }
                sb.Append(")");
            }
            sb.Append(": ");

            bool first = true;
            foreach (IndexColumnSchema column in Columns)
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
                if (column.Order == SortOrder.Descending)
                {
                    sb.Append(" DESC");
                }
            }

            return sb.ToString();
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

    }
}
